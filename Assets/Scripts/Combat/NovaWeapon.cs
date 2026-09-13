using UnityEngine;

namespace Veinfire
{
    public sealed class NovaWeapon : MonoBehaviour
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
            _cooldown = (_stats != null ? _stats.ScaledInterval(2.4f) : 2.4f) / (0.85f + Level * 0.08f);
            Pulse();
        }

        void Pulse()
        {
            var radius = (3.2f + Level * 0.45f) * (_stats != null ? _stats.Area : 1f);
            var damage = (_stats != null ? _stats.ScaledDamage : 12f) * (0.9f + Level * 0.2f);
            CombatUtil.DamageEnemiesInRadius(transform.position, radius, damage, 4f, _health, _stats != null ? _stats.LifeSteal : 0f, _stats);
            GameFeel.Pulse(transform.position + Vector3.up * 0.4f);

            var ring = PrimitiveFactory.Sphere("Nova", new Color(1f, 0.3f, 0.55f, 0.35f), 0.4f, true);
            ring.transform.position = transform.position + Vector3.up * 0.4f;
            var body = ring.GetComponent<Rigidbody>();
            body.isKinematic = true;
            Destroy(ring.GetComponent<SphereCollider>());
            ring.AddComponent<NovaBurst>().Play(radius, 0.28f * (_stats != null ? _stats.Duration : 1f));
        }
    }

    public sealed class NovaBurst : MonoBehaviour
    {
        float _life;
        float _target;

        public void Play(float radius, float life)
        {
            _target = radius * 2f;
            _life = life;
        }

        void Update()
        {
            if (GameSession.IsPaused) return;
            _life -= Time.deltaTime;
            var scale = Mathf.Lerp(transform.localScale.x, _target, 1f - Mathf.Exp(-10f * Time.deltaTime));
            transform.localScale = Vector3.one * scale;
            if (_life <= 0f) Destroy(gameObject);
        }
    }
}
