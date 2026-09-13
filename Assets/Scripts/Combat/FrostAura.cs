using UnityEngine;

namespace Veinfire
{
    public sealed class FrostAura : MonoBehaviour
    {
        public int Level = 1;
        PlayerCombatStats _stats;
        SphereCollider _zone;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            var zone = new GameObject("FrostZone");
            zone.transform.SetParent(transform, false);
            zone.transform.localPosition = Vector3.up * 0.2f;
            _zone = zone.AddComponent<SphereCollider>();
            _zone.isTrigger = true;
            zone.AddComponent<FrostZoneHit>().Aura = this;
            var vis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vis.name = "FrostRing";
            vis.transform.SetParent(zone.transform, false);
            vis.transform.localScale = new Vector3(5.2f, 0.03f, 5.2f);
            PrimitiveFactory.Paint(vis, new Color(0.55f, 0.88f, 1f, 0.35f));
            var col = vis.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        void Update()
        {
            if (_zone == null) return;
            var r = (2.6f + Level * 0.35f) * (_stats != null ? _stats.Area : 1f);
            _zone.radius = r;
            var vis = _zone.transform.Find("FrostRing");
            if (vis != null) vis.localScale = new Vector3(r * 2f, 0.03f, r * 2f);
        }

        public float SlowMul => Mathf.Clamp(0.72f - Level * 0.06f, 0.35f, 0.8f);
        public float Damage => (_stats != null ? _stats.ScaledDamage : 8f) * (0.12f + Level * 0.03f);
        public PlayerCombatStats Stats => _stats;
    }

    public sealed class FrostZoneHit : MonoBehaviour
    {
        public FrostAura Aura;

        void OnTriggerStay(Collider other)
        {
            if (GameSession.IsPaused || Aura == null) return;
            var enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy == null) return;
            enemy.ApplySlow(Aura.SlowMul, 0.25f);
            if (!enemy.TryDot(0.4f)) return;
            var crit = false;
            var damage = Aura.Stats != null ? Aura.Stats.RollHit(Aura.Damage, out crit) : Aura.Damage;
            enemy.Damage(damage, transform.position, 0.4f, crit);
        }
    }
}
