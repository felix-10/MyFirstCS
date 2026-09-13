using UnityEngine;

namespace Veinfire
{
    public sealed class HeroPassives : MonoBehaviour
    {
        PlayerCombatStats _stats;
        PlayerHealth _health;
        WeaponLoadout _loadout;
        float _pulseLock;
        float _baseSpeed = 5.5f;
        bool _lowHpBuff;

        public static HeroPassives Of(Component c) => c != null ? c.GetComponent<HeroPassives>() : null;

        public float GeraltCdMul
        {
            get
            {
                if (GameInstaller.CurrentHero != HeroId.Geralt || _loadout == null) return 1f;
                var n = Mathf.Min(3, _loadout.SupportCount);
                var mul = 1f;
                for (var i = 0; i < n; i++) mul *= 0.92f;
                return mul;
            }
        }

        public float MoveMul
        {
            get
            {
                var v = 1f;
                if (_lowHpBuff) v *= 1.15f;
                if (_stats != null) v += _stats.FrenzyMove;
                return v;
            }
        }

        public float KnockMul => _lowHpBuff ? 1.6f : 1f;

        public float ExtraMaxHp =>
            GameInstaller.CurrentHero == HeroId.Antalo && _stats != null ? _stats.Armor * 0.8f : 0f;

        void Awake()
        {
            _stats = GetComponent<PlayerCombatStats>();
            _health = GetComponent<PlayerHealth>();
            _loadout = GetComponent<WeaponLoadout>();
            if (_stats != null) _baseSpeed = _stats.MoveSpeed;
        }

        void Start()
        {
            if (GameInstaller.CurrentHero != HeroId.Antalo || _health == null) return;
            var missing = _health.Max - _health.Current;
            if (missing > 0.5f) _health.Heal(missing, false);
        }

        void Update()
        {
            if (_health == null || _stats == null || _health.IsDead || GameSession.IsPaused) return;
            var low = GameInstaller.CurrentHero == HeroId.Antalo && _health.Max > 1f && _health.Current / _health.Max <= 0.4f;
            _lowHpBuff = low;
        }

        public void OnDealtHit(EnemyHealth enemy, float dealt, bool crit, bool melee, bool bullet)
        {
            if (enemy == null) return;
            if (_stats != null && _stats.ArmorBreak)
                enemy.ShredArmor();

            if (melee && RunRules.BladeWater && enemy.IsWet)
                enemy.AddPetrify(dealt, transform.position);
            if (bullet && RunRules.HymnFire)
                enemy.BoostBurn();
            if (GameInstaller.CurrentHero == HeroId.Liz && crit)
            {
                enemy.RefreshVeinOnce();
                TryPulse(enemy.transform.position);
            }
        }

        public void OnKilled(EnemyHealth dead)
        {
            if (GameInstaller.CurrentHero != HeroId.Geralt) return;
            if (Random.value > 0.12f) return;
            var other = CombatUtil.UniqueTarget(dead != null ? dead.transform.position : transform.position, 10f, dead);
            other?.GetComponent<EnemyHealth>()?.Mark(4f);
        }

        public void OnHealthPickup()
        {
            if (GameInstaller.CurrentHero != HeroId.Antalo || _health == null) return;
            _health.GrantShield(_health.Max * 0.1f, 3f);
        }

        void TryPulse(Vector3 at)
        {
            if (Time.time < _pulseLock || _stats == null) return;
            _pulseLock = Time.time + 0.7f;
            CombatUtil.DamageEnemiesInRadius(at, 1.8f, _stats.Damage * 0.4f, 1.2f);
        }
    }
}
