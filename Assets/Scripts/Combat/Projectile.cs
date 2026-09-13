using System.Collections.Generic;
using UnityEngine;

namespace Veinfire
{
    public sealed class Projectile : MonoBehaviour
    {
        public float Damage;
        public float LifeSteal;
        public float Knockback = 2.2f;
        public int Pierce;
        public Vector3 Velocity;
        public PlayerCombatStats OwnerStats;
        public PlayerHealth OwnerHealth;
        public bool SplitOnHit;
        public bool ProcVein = true;
        public bool IsBullet;
        public float Duration = 1.2f;
        public Vector3 Axis;
        public float Helix;
        public float HelixSpin = 520f;
        public bool ReturnShot;
        public float ReturnAfter = 8.5f;
        public float HitRadius = 0.55f;
        public HitKind Kind = HitKind.Normal;
        public float IgniteTime;
        public float SlowTime;
        public float SlowMul = 0.72f;
        public float HomingTurn;
        public int Bounces;
        public int WallBounce;
        public float ExplodeRadius;
        public int SplitCount = 2;
        public float Pull;
        public float ZoneLife;
        public Color ZoneTint = new Color(0.4f, 0.9f, 0.25f, 0.5f);

        readonly HashSet<int> _hit = new HashSet<int>();
        float _life = 1.6f;
        bool _spent;
        Vector3 _lastPos;
        Vector3 _start;
        float _travel;
        float _spin;
        bool _returned;

        void OnEnable() => Begin();

        public void Begin()
        {
            _lastPos = transform.position;
            _start = transform.position;
            _hit.Clear();
            _spent = false;
            _returned = false;
            _travel = 0f;
            _spin = Random.value * 360f;
            _life = Duration;
            if (Axis.sqrMagnitude < 0.01f && Velocity.sqrMagnitude > 0.01f)
                Axis = Velocity;
        }

        void Update()
        {
            if (GameSession.IsPaused || _spent) return;
            _lastPos = transform.position;
            var dt = Time.deltaTime;
            var speed = Velocity.magnitude;
            if (speed < 0.01f) speed = 11f;
            Steer(dt, speed);
            var axis = Axis.sqrMagnitude > 0.01f ? Axis : Velocity;
            axis.y = 0f;
            if (axis.sqrMagnitude < 0.01f) axis = transform.forward;
            axis.Normalize();

            if (Helix > 0.02f)
            {
                _travel += speed * dt;
                if (ReturnShot && !_returned && _travel >= ReturnAfter)
                {
                    Reverse(axis, speed);
                    axis = Axis.sqrMagnitude > 0.01f ? Axis.normalized : -axis;
                }

                _spin += HelixSpin * dt;
                var radial = Vector3.Cross(Vector3.up, axis);
                if (radial.sqrMagnitude < 0.01f) radial = Vector3.right;
                radial.Normalize();
                var offset = Quaternion.AngleAxis(_spin, axis) * radial * Helix;
                transform.position = _start + axis * _travel + offset;
                if (axis.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(axis, Vector3.up) * Quaternion.Euler(0f, 0f, _spin);
            }
            else
            {
                if (ReturnShot && !_returned)
                {
                    _travel += speed * dt;
                    if (_travel >= ReturnAfter) Reverse(axis, speed);
                }

                transform.position += Velocity * dt;
            }

            HitAlongPath(_lastPos, transform.position);
            _life -= dt;
            if (_life <= 0f) Spend(true);
        }

        void Steer(float dt, float speed)
        {
            if (HomingTurn <= 0.01f) return;
            var focus = CombatUtil.UniqueTarget(transform.position, 16f);
            if (focus == null) return;
            var want = World.Planar(transform.position, focus.position);
            if (want.sqrMagnitude < 0.01f) return;
            want.Normalize();
            var vel = Velocity.sqrMagnitude > 0.01f ? Velocity.normalized : want;
            var steered = Vector3.RotateTowards(vel, want, HomingTurn * Mathf.Deg2Rad * dt, 0f);
            Velocity = steered * speed;
            Axis = Velocity;
        }

        void Reverse(Vector3 axis, float speed)
        {
            _returned = true;
            axis = -axis;
            Axis = axis;
            Velocity = -axis * speed;
            Damage *= 1.3f;
            _start = transform.position;
            _travel = 0f;
        }

        void OnTriggerEnter(Collider other) => TryHitEnemy(other.GetComponentInParent<EnemyHealth>());

        void OnTriggerStay(Collider other) => TryHitEnemy(other.GetComponentInParent<EnemyHealth>());

        void HitAlongPath(Vector3 from, Vector3 to)
        {
            var radius = HitRadius;
            if ((to - from).sqrMagnitude < 0.0001f)
                to = from + (Velocity.sqrMagnitude > 0.01f ? Velocity.normalized : transform.forward) * 0.08f;
            var delta = to - from;
            var mag = delta.magnitude;
            if (mag > 0.0001f
                && Physics.SphereCast(from, 0.18f, delta / mag, out var wall, mag, ~0, QueryTriggerInteraction.Ignore)
                && World.IsMapSolid(wall.collider))
            {
                if (WallBounce > 0)
                {
                    WallBounce -= 1;
                    var bounce = Vector3.Reflect(delta / mag, wall.normal);
                    bounce.y = 0f;
                    if (bounce.sqrMagnitude < 0.01f) bounce = -delta;
                    bounce.Normalize();
                    var speed = Mathf.Max(8f, Velocity.magnitude);
                    Velocity = bounce * speed;
                    Axis = Velocity;
                    _start = transform.position;
                    _travel = 0f;
                    transform.position = wall.point + bounce * 0.2f;
                    return;
                }

                Spend(true);
                return;
            }

            var hits = Physics.OverlapCapsule(from, to, radius, ~0, QueryTriggerInteraction.Collide);
            for (var i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null) continue;
                TryHitEnemy(hits[i].GetComponentInParent<EnemyHealth>());
            }
        }

