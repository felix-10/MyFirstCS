using UnityEngine;

namespace Veinfire
{
    public sealed class EnemyBolt : MonoBehaviour
    {
        public float Damage = 10f;
        public Vector3 Velocity;
        float _life = 3.5f;
        Transform _player;
        bool _spent;

        void OnEnable()
        {
            _spent = false;
            _life = 3.5f;
            foreach (var col in GetComponents<Collider>())
                col.enabled = false;
        }

        void Update()
        {
            if (_spent || GameSession.IsPaused) return;
            if (_player == null)
            {
                var motor = FindFirstObjectByType<PlayerMotor>();
                if (motor != null) _player = motor.transform;
            }

            var from = transform.position;
            transform.position += Velocity * Time.deltaTime;
            var to = transform.position;
            var delta = to - from;
            var mag = delta.magnitude;
            if (mag > 0.0001f
                && Physics.SphereCast(from, 0.12f, delta / mag, out var wall, mag, ~0, QueryTriggerInteraction.Ignore)
                && World.IsMapSolid(wall.collider))
            {
                Expire(false);
                return;
            }

            if (_player != null && HitsPlayer(from, to, _player.position, 0.34f))
            {
                var health = _player.GetComponent<PlayerHealth>();
                if (health != null) health.Damage(Damage, true);
                Expire(true);
                return;
            }

            _life -= Time.deltaTime;
            if (_life <= 0f) Expire(false);
        }

        static bool HitsPlayer(Vector3 from, Vector3 to, Vector3 player, float radius)
        {
            from.y = 0f;
            to.y = 0f;
            player.y = 0f;
            var ab = to - from;
            var t = 0f;
            if (ab.sqrMagnitude > 0.0001f)
                t = Mathf.Clamp01(Vector3.Dot(player - from, ab) / ab.sqrMagnitude);
            var closest = from + ab * t;
            return (player - closest).sqrMagnitude <= radius * radius;
        }

        void Expire(bool hitPlayer)
        {
            if (_spent) return;
            _spent = true;
            if (GameSession.Clock >= 90f)
            {
                var at = hitPlayer && _player != null ? _player.position : transform.position;
                MapHazard.SpawnPuddle(at, 2.5f, 10f, 2.1f);
            }

            Destroy(gameObject);
        }
    }
}
