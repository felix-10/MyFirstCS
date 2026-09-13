using UnityEngine;

namespace Veinfire
{
    public sealed class StrikeZone : MonoBehaviour
    {
        public float Radius = 2f;
        public float TickDamage = 8f;
        public float Interval = 0.4f;
        public float Life = 2.6f;
        public bool Vein;
        public PlayerHealth OwnerHealth;
        public PlayerCombatStats OwnerStats;
        public Color Tint = new Color(0.7f, 0.85f, 1f, 0.45f);
        public float Burn;
        public bool Plague;
        public Transform Follow;
        public float GrowPerSec;
        float _tick;
        bool _drawn;

        void Start()
        {
            if (_drawn) return;
            _drawn = true;
            ShotFx.Ring(transform.position, Tint, Radius * 2f, Mathf.Min(Life, 0.45f));
        }

        void Update()
        {
            if (GameSession.IsPaused) return;
            if (Follow != null) transform.position = Follow.position + Vector3.up * 0.15f;
            if (GrowPerSec > 0f) Radius += GrowPerSec * Time.deltaTime;
            Life -= Time.deltaTime;
            if (Life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            _tick -= Time.deltaTime;
            if (_tick > 0f) return;
            _tick = Interval;
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            var r2 = Radius * Radius;
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                if (World.Planar(transform.position, enemy.transform.position).sqrMagnitude > r2) continue;
                var crit = false;
                var hit = OwnerStats != null ? OwnerStats.RollHit(TickDamage, out crit) : TickDamage;
                enemy.Damage(hit, transform.position, 0.6f, crit, Vein && OwnerStats != null);
                if (Burn > 0f) enemy.Ignite(Burn);
                if (Plague) enemy.ApplyPlague(TickDamage, transform.position);
                if (OwnerHealth != null && OwnerStats != null && OwnerStats.LifeSteal > 0f)
                    OwnerHealth.Heal(OwnerStats.LifeSteal);
            }
            ShotFx.Ring(transform.position, Tint, Radius * 2f, 0.12f);
        }
    }
}