        void TryHitEnemy(EnemyHealth enemy)
        {
            if (_spent || enemy == null) return;
            if (!_hit.Add(enemy.GetInstanceID())) return;

            var damage = Damage;
            var crit = false;
            if (OwnerStats != null)
                damage = OwnerStats.RollHit(Damage, out crit);
            enemy.Damage(damage, transform.position, Knockback, crit, ProcVein && OwnerStats != null, Kind, false, IsBullet);
            if (IgniteTime > 0f) enemy.Ignite(IgniteTime);
            if (SlowTime > 0f) enemy.ApplySlow(SlowMul, SlowTime);
            if (Pull > 0f && OwnerHealth != null) enemy.PullToward(OwnerHealth.transform.position, Pull);
            if (OwnerHealth != null && LifeSteal > 0f)
                OwnerHealth.Heal(LifeSteal);
            if (SplitOnHit)
                Split(enemy.transform.position, Velocity);

            if (Bounces > 0)
            {
                Bounces -= 1;
                if (Jump(enemy)) return;
            }

            if (Pierce <= 0)
            {
                Spend(ExplodeRadius > 0.1f);
                return;
            }

            Pierce -= 1;
        }

        bool Jump(EnemyHealth justHit)
        {
            var next = CombatUtil.UniqueTarget(transform.position, 7.5f, justHit);
            if (next == null) return false;
            var dir = World.Planar(transform.position, next.position);
            if (dir.sqrMagnitude < 0.01f) return false;
            dir.Normalize();
            var speed = Mathf.Max(9f, Velocity.magnitude);
            Velocity = dir * speed;
            Axis = Velocity;
            _start = transform.position;
            _travel = 0f;
            Helix = 0f;
            return true;
        }

        void Split(Vector3 at, Vector3 vel)
        {
            SplitOnHit = false;
            var dir = vel.sqrMagnitude > 0.01f ? vel.normalized : transform.forward;
            var n = Mathf.Max(2, SplitCount);
            var spread = Mathf.Min(70f, 12f * n);
            for (var i = 0; i < n; i++)
            {
                var yaw = n == 1 ? 0f : Mathf.Lerp(-spread * 0.5f, spread * 0.5f, i / (n - 1f));
                SpawnShard(at, Quaternion.Euler(0f, yaw, 0f) * dir);
            }
        }

        void SpawnShard(Vector3 at, Vector3 dir)
        {
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            var shard = Instantiate(this, at + Vector3.up * 0.2f, Quaternion.LookRotation(dir, Vector3.up));
            shard.gameObject.SetActive(true);
            shard.Damage = Damage * 0.4f;
            shard.Pierce = 0;
            shard.SplitOnHit = false;
            shard.ProcVein = false;
            shard.IsBullet = false;
            shard.OwnerStats = null;
            shard.OwnerHealth = OwnerHealth;
            shard.LifeSteal = 0f;
            shard.Velocity = dir.normalized * (Mathf.Max(6f, Velocity.magnitude) * 0.85f);
            shard.Axis = shard.Velocity;
            shard.Helix = 0f;
            shard.ReturnShot = false;
            shard.HomingTurn = 0f;
            shard.Bounces = 0;
            shard.WallBounce = 0;
            shard.ExplodeRadius = 0f;
            shard.ZoneLife = 0f;
            shard.Duration = 0.55f;
            shard.Begin();
        }

        void Spend(bool boom)
        {
            if (_spent) return;
            _spent = true;
            if (boom) Detonate();
            Destroy(gameObject);
        }

        void Detonate()
        {
            if (ExplodeRadius > 0.1f)
            {
                CombatUtil.DamageEnemiesInRadius(transform.position, ExplodeRadius, Damage * 0.85f, Knockback + 1.2f,
                    OwnerHealth, LifeSteal, ProcVein ? OwnerStats : null);
                ShotFx.Ring(transform.position, ZoneTint, ExplodeRadius * 2f, 0.16f);
            }

            if (ZoneLife <= 0.05f) return;
            var zone = new GameObject("ShotZone");
            zone.transform.position = transform.position + Vector3.up * 0.15f;
            var field = zone.AddComponent<StrikeZone>();
            field.Radius = Mathf.Max(1.2f, ExplodeRadius > 0.1f ? ExplodeRadius : 1.6f);
            field.TickDamage = Damage * 0.32f;
            field.Interval = 0.4f;
            field.Life = ZoneLife;
            field.Vein = ProcVein;
            field.OwnerHealth = OwnerHealth;
            field.OwnerStats = OwnerStats;
            field.Tint = ZoneTint;
            field.Burn = IgniteTime;
            field.Plague = Kind == HitKind.Plague;
        }
    }
}
