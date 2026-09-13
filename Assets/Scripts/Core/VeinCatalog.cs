using UnityEngine;

namespace Veinfire
{
    public enum VeinId
    {
        Fire = 0,
        Water = 1,
        Earth = 2,
        Corrode = 3,
        Thunder = 4,
        Ice = 5,
        Plague = 6,
        Holy = 7,
        Chaos = 8
    }

    public static class VeinCatalog
    {
        public const float BurnDps = 3.5f;
        public const float BurnTime = 2.2f;
        const float SplashRadius = 2.4f;
        const float SplashScale = 0.32f;

        public const int Count = 9;

        public static string Title(VeinId id)
        {
            switch (id)
            {
                case VeinId.Water: return "水之血脉";
                case VeinId.Earth: return "土之血脉";
                case VeinId.Corrode: return "腐蚀血脉";
                case VeinId.Thunder: return "雷霆血脉";
                case VeinId.Ice: return "寒冰血脉";
                case VeinId.Plague: return "瘟疫血脉";
                case VeinId.Holy: return "圣光血脉";
                case VeinId.Chaos: return "混沌血脉";
                default: return "火之血脉";
            }
        }

        public static string Blurb(VeinId id)
        {
            switch (id)
            {
                case VeinId.Water: return "攻击附带湿润。伤害会扩散到附近小范围敌人，扩散伤害较低。";
                case VeinId.Earth: return "攻击附带石化。叠满 3 层时爆发特殊伤害，并降低敌人移速 1 秒。";
                case VeinId.Corrode: return "腐蚀降低敌人护甲，最低锁 1 血。";
                case VeinId.Thunder: return "雷电印记，有概率链式电击。";
                case VeinId.Ice: return "冻结累积，满层冻结并提升所受物理伤害。";
                case VeinId.Plague: return "瘟疫感染持续掉血，死亡可传染。";
                case VeinId.Holy: return "神圣持续伤害，命中为自己叠少量护甲。";
                case VeinId.Chaos: return "随机易伤或减速，伤害倍率浮动。";
                default: return "攻击附带灼烧。灼烧伤害单独计算，与当次攻击力无关。";
            }
        }

        public static string CardText(VeinId id)
        {
            return $"{Title(id)}\n{Blurb(id)}";
        }

        public static string Hover(VeinId id, bool open)
        {
            if (open) return CardText(id);
            return $"{Title(id)}\n未解锁。玩家模式请到元研究 → 血脉分支解锁。开发者模式可直接选择。";
        }

        public static void OnPlayerHit(EnemyHealth enemy, float dealt, Vector3 from)
        {
            if (enemy == null || dealt <= 0f) return;
            var scale = 1f;
            if (RunConfig.HasMod(RelicMod.VeinResonance)) scale *= 1.35f;
            if (RunConfig.RelicOn("血脉倍增") || RunConfig.RelicOn("血脉之心")) scale *= 2f;
            scale *= 1f + MetaProgress.LiveVeinDot;
            if (RunConfig.RelicOn("腐蚀烙印")) enemy.ApplyCorrode(2.8f);
            var stats = Object.FindFirstObjectByType<PlayerCombatStats>();
            var duration = stats != null ? Mathf.Max(1f, stats.Duration) : 1f;
            ApplyOne(GameInstaller.CurrentVein, enemy, dealt, from, scale, duration);
            if (RunConfig.DualVein)
            {
                var other = (VeinId)(((int)GameInstaller.CurrentVein + 1) % VeinCatalog.Count);
                ApplyOne(other, enemy, dealt * 0.55f, from, 1f, duration);
            }
        }

        static void ApplyOne(VeinId vein, EnemyHealth enemy, float dealt, Vector3 from, float scale, float duration)
        {
            switch (vein)
            {
                case VeinId.Water:
                    enemy.ApplyWet(1.8f * scale * duration);
                    Splash(enemy, dealt * scale, from);
                    break;
                case VeinId.Earth:
                    enemy.AddPetrify(dealt * scale, from);
                    break;
                case VeinId.Corrode:
                    enemy.ApplyCorrode(2.8f * scale * duration);
                    break;
                case VeinId.Thunder:
                    enemy.ApplyShock(dealt * scale, from);
                    break;
                case VeinId.Ice:
                    enemy.ApplyFreeze(1.2f * scale);
                    break;
                case VeinId.Plague:
                    enemy.ApplyPlague(dealt * scale, from);
                    break;
                case VeinId.Holy:
                    enemy.ApplyHoly(dealt * scale, from);
                    break;
                case VeinId.Chaos:
                    enemy.ApplyChaos(dealt * scale, from);
                    break;
                default:
                    enemy.Ignite(BurnTime * scale * duration);
                    break;
            }

            if (GameInstaller.CurrentHero == HeroId.Mora && Random.value < 0.25f)
            {
                var spread = CombatUtil.UniqueTarget(enemy.transform.position, 3.2f, enemy);
                if (spread != null)
                    ApplyOne(vein, spread.GetComponent<EnemyHealth>(), dealt * 0.4f, from, 0.7f, duration);
            }
        }

        static void Splash(EnemyHealth source, float dealt, Vector3 from)
        {
            if (source == null) return;
            var splash = dealt * SplashScale;
            if (splash < 1f) return;
            var origin = source.transform.position;
            var r2 = (SplashRadius + (RunRules.BladeWater ? 1f : 0f)) * (SplashRadius + (RunRules.BladeWater ? 1f : 0f));
            var sourceId = source.GetInstanceID();
            var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            var nearby = 0;
            for (var i = 0; i < enemies.Length; i++)
            {
                if (IsSplashTarget(enemies[i], sourceId, origin, r2))
                    nearby += 1;
            }

            if (nearby <= 0) return;

            for (var i = 0; i < enemies.Length; i++)
            {
                var other = enemies[i];
                if (!IsSplashTarget(other, sourceId, origin, r2)) continue;
                other.ApplyWet(1.4f);
                other.Damage(splash, from, 0.4f, false, false, HitKind.Wet);
            }
        }

        static bool IsSplashTarget(EnemyHealth other, int sourceId, Vector3 origin, float r2)
        {
            if (other == null || !other.gameObject.activeInHierarchy) return false;
            if (other.GetInstanceID() == sourceId) return false;
            var dist = World.Planar(origin, other.transform.position).sqrMagnitude;
            return dist >= 0.25f && dist <= r2;
        }
    }
}
