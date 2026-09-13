using UnityEngine;

namespace Veinfire
{
    public sealed class EnemyRanged : MonoBehaviour
    {
        public EnemyBolt BoltPrefab;
        public float Interval = 2.6f;
        public float Damage = 7f;
        Transform _player;
        float _cd;

        void Start()
        {
            var motor = FindFirstObjectByType<PlayerMotor>();
            if (motor != null) _player = motor.transform;
        }

        void Update()
        {
            if (GameSession.IsPaused || _player == null || BoltPrefab == null) return;
            _cd -= Time.deltaTime;
            if (_cd > 0f) return;
            var delta = World.Planar(transform.position, _player.position);
            if (delta.sqrMagnitude > 14f * 14f) return;
            _cd = Interval;
            var origin = transform.position + Vector3.up * 0.9f + delta.normalized * 0.8f;
            var bolt = Instantiate(BoltPrefab, origin, Quaternion.LookRotation(delta.normalized, Vector3.up));
            bolt.gameObject.SetActive(true);
            bolt.Damage = Damage;
            bolt.Velocity = delta.normalized * 7.2f;
        }
    }
}
