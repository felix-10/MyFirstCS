using UnityEngine;

namespace Veinfire
{
    public sealed class LightningWeapon : MonoBehaviour
    {
        public int Level = 1;
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
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;
            var target = CombatUtil.UniqueTarget(transform.position, RunRules.UniqueRange(UniqueId.Bolt));
            if (target == null) return;
            var build = UniqueBuild.Of(this);
            var path = build != null ? build.Path : 1;
            var evo = build != null ? build.Evolution : 1;
            var interval = path == 2 ? 0.88f : Level >= 5 ? 0.95f : 1.15f;
            _cooldown = _stats != null ? _stats.ScaledInterval(interval) : interval;
            Strike(target, true);
            var hops = 0;
            if (Level >= 5 && evo == 1) hops = 1;
            if (Level >= 5 && evo == 2) hops = 3;
            var from = target;
            var skip = target.GetComponent<EnemyHealth>();
            for (var h = 0; h < hops; h++)
            {
                var chain = CombatUtil.UniqueTarget(from.position, 4.2f + h * 0.6f, skip);
                if (chain == null) break;
                ShotFx.Lightning(from.position + Vector3.up * 0.95f, chain.position + Vector3.up * 0.95f, Color.white, 0.14f, 8, 0.2f);
                Strike(chain, false, 0.7f - h * 0.08f);
                skip = chain.GetComponent<EnemyHealth>();
                from = chain;
            }
        }

        void Strike(Transform target, bool primary, float scale = 1f)
        {
            var at = target.position;
            at.y = transform.position.y;
            var radius = (2.05f + Level * 0.22f) * (_stats != null ? _stats.Area : 1f);
            var damage = (_stats != null ? _stats.ScaledDamage : 16f) * (2.15f + Level * 0.18f) * scale;
            var build = UniqueBuild.Of(this);
            var path = build != null ? build.Path : 1;
            var evo = build != null ? build.Evolution : 1;
            if (path == 1) radius *= 1.22f;
            if (path == 2) radius *= 0.82f;
            if (path == 3) { radius *= 0.72f; damage *= 1.35f; }
            if (Level >= 5 && evo == 3) radius *= 1.45f;
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            var r2 = radius * radius;
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                if (World.Planar(at, enemy.transform.position).sqrMagnitude > r2) continue;
                var hit = damage;
                if (RunRules.BoltEarth && enemy.PetrifyStacks > 0) hit *= 1.3f;
                var crit = false;
                if (_stats != null) hit = _stats.RollHit(hit, out crit);
                enemy.Damage(hit, at, 3.4f, crit, _stats != null);
                if (_stats != null && _stats.LifeSteal > 0f) _health?.Heal(_stats.LifeSteal);
            }

            GameFeel.Pulse(at + Vector3.up * 0.4f);
            ShotFx.SkyBolt(at, Color.white);

            if (primary && Level >= 5 && evo != 2)
            {
                var zone = new GameObject("StormField");
                zone.transform.position = at + Vector3.up * 0.2f;
                var field = zone.AddComponent<StrikeZone>();
                field.Radius = radius * 0.6f;
                field.TickDamage = damage * 0.35f;
                field.Interval = 0.4f;
                field.Life = 2.6f;
                field.Vein = true;
                field.OwnerHealth = _health;
                field.OwnerStats = _stats;
                field.Tint = new Color(0.4f, 0.8f, 1f, 0.5f);
            }
        }
    }
}
