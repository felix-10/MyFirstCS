using UnityEngine;

namespace Veinfire
{
    public sealed class SpiralHuntWeapon : MonoBehaviour
    {
        public int Level = 1;
        float _cd;
        PlayerCombatStats _stats;
        PlayerHealth _hp;
        Projectile _prefab;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            _hp = GetComponent<PlayerHealth>();
            var needle = GetComponent<AutoAimWeapon>();
            if (needle != null) _prefab = needle.ProjectilePrefab;
        }

        void Update()
        {
            if (!enabled || GameSession.IsPaused || _hp != null && _hp.IsDead) return;
            _cd -= Time.deltaTime;
            if (_cd > 0f) return;
            var target = CombatUtil.UniqueTarget(transform.position, RunRules.UniqueRange(UniqueId.Spiral));
            if (target == null) return;
            _cd = _stats != null ? _stats.ScaledInterval(Mathf.Max(0.22f, 0.36f - Level * 0.02f)) : 0.34f;
            var dir = World.Planar(transform.position, target.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.Normalize();
            var needle = GetComponent<AutoAimWeapon>();
            if (needle != null && needle.ProjectilePrefab != null) _prefab = needle.ProjectilePrefab;
            if (_prefab == null) return;
            var origin = transform.position + Vector3.up * 0.45f;
            var go = Instantiate(_prefab, origin, Quaternion.LookRotation(dir));
            go.gameObject.SetActive(true);
            var p = go.GetComponent<Projectile>();
            if (p == null) return;
            var size = _stats != null ? _stats.ProjectileSize * _stats.Area : 1f;
            var build = UniqueBuild.Of(this);
            var path = build != null ? build.Path : 1;
            var evo = build != null ? build.Evolution : 1;
            ShotFx.Spiral(go.gameObject, size * (path == 3 ? 1.35f : 1f));
            p.Damage = (_stats != null ? _stats.ScaledDamage : 14f) * (0.9f + 0.08f * Level);
            p.Pierce = (path == 2 ? 5 : 3) + Level;
            p.Knockback = _stats != null ? _stats.Knockback : 2f;
            p.Velocity = dir * (path == 2 ? 14f : 11f);
            p.Axis = dir;
            p.Helix = (path == 1 ? 1.25f : 0.75f) + Level * 0.12f;
            if (path == 3) p.Helix += 0.35f;
            p.HelixSpin = 420f + Level * 50f;
            p.Duration = 2.4f;
            p.HitRadius = path == 3 ? 0.95f : 0.72f;
            p.SplitOnHit = Level >= 5 && evo == 2;
            p.ReturnShot = Level >= 5 && evo == 1;
            p.SlowTime = path == 3 || (Level >= 5 && evo == 3) ? 0.7f : 0f;
            p.SlowMul = 0.7f;
            p.ReturnAfter = 8.2f;
            p.OwnerStats = _stats;
            p.OwnerHealth = _hp;
            p.LifeSteal = _stats != null ? _stats.LifeSteal : 0f;
            p.IsBullet = true;
            p.ProcVein = true;
            p.Begin();
            GameFeel.Shoot(origin);
        }
    }
}
