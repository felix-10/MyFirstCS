using UnityEngine;

namespace Veinfire
{
    public sealed class PlayerCombatStats : MonoBehaviour
    {
        public float MoveSpeed = 5.5f;
        public float MaxHp = 130f;
        public float Damage = 16f;
        public float FireInterval = 0.28f;
        public int ProjectileCount = 1;
        public float ProjectileSpeed = 16f;
        public float ProjectileSize = 0.24f;
        public float PickupRadius = 2.2f;
        public float LifeSteal = 0f;
        public float HealthRegen = 0f;
        public float Armor = 0f;
        public float Might = 1f;
        public float CooldownMul = 1f;
        public float Area = 1f;
        public int Pierce = 0;
        public float Knockback = 2.2f;
        public float Duration = 1f;
        public float CritChance;
        public float CritDamage = 2f;
        public float HazardResist;
        public bool SiphonXp;
        public bool ArmorBreak;
        public float DashIFrameBonus;
        public float FrenzyMove;
        public float FrenzyDamage;
        public float ExecuteMul;

        public float ScaledDamage => Damage * Might * (1f + FrenzyDamage);
        public float HitMul(EnemyHealth enemy)
        {
            var m = 1f;
            if (enemy != null && ExecuteMul > 0f && enemy.Hp01 <= 0.35f) m += ExecuteMul;
            return m;
        }
        public float ScaledInterval(float baseInterval)
        {
            var extra = 1f;
            var passives = GetComponent<HeroPassives>();
            if (passives != null) extra = passives.GeraltCdMul;
            var floor = GameInstaller.CurrentUnique == UniqueId.Hymn
                        && GetComponent<WeaponLoadout>() != null
                        && GetComponent<WeaponLoadout>().LevelOf(WeaponId.Needle) >= 5
                ? 0.04f
                : 0.07f;
            return Mathf.Max(floor, baseInterval * CooldownMul * extra * (RunConfig.RelicOn("冷静风暴") ? 0.82f : 1f));
        }
        public int ShotCount => Mathf.Max(1, ProjectileCount);

        public float RollHit(float amount, out bool crit)
        {
            crit = CritChance > 0f && UnityEngine.Random.value < Mathf.Clamp01(CritChance);
            if (crit) amount *= Mathf.Max(1f, CritDamage);
            return amount;
        }
    }
}
