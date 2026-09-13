using UnityEngine;

namespace Veinfire
{
    public enum PickupKind
    {
        Health,
        Magnet,
        Bomb,
        Chest
    }

    public sealed class Pickup : MonoBehaviour
    {
        public PickupKind Kind = PickupKind.Health;
        Transform _player;
        PlayerCombatStats _stats;
        PlayerHealth _health;
        float _spin;

        void Start()
        {
            var motor = FindFirstObjectByType<PlayerMotor>();
            if (motor != null)
            {
                _player = motor.transform;
                _stats = motor.GetComponent<PlayerCombatStats>();
                _health = motor.GetComponent<PlayerHealth>();
            }

            ApplyLook();
        }

        public void ApplyLook()
        {
            Color color;
            float scale;
            switch (Kind)
            {
                case PickupKind.Health:
                    color = new Color(1f, 0.22f, 0.32f);
                    scale = 0.52f;
                    break;
                case PickupKind.Magnet:
                    color = new Color(0.95f, 0.2f, 0.85f);
                    scale = 0.4f;
                    break;
                case PickupKind.Bomb:
                    color = new Color(1f, 0.55f, 0.1f);
                    scale = 0.48f;
                    break;
                default:
                    color = new Color(1f, 0.84f, 0.2f);
                    scale = 0.55f;
                    break;
            }

            transform.localScale = Vector3.one * scale;
            PrimitiveFactory.Paint(gameObject, color);
        }

        void Update()
        {
            if (GameSession.IsPaused || _player == null) return;
            _spin += 120f * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, _spin, 0f);
            var delta = World.Planar(transform.position, _player.position);
            var reach = _stats != null ? _stats.PickupRadius : 1.6f;
            var pull = Kind == PickupKind.Health ? Mathf.Min(1.1f, reach * 0.5f) : reach * 1.6f;
            if (delta.magnitude < 0.55f)
            {
                Collect();
                return;
            }

            if (delta.magnitude < pull)
                transform.position += delta.normalized * (10f * Time.deltaTime);
        }

        void Collect()
        {
            switch (Kind)
            {
                case PickupKind.Health:
                    _health?.Heal(28f);
                    _player.GetComponent<HeroPassives>()?.OnHealthPickup();
                    break;
                case PickupKind.Magnet:
                    PullAllXp();
                    break;
                case PickupKind.Bomb:
                    CombatUtil.DamageEnemiesInRadius(_player.position, 7.5f, 80f, 8f);
                    GameFeel.Explosion(_player.position);
                    break;
                case PickupKind.Chest:
                    FindFirstObjectByType<LevelDirector>()?.OfferChest();
                    break;
            }

            if (Kind != PickupKind.Bomb) GameFeel.Pickup();
            Destroy(gameObject);
        }

        static void PullAllXp()
        {
            var orbs = FindObjectsByType<XpOrb>(FindObjectsSortMode.None);
            var player = FindFirstObjectByType<PlayerMotor>();
            if (player == null) return;
            for (var i = 0; i < orbs.Length; i++)
            {
                if (orbs[i] == null) continue;
                orbs[i].transform.position = Vector3.MoveTowards(orbs[i].transform.position, player.transform.position, 0.1f);
                var levels = FindFirstObjectByType<LevelDirector>();
                levels?.AddXp(orbs[i].Amount);
                Destroy(orbs[i].gameObject);
            }
        }
    }
}
