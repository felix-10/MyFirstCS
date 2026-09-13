using UnityEngine;

namespace Veinfire
{
    public sealed class TemorisSword : MonoBehaviour
    {
        public int Level = 1;
        PlayerCombatStats _stats;
        PlayerHealth _health;
        PlayerDash _dash;
        float _cooldown;
        int _frenzy;
        float _frenzyUntil;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            _health = GetComponent<PlayerHealth>();
            _dash = GetComponent<PlayerDash>();
        }

        void Update()
        {
            if (!enabled || GameSession.IsPaused || _health == null || _health.IsDead) return;
            if (Time.time >= _frenzyUntil)
            {
                _frenzy = 0;
                if (_stats != null)
                {
                    _stats.FrenzyMove = 0f;
                    _stats.FrenzyDamage = 0f;
                }
            }

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;
            var target = CombatUtil.UniqueTarget(transform.position, RunRules.UniqueRange(UniqueId.Blade));
            if (target == null) return;
            var face = World.Planar(transform.position, target.position);
            var rot = face.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(face, Vector3.up)
                : transform.rotation;
            _cooldown = _stats != null ? _stats.ScaledInterval(0.22f) : 0.22f;
            var build = UniqueBuild.Of(this);
            var path = build != null ? build.Path : 1;
            if (path == 2) _cooldown *= 0.78f;
            if (path == 3) _cooldown *= 1.15f;
            Swing(rot);
        }

        void Swing(Quaternion rot)
        {
            var range = (2.85f + Level * 0.22f) * (_stats != null ? _stats.Area : 1f);
            var width = (1.55f + Level * 0.12f) * (_stats != null ? _stats.Area : 1f);
            var build = UniqueBuild.Of(this);
            var path = build != null ? build.Path : 1;
            var evo = build != null ? build.Evolution : 1;
            if (path == 1) { range *= 1.18f; width *= 1.12f; }
            if (path == 3) range *= 0.92f;
            var origin = transform.position + Vector3.up * 0.75f + rot * Vector3.forward * (range * 0.5f);
            var damage = (_stats != null ? _stats.ScaledDamage : 14f) * (0.95f + Level * 0.08f);
            if (path == 3) damage *= 1.28f;
            if (Level >= 5 && evo == 3) damage *= 1.15f;
            var seen = new System.Collections.Generic.HashSet<int>();
            var hits = 0;
            var fan = Level >= 5 && evo != 2;
            if (Level >= 5 && evo == 2) { range *= 1.35f; width *= 1.4f; fan = true; }
            Collider[] cols;
            if (fan)
                cols = Physics.OverlapSphere(transform.position + Vector3.up * 0.7f, range, ~0, QueryTriggerInteraction.Collide);
            else
                cols = Physics.OverlapBox(origin, new Vector3(width * 0.5f, 1.1f, range * 0.5f), rot, ~0, QueryTriggerInteraction.Collide);

            var forward = rot * Vector3.forward;
            for (var i = 0; i < cols.Length; i++)
            {
                var enemy = cols[i].GetComponentInParent<EnemyHealth>();
                if (enemy == null || !seen.Add(enemy.GetInstanceID())) continue;
                if (Level >= 5)
                {
                    var to = World.Planar(transform.position, enemy.transform.position);
                    if (to.magnitude > range + 0.4f) continue;
                    if (Vector3.Angle(forward, to) > 70f) continue;
                }

                var crit = false;
                var hit = _stats != null ? _stats.RollHit(damage, out crit) : damage;
                enemy.Damage(hit, transform.position, 3.2f, crit, _stats != null, HitKind.Normal, true, false);
                if (_stats != null && _stats.LifeSteal > 0f)
                    _health?.Heal(_stats.LifeSteal);
                if (Level >= 5 && evo == 3) _health?.Heal(Mathf.Max(1.2f, damage * 0.08f), false);
                hits++;
            }

            if (Level >= 5 && evo == 1 && hits > 0)
            {
                _frenzy = Mathf.Min(5, _frenzy + 1);
                _frenzyUntil = Time.time + 3f;
                if (_stats != null)
                {
                    _stats.FrenzyDamage = _frenzy * 0.08f;
                    _stats.FrenzyMove = _frenzy * 0.04f;
                }

                _dash?.ShortenCooldown(Mathf.Min(0.36f, hits * 0.12f));
            }

            GameFeel.Shoot(origin);
            ShotFx.Flash("Slash", origin, new Color(0.92f, 0.94f, 1f), new Vector3(width * 0.28f, 0.07f, range), rot, 0.09f, PrimitiveType.Cube);
            ShotFx.Flash("SlashArc", origin + rot * Vector3.right * (width * 0.22f), new Color(0.7f, 0.78f, 0.95f), new Vector3(width * 0.9f, 0.04f, range * 0.55f), rot * Quaternion.Euler(0f, 18f, 0f), 0.08f, PrimitiveType.Cube);
        }
    }
}
