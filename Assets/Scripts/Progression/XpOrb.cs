using UnityEngine;

namespace Veinfire
{
    public sealed class XpOrb : MonoBehaviour
    {
        public int Amount = 4;
        Transform _player;
        PlayerCombatStats _stats;
        LevelDirector _levels;

        void Start()
        {
            var motor = FindFirstObjectByType<PlayerMotor>();
            if (motor != null)
            {
                _player = motor.transform;
                _stats = motor.GetComponent<PlayerCombatStats>();
            }

            _levels = FindFirstObjectByType<LevelDirector>();
        }

        void Update()
        {
            if (GameSession.IsPaused || _player == null) return;
            var delta = World.Planar(transform.position, _player.position);
            var pickup = _stats != null ? _stats.PickupRadius : 1.5f;
            var dist = delta.magnitude;
            if (dist < 0.45f)
            {
                _levels?.AddXp(Amount);
                if (_stats != null && _stats.SiphonXp)
                    _player.GetComponent<PlayerHealth>()?.Heal(4f);
                GameFeel.Pickup();
                Destroy(gameObject);
                return;
            }

            if (dist < pickup * 2.4f)
            {
                var pull = Mathf.Lerp(4f, 14f, 1f - dist / (pickup * 2.4f));
                var next = transform.position + delta.normalized * (pull * Time.deltaTime);
                next.y = _player.position.y + 0.35f;
                transform.position = next;
            }
        }
    }
}
