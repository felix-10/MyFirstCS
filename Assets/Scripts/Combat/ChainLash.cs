using UnityEngine;

namespace Veinfire
{
    public sealed class ChainLash : MonoBehaviour
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
            if (GameSession.IsPaused || _health != null && _health.IsDead) return;
            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;
            var range = WhipRange;
            var target = CombatUtil.UniqueTarget(transform.position, range + 1.2f);
            if (target == null) return;
            _cooldown = _stats != null ? _stats.ScaledInterval(0.7f) : 0.7f;
            var face = World.Planar(transform.position, target.position);
            var rot = face.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(face, Vector3.up)
                : transform.rotation;
            Swing(rot, range);
            if (Level >= 4)
                Swing(rot * Quaternion.Euler(0f, 28f, 0f), range * 0.82f);
        }

        float WhipRange => (3.4f + Level * 0.4f) * (_stats != null ? _stats.Area : 1f);

        void Swing(Quaternion rot, float range)
        {
            var width = 1.1f + Level * 0.15f;
            var origin = transform.position + Vector3.up * 0.8f + rot * Vector3.forward * (range * 0.5f);
            var hits = Physics.OverlapBox(
                origin,
                new Vector3(width, 1.2f, range * 0.5f),
                rot,
                ~0,
                QueryTriggerInteraction.Collide);
            var damage = (_stats != null ? _stats.ScaledDamage : 12f) * (1.1f + Level * 0.15f);
            var seen = new System.Collections.Generic.HashSet<int>();
            for (var i = 0; i < hits.Length; i++)
            {
                var enemy = hits[i].GetComponentInParent<EnemyHealth>();
                if (enemy == null || !seen.Add(enemy.GetInstanceID())) continue;
                var crit = false;
                var hit = _stats != null ? _stats.RollHit(damage, out crit) : damage;
                enemy.Damage(hit, transform.position, FusionCatalog.On("lash_shock") ? 8f : 5f, crit);
                if (FusionCatalog.On("frost_lash") || RunConfig.FusedFrostLash) enemy.ApplySlow(0.62f, 0.6f);
                if (_stats != null && _stats.LifeSteal > 0f)
                    _health?.Heal(_stats.LifeSteal);
            }

            GameFeel.Shoot(origin);
            var hand = transform.position + Vector3.up * 0.95f;
            var tip = hand + rot * Vector3.forward * range;
            ShotFx.Whip(hand, tip, new Color(0.72f, 0.42f, 0.18f), 0.12f);
        }
    }
}
