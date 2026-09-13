using UnityEngine;

namespace Veinfire
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class EnemyChase : MonoBehaviour
    {
        public float Speed = 2.4f;
        public bool KeepDistance;
        public float PreferredRange = 9f;
        Transform _player;
        Rigidbody _body;
        EnemyHealth _health;
        float _lungeCd;
        float _lungeLeft;

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _health = GetComponent<EnemyHealth>();
            World.PrepareCharacterBody(_body);
            World.MakeTriggerVolume(gameObject);
        }

        void FixedUpdate()
        {
            if (_player == null)
            {
                var motor = FindFirstObjectByType<PlayerMotor>();
                if (motor != null) _player = motor.transform;
            }

            if (GameSession.IsPaused || _player == null)
                return;

            var delta = World.Planar(_body.position, _player.position);
            var dist = delta.magnitude;
            var knock = _health != null ? _health.ConsumeKnock() : Vector3.zero;
            var mul = _health != null ? _health.MoveMul : 1f;
            var speed = Speed * mul;
            if (_health != null && _health.Kind == EnemyKind.Runner && GameSession.Clock >= 60f && !KeepDistance)
            {
                _lungeCd -= Time.fixedDeltaTime;
                if (_lungeLeft > 0f)
                {
                    _lungeLeft -= Time.fixedDeltaTime;
                    speed *= 2.35f;
                }
                else if (_lungeCd <= 0f && dist > 2.4f)
                {
                    _lungeCd = 4.5f;
                    _lungeLeft = 0.28f;
                    speed *= 2.35f;
                }
            }

            var stopAt = World.BodyRadius(transform) + World.BodyRadius(_player) * 0.35f + 0.06f;

            if (KeepDistance)
            {
                if (dist < 0.0001f)
                {
                    World.MovePlanar(_body, -transform.forward * speed + knock);
                    return;
                }

                var dir = delta / dist;
                if (dist < PreferredRange - 1.2f) dir = -dir;
                else if (dist < PreferredRange + 1.2f)
                {
                    World.MovePlanar(_body, knock);
                    LookAtPlayer();
                    return;
                }

                World.MovePlanar(_body, dir * speed + knock);
                LookAtPlayer();
                return;
            }

            if (dist < stopAt)
            {
                if (dist < 0.12f)
                {
                    var away = delta.sqrMagnitude > 0.001f ? -delta.normalized : -transform.forward;
                    World.MovePlanar(_body, away * speed + knock);
                }
                else
                {
                    World.MovePlanar(_body, knock);
                }

                LookAtPlayer();
                return;
            }

            if (dist < 0.0001f) return;
            World.MovePlanar(_body, delta.normalized * speed + knock);
            LookAtPlayer();
        }

        void LookAtPlayer()
        {
            var look = World.Planar(_body.position, _player.position);
            if (look.sqrMagnitude > 0.01f)
                _body.MoveRotation(Quaternion.LookRotation(look.normalized, Vector3.up));
        }
    }
}
