using UnityEngine;

namespace Veinfire
{
    public sealed class StarfallInstrument : MonoBehaviour
    {
        public int Level = 1;
        float _cd;
        PlayerCombatStats _stats;
        PlayerHealth _hp;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            _hp = GetComponent<PlayerHealth>();
        }

        void Update()
        {
            if (!enabled || GameSession.IsPaused || _hp != null && _hp.IsDead) return;
            _cd -= Time.deltaTime;
            if (_cd > 0f) return;
            var range = RunRules.UniqueRange(UniqueId.Star);
            var focus = CombatUtil.UniqueTarget(transform.position, range);
            if (focus == null) return;
            _cd = _stats != null ? _stats.ScaledInterval(1.4f) : 1.4f;

            var n = 3 + Level;
            var area = _stats != null ? _stats.Area : 1f;
            var dmg = (_stats != null ? _stats.ScaledDamage : 16f) * (1.15f + 0.12f * Level);
            var radius = 2.1f * area;
            var build = UniqueBuild.Of(this);
            var path = build != null ? build.Path : 1;
            var evo = build != null ? build.Evolution : 1;
            if (path == 1) n += 2;
            if (path == 2) { n = Mathf.Max(2, n - 2); dmg *= 1.35f; radius *= 1.25f; }
            if (Level >= 5 && evo == 2) n += 4;
            var center = focus.position;
            center.y = transform.position.y;

            Strike(center, dmg, radius);
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            var extra = 1;
            for (var i = 0; i < enemies.Length && extra < n; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.transform == focus) continue;
                if (World.Planar(center, enemy.transform.position).sqrMagnitude > (path == 3 ? 16f : 81f)) continue;
                Strike(enemy.transform.position, dmg * 0.85f, radius * 0.9f);
                extra++;
            }

            for (var i = extra; i < n; i++)
            {
                var yaw = i * (360f / n);
                var at = center + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * ((path == 3 ? 0.7f : 1.6f) + i * (path == 3 ? 0.12f : 0.35f));
                Strike(at, dmg * 0.75f, radius * 0.85f);
            }

            if (Level < 5) return;
            if (evo != 2)
            {
                var zone = new GameObject("StarBurn");
                zone.transform.position = center + Vector3.up * 0.2f;
                var field = zone.AddComponent<StrikeZone>();
                field.Radius = 2.4f * area * (evo == 3 ? 1.3f : 1f);
                field.TickDamage = dmg * 0.28f;
                field.Interval = 0.4f;
                field.Life = 2.8f;
                field.Vein = true;
                field.OwnerHealth = _hp;
                field.OwnerStats = _stats;
                field.Tint = new Color(1f, 0.82f, 0.28f, 0.55f);
            }

            if (evo == 2) return;
            var pull = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            var pullForce = evo == 3 ? 4.2f : 2.2f;
            var pullR = evo == 3 ? 64f : 36f;
            for (var i = 0; i < pull.Length; i++)
            {
                if (pull[i] == null) continue;
                if (World.Planar(center, pull[i].transform.position).sqrMagnitude > pullR) continue;
                pull[i].PullToward(center, pullForce);
            }
        }

        void Strike(Vector3 at, float dmg, float radius)
        {
            at.y = transform.position.y;
            CombatUtil.DamageEnemiesInRadius(at, radius, dmg, 2.8f, _hp, _stats != null ? _stats.LifeSteal : 0f, _stats);
            GameFeel.Pulse(at + Vector3.up * 0.4f);
            var gold = new Color(1f, 0.78f, 0.22f);
            var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = "Starfall";
            star.transform.position = at + Vector3.up * 11.5f;
            star.transform.localScale = Vector3.one * 0.55f;
            PrimitiveFactory.Paint(star, gold);
            var col = star.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var fall = star.AddComponent<VfxFall>();
            fall.Target = at + Vector3.up * 0.45f;
            fall.Duration = 0.2f;
            ShotFx.Ring(at, gold, radius * 1.15f, 0.22f);
        }
    }
}
