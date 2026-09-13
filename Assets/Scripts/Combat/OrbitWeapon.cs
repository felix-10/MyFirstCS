using UnityEngine;

namespace Veinfire
{
    public sealed class OrbitWeapon : MonoBehaviour
    {
        public int Level = 1;
        public int Fangs = 2;
        Transform _root;
        PlayerCombatStats _stats;
        PlayerHealth _health;
        float _angle;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            _health = GetComponent<PlayerHealth>();
            Rebuild();
        }

        public void Rebuild()
        {
            if (_root != null) Destroy(_root.gameObject);
            _root = new GameObject("OrbitFangs").transform;
            _root.SetParent(transform, false);
            var count = Fangs + Level - 1;
            var radius = (2.1f + Level * 0.18f) * (_stats != null ? _stats.Area : 1f);
            for (var i = 0; i < count; i++)
            {
                var fang = PrimitiveFactory.Sphere("Fang", new Color(0.95f, 0.32f, 0.08f), 0.32f, true);
                fang.transform.localScale = new Vector3(0.22f, 0.18f, 0.55f);
                fang.transform.SetParent(_root, false);
                PrimitiveFactory.StripRigidbody(fang);
                var blade = fang.AddComponent<OrbitBlade>();
                blade.Owner = this;
                var t = (Mathf.PI * 2f) * i / count;
                fang.transform.localPosition = new Vector3(Mathf.Cos(t) * radius, 0.7f, Mathf.Sin(t) * radius);
                fang.transform.localRotation = Quaternion.LookRotation(new Vector3(-Mathf.Sin(t), 0f, Mathf.Cos(t)), Vector3.up);
            }
        }

        void Update()
        {
            if (_root == null || GameSession.IsPaused) return;
            var spin = (90f + Level * 18f) * Time.deltaTime;
            _angle += spin;
            _root.localRotation = Quaternion.Euler(0f, _angle, 0f);
        }

        public float HitDamage => (_stats != null ? _stats.ScaledDamage : 12f) * (0.55f + Level * 0.12f);
        public float HitKnock => _stats != null ? _stats.Knockback * 0.6f : 1.5f;
        public PlayerCombatStats Stats => _stats;
        public PlayerHealth OwnerHealth => _health;
        public float LifeSteal => _stats != null ? _stats.LifeSteal * 0.35f : 0f;

        void OnDestroy()
        {
            if (_root != null) Destroy(_root.gameObject);
        }
    }

    public sealed class OrbitBlade : MonoBehaviour
    {
        public OrbitWeapon Owner;

        void OnTriggerStay(Collider other)
        {
            if (GameSession.IsPaused || Owner == null) return;
            var enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy == null || !enemy.TryDot(0.22f)) return;
            var crit = false;
            var damage = Owner.Stats != null ? Owner.Stats.RollHit(Owner.HitDamage, out crit) : Owner.HitDamage;
            enemy.Damage(damage, transform.position, Owner.HitKnock, crit);
            if (Owner.LifeSteal > 0f)
                Owner.OwnerHealth?.Heal(Owner.LifeSteal);
        }
    }
}
