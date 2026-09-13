using UnityEngine;

namespace Veinfire
{
    public sealed class MapHazard : MonoBehaviour
    {
        public float DamagePerSecond = 16f;
        public float Life;
        PlayerHealth _player;
        float _tick;
        Collider _col;
        float _lifeLeft;

        public static void SpawnPuddle(Vector3 pos, float seconds, float dps = 10f, float size = 2.2f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "MapHazard";
            go.transform.position = new Vector3(pos.x, 0.12f, pos.z);
            go.transform.localScale = new Vector3(size, 0.22f, size);
            PrimitiveFactory.Paint(go, new Color(0.35f, 0.95f, 0.28f, 0.7f));
            var col = go.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            var hazard = go.AddComponent<MapHazard>();
            hazard.DamagePerSecond = dps;
            hazard._lifeLeft = seconds;
            Destroy(go, seconds + 0.3f);
        }

        void Awake()
        {
            _col = GetComponent<Collider>();
        }

        void Update()
        {
            if (GameSession.IsPaused) return;
            if (_lifeLeft > 0f)
            {
                _lifeLeft -= Time.deltaTime;
                if (_lifeLeft <= 0f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            if (_player == null)
            {
                var motor = FindFirstObjectByType<PlayerMotor>();
                if (motor != null) _player = motor.GetComponent<PlayerHealth>();
            }

            if (_player == null || _player.IsDead) return;
            var reach = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) * 0.5f + 0.35f;
            if (World.Planar(transform.position, _player.transform.position).magnitude > reach) return;

            _tick += Time.deltaTime;
            if (_tick < 0.18f) return;
            var stats = _player.GetComponent<PlayerCombatStats>();
            var resist = stats != null ? Mathf.Clamp(stats.HazardResist, 0f, 0.7f) : 0f;
            _player.Damage(DamagePerSecond * _tick * (1f - resist));
            _tick = 0f;
        }
    }
}
