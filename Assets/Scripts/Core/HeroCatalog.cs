using UnityEngine;

namespace Veinfire
{
    public enum HeroId
    {
        Geralt = 0,
        Liz = 1,
        Antalo = 2,
        Karen = 3,
        Mora = 4
    }

    public static class HeroCatalog
    {
        public const int Count = 5;

        public static string Title(HeroId id)
        {
            switch (id)
            {
                case HeroId.Liz: return "利兹";
                case HeroId.Antalo: return "安塔洛";
                case HeroId.Karen: return "卡伦";
                case HeroId.Mora: return "莫拉";
                default: return "杰罗特";
            }
        }

        public static string Role(HeroId id)
        {
            switch (id)
            {
                case HeroId.Liz: return "暴击";
                case HeroId.Antalo: return "生存";
                case HeroId.Karen: return "冲刺反击";
                case HeroId.Mora: return "DOT溃烂";
                default: return "均衡";
            }
        }

        public static string Passive(HeroId id)
        {
            switch (id)
            {
                case HeroId.Liz: return "裂隙裁决";
                case HeroId.Antalo: return "石肤共振";
                case HeroId.Karen: return "奔袭回响";
                case HeroId.Mora: return "溃烂蔓延";
                default: return "协同打击";
            }
        }

        public static string Blurb(HeroId id)
        {
            switch (id)
            {
                case HeroId.Liz: return "暴击命中刷新血脉、额外暴伤，并打出小范围脉冲。生命略低。";
                case HeroId.Antalo: return "护甲转化为生命。残血时击退与移速提升，回血获得短暂护盾。";
                case HeroId.Karen: return "冲刺穿过敌人造成范围伤害。冷却越低伤害越高。";
                case HeroId.Mora: return "DOT 有概率传染给附近敌人，自身直接伤害降低 12%。";
                default: return "每带一件普通武器加快攻速。击杀有概率标记敌人。";
            }
        }

        public static string CardText(HeroId id)
        {
            return $"{Title(id)}\n属性：{Role(id)}\n被动：{Passive(id)}\n{Blurb(id)}";
        }

        public static string Hover(HeroId id, bool open)
        {
            if (open) return CardText(id);
            return $"{Title(id)}\n未解锁。玩家模式请到元研究 → 生存分支解锁卡伦 / 莫拉。开发者模式可直接选择。";
        }

        public static Color Tint(HeroId id)
        {
            switch (id)
            {
                case HeroId.Liz: return new Color(0.35f, 0.85f, 1f);
                case HeroId.Antalo: return new Color(0.45f, 0.82f, 0.55f);
                case HeroId.Karen: return new Color(0.95f, 0.62f, 0.2f);
                case HeroId.Mora: return new Color(0.55f, 0.9f, 0.4f);
                default: return new Color(0.95f, 0.28f, 0.38f);
            }
        }

        public static void Apply(HeroId id, PlayerCombatStats stats)
        {
            if (stats == null) return;
            stats.MoveSpeed = 5.5f;
            stats.MaxHp = 130f;
            stats.Damage = 16f;
            stats.FireInterval = 0.28f;
            stats.ProjectileCount = 1;
            stats.ProjectileSpeed = 16f;
            stats.ProjectileSize = 0.24f;
            stats.PickupRadius = 2.2f;
            stats.LifeSteal = 0f;
            stats.HealthRegen = 0f;
            stats.Armor = 0f;
            stats.Might = 1f;
            stats.CooldownMul = 1f;
            stats.Area = 1f;
            stats.Pierce = 0;
            stats.Knockback = 2.2f;
            stats.Duration = 1f;
            stats.CritChance = 0f;
            stats.CritDamage = 2f;

            switch (id)
            {
                case HeroId.Liz:
                    stats.MaxHp = 112f;
                    stats.Damage = 15.5f;
                    stats.MoveSpeed = 5.65f;
                    stats.CritChance = 0.15f;
                    stats.CritDamage = 2.7f;
                    break;
                case HeroId.Antalo:
                    stats.MaxHp = 168f;
                    stats.Damage = 13.5f;
                    stats.MoveSpeed = 5.15f;
                    stats.Armor = 5f;
                    break;
                case HeroId.Karen:
                    stats.MaxHp = 122f;
                    stats.Damage = 14f;
                    stats.MoveSpeed = 6f;
                    stats.Armor = 2f;
                    break;
                case HeroId.Mora:
                    stats.MaxHp = 118f;
                    stats.Damage = 13f * 0.88f;
                    stats.MoveSpeed = 5.3f;
                    break;
            }
        }
    }
}
