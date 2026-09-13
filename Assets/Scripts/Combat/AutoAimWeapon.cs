using UnityEngine;

namespace Veinfire
{
    public sealed class AutoAimWeapon : MonoBehaviour
    {
        public int Level = 1;
        public Projectile ProjectilePrefab;
        public Transform Muzzle;
        PlayerCombatStats _stats;
        PlayerHealth _health;
        float _cooldown;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            _health = GetComponent<PlayerHealth>();
        }

        void Update()
        {
            if (!enabled || GameSession.IsPaused || _health == null || _health.IsDead) return;
            if (ProjectilePrefab == null) return;
            _cooldown -= Time.unscaledDeltaTime;
            if (_cooldown > 0f) return;

            var target = CombatUtil.UniqueTarget(transform.position, RunRules.UniqueRange(UniqueId.Hymn));
            if (target == null) return;

            _cooldown = _stats.ScaledInterval(_stats.FireInterval);
            FireAt(target.position, Level >= 5);
        }

        int ShotCount
        {
            get
            {
                var b = UniqueBuild.Of(this);
                var path = b != null ? b.Path : 1;
                var evo = b != null ? b.Evolution : 1;
                if (path == 2) return Mathf.Max(1, 1 + Level / 2);
                var n = Level + (path == 1 ? 1 : 0);
                if (Level >= 5 && evo == 3) n += 3;
                return Mathf.Max(1, n);
            }
        }

        void FireAt(Vector3 target, bool split = false)
        {
            var origin = Muzzle != null ? Muzzle.position : transform.position + Vector3.up * 0.9f;
            var planar = World.Planar(origin, target);
            var dist = planar.magnitude;
            var dir = dist > 0.01f ? planar / dist : transform.forward;
            dir.y = 0f;
            dir.Normalize();
            var build = UniqueBuild.Of(this);
            var path = build != null ? build.Path : 1;
            var evo = build != null ? build.Evolution : 1;
            var count = ShotCount;
            var spread = path == 1 || (Level >= 5 && evo == 3) ? 16f : 7f;
            var start = count == 1 ? 0f : -spread * 0.5f;
            var step = count == 1 ? 0f : spread / (count - 1);
            var size = _stats.ProjectileSize * _stats.Area * (path == 2 ? 0.85f : 1f);
            var side = Vector3.Cross(Vector3.up, dir).normalized;
            var spawnDist = Mathf.Clamp(dist - 0.75f, 0.12f, 0.35f);
            var doSplit = split && evo == 1;
            var ignite = path == 3 || (Level >= 5 && evo == 2);

            for (var i = 0; i < count; i++)
            {
                var angle = start + step * i;
                var shotDir = Quaternion.Euler(0f, angle, 0f) * dir;
                var spawn = origin + shotDir * spawnDist + side * ((i - (count - 1) * 0.5f) * 0.18f);
                var rot = shotDir.sqrMagnitude > 0.001f
                    ? Quaternion.LookRotation(shotDir, Vector3.up)
                    : Quaternion.identity;
                var projectile = Instantiate(ProjectilePrefab, spawn, rot);
                if (projectile == null) continue;
                projectile.gameObject.SetActive(true);
                ShotFx.Hymn(projectile.gameObject, size);
                projectile.Damage = _stats.ScaledDamage * (path == 2 ? 1.12f : 1f);
                projectile.LifeSteal = _stats.LifeSteal;
                projectile.Knockback = _stats.Knockback;
                projectile.Pierce = _stats.Pierce + (path == 2 ? 2 + Level / 2 : (Level >= 2 ? 1 : 0));
                if (Level >= 5 && evo == 2) projectile.Pierce += 3;
                projectile.OwnerHealth = _health;
                projectile.OwnerStats = _stats;
                projectile.SplitOnHit = doSplit;
                projectile.IgniteTime = ignite ? 1.6f : 0f;
                projectile.Kind = ignite ? HitKind.Burn : HitKind.Normal;
                projectile.ProcVein = true;
                projectile.IsBullet = true;
                projectile.Helix = 0f;
                projectile.ReturnShot = false;
                projectile.Duration = 1.05f;
                projectile.HitRadius = 0.42f;
                projectile.Velocity = shotDir * (_stats.ProjectileSpeed * (1f + Level * 0.04f) * (path == 2 ? 1.2f : 1f));
                projectile.Axis = shotDir;
                projectile.Begin();
            }

            GameFeel.Shoot(origin);
        }
    }
}
