using UnityEngine;

namespace Veinfire
{
    public enum UniqueId
    {
        Hymn = 0,
        Bolt = 1,
        Blade = 2,
        Spiral = 3,
        Star = 4
    }

    public static class UniqueCatalog
    {
        public const int Count = 5;

        public static UniqueId OfWeapon(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Divine: return UniqueId.Bolt;
                case WeaponId.Blade: return UniqueId.Blade;
                case WeaponId.Spiral: return UniqueId.Spiral;
                case WeaponId.Star: return UniqueId.Star;
                default: return UniqueId.Hymn;
            }
        }

        public static string Title(UniqueId id)
        {
            switch (id)
            {
                case UniqueId.Bolt: return "神罚天降";
                case UniqueId.Blade: return "特莫利斯之剑";
                case UniqueId.Spiral: return "幽猎螺旋";
                case UniqueId.Star: return "星陨仪";
                default: return "地狱绝唱";
            }
        }

        public static string Blurb(UniqueId id)
        {
            switch (id)
            {
                case UniqueId.Bolt: return "锁定落雷。升级加伤害和落点半径，不是加弹数。";
                case UniqueId.Blade: return "近战扇形斩。升级加剑气范围，满级狂斩。";
                case UniqueId.Spiral: return "单发螺旋穿透弹。升级加螺距和穿透，满级折返。";
                case UniqueId.Star: return "天上落星轰炸。升级加落星数量和范围，满级留灼烧并拉敌。";
                default: return "瞄准连射火针。升级加弹数和贯穿，满级命中分裂。";
            }
        }

        public static string CardText(UniqueId id) => $"{Title(id)}\n{Blurb(id)}";

        public static string Hover(UniqueId id, bool open)
        {
            if (open) return CardText(id);
            return $"{Title(id)}\n未解锁。玩家模式请到元研究 → 军械分支解锁专武Ⅳ。开发者模式可直接选择。";
        }

        public static WeaponId WeaponOf(UniqueId id)
        {
            switch (id)
            {
                case UniqueId.Bolt: return WeaponId.Divine;
                case UniqueId.Blade: return WeaponId.Blade;
                case UniqueId.Spiral: return WeaponId.Spiral;
                case UniqueId.Star: return WeaponId.Star;
                default: return WeaponId.Needle;
            }
        }

        public static bool IsUniqueWeapon(WeaponId id)
        {
            return id == WeaponId.Needle || id == WeaponId.Divine || id == WeaponId.Blade
                || id == WeaponId.Spiral || id == WeaponId.Star;
        }

        public static string UpgradeBlurb(UniqueId id, int nextLevel = 0)
        {
            if (nextLevel >= 5)
            {
                switch (id)
                {
                    case UniqueId.Bolt: return "进化：天陨审判。落地雷域，并可连锁一名敌人。";
                    case UniqueId.Blade: return "进化：断脉狂斩。扇形斩击，叠层加速，削减冲刺冷却。";
                    case UniqueId.Spiral: return "进化：虚空涡旋。螺旋弹折返，折返伤害提高。";
                    case UniqueId.Star: return "进化：星界坍缩。轰炸留下灼烧区域并拉扯敌人。";
                    default: return "进化：炼狱弹幕。命中分裂次级弹。";
                }
            }

            switch (id)
            {
                case UniqueId.Bolt: return "雷击伤害提高，落点范围扩大";
                case UniqueId.Blade: return "斩击范围与伤害提高";
                case UniqueId.Spiral: return "螺旋半径与穿透提高";
                case UniqueId.Star: return "落星更多，轰炸范围扩大";
                default: return "多一支火针，并提高贯穿";
            }
        }

        public static void Apply(GameObject player)
        {
            if (player == null) return;
            var id = GameInstaller.CurrentUnique;
            var hymn = player.GetComponent<AutoAimWeapon>();
            if (hymn != null) hymn.enabled = id == UniqueId.Hymn;

            var bolt = player.GetComponent<LightningWeapon>() ?? player.AddComponent<LightningWeapon>();
            bolt.enabled = id == UniqueId.Bolt;

            var blade = player.GetComponent<TemorisSword>() ?? player.AddComponent<TemorisSword>();
            blade.enabled = id == UniqueId.Blade;

            var spiral = player.GetComponent<SpiralHuntWeapon>() ?? player.AddComponent<SpiralHuntWeapon>();
            spiral.enabled = id == UniqueId.Spiral;

            var star = player.GetComponent<StarfallInstrument>() ?? player.AddComponent<StarfallInstrument>();
            star.enabled = id == UniqueId.Star;

            if (player.GetComponent<UniqueBuild>() == null) player.AddComponent<UniqueBuild>();
            player.GetComponent<WeaponLoadout>()?.EquipStarter(id);
        }

        public static void FillBranchCards(System.Collections.Generic.List<UpgradeOption> into, WeaponLoadout loadout)
        {
            if (into == null || loadout == null) return;
            var unique = GameInstaller.CurrentUnique;
            var weapon = WeaponOf(unique);
            var next = loadout.LevelOf(weapon) + 1;
            var build = UniqueBuild.Of(loadout);
            if (next >= 5)
            {
                AddCard(into, loadout, weapon, build, 0, 1, $"进化 · {EvoName(unique, 1)}", EvoBlurb(unique, 1));
                AddCard(into, loadout, weapon, build, 0, 2, $"进化 · {EvoName(unique, 2)}", EvoBlurb(unique, 2));
                AddCard(into, loadout, weapon, build, 0, 3, $"进化 · {EvoName(unique, 3)}", EvoBlurb(unique, 3));
                return;
            }

            AddCard(into, loadout, weapon, build, 1, 0, $"分支 · {BranchName(unique, 1)}", BranchBlurb(unique, 1) + $"\n专武升至 {next} 级");
            AddCard(into, loadout, weapon, build, 2, 0, $"分支 · {BranchName(unique, 2)}", BranchBlurb(unique, 2) + $"\n专武升至 {next} 级");
            AddCard(into, loadout, weapon, build, 3, 0, $"分支 · {BranchName(unique, 3)}", BranchBlurb(unique, 3) + $"\n专武升至 {next} 级");
        }

        static void AddCard(System.Collections.Generic.List<UpgradeOption> into, WeaponLoadout loadout, WeaponId weapon, UniqueBuild build, int branch, int evo, string title, string desc)
        {
            into.Add(new UpgradeOption(title, desc, () =>
            {
                if (branch > 0) build.Branch = branch;
                if (evo > 0) build.Evo = evo;
                loadout.Upgrade(weapon);
            }, evo > 0 ? "进" : "专", true, "专武"));
        }

        public static string BranchName(UniqueId id, int branch)
        {
            switch (id)
            {
                case UniqueId.Bolt: return branch == 2 ? "疾雷" : branch == 3 ? "重罚" : "天域";
                case UniqueId.Blade: return branch == 2 ? "连斩" : branch == 3 ? "重斩" : "开锋";
                case UniqueId.Spiral: return branch == 2 ? "穿空" : branch == 3 ? "虚刃" : "涡旋";
                case UniqueId.Star: return branch == 2 ? "陨核" : branch == 3 ? "群星" : "星雨";
                default: return branch == 2 ? "贯穿" : branch == 3 ? "灼心" : "弹幕";
            }
        }

        public static string BranchBlurb(UniqueId id, int branch)
        {
            switch (id)
            {
                case UniqueId.Bolt: return branch == 2 ? "落雷更快，范围略收。" : branch == 3 ? "单点伤害更高，落点更小。" : "落点范围更大。";
                case UniqueId.Blade: return branch == 2 ? "挥砍更快。" : branch == 3 ? "每刀更痛，速度略慢。" : "斩距更长更宽。";
                case UniqueId.Spiral: return branch == 2 ? "更快更穿。" : branch == 3 ? "弹体更粗，可减速。" : "螺旋半径更大。";
                case UniqueId.Star: return branch == 2 ? "落星更少更猛。" : branch == 3 ? "落星挤在目标周围。" : "落星更多更散。";
                default: return branch == 2 ? "少弹、高穿、高速。" : branch == 3 ? "命中点燃。" : "火针数量优先。";
            }
        }

        public static string EvoName(UniqueId id, int evo)
        {
            switch (id)
            {
                case UniqueId.Bolt: return evo == 2 ? "万钧连环" : evo == 3 ? "神罚领域" : "天陨审判";
                case UniqueId.Blade: return evo == 2 ? "开疆裂地" : evo == 3 ? "血祭刀势" : "断脉狂斩";
                case UniqueId.Spiral: return evo == 2 ? "冰晶裂空" : evo == 3 ? "虚空绞杀" : "虚空涡旋";
                case UniqueId.Star: return evo == 2 ? "星河倾泻" : evo == 3 ? "引力坍缩" : "星界坍缩";
                default: return evo == 2 ? "业火长钉" : evo == 3 ? "狱火扫射" : "炼狱弹幕";
            }
        }

        public static string EvoBlurb(UniqueId id, int evo)
        {
            switch (id)
            {
                case UniqueId.Bolt: return evo == 2 ? "不再留雷域，电弧连跳更多人。" : evo == 3 ? "超大雷域，不再连锁。" : "落地留雷域，并可连锁一人。";
                case UniqueId.Blade: return evo == 2 ? "超宽扇斩，不叠狂斩。" : evo == 3 ? "命中吸血，伤害更高。" : "扇形狂斩，叠层加速并减冲刺冷却。";
                case UniqueId.Spiral: return evo == 2 ? "命中分裂冰片。" : evo == 3 ? "更粗的螺旋并减速。" : "到达最远后折返，折返伤害提高。";
                case UniqueId.Star: return evo == 2 ? "落星数量大增，不留灼烧。" : evo == 3 ? "强力拉向落点中心。" : "留下灼烧区域并拉敌。";
                default: return evo == 2 ? "不再分裂，改为贯穿并点燃。" : evo == 3 ? "超宽弹幕扫射。" : "命中分裂两枚次级弹。";
            }
        }
    }

    public sealed class UniqueBuild : MonoBehaviour
    {
        public int Branch;
        public int Evo;

        public int Path => Branch <= 0 ? 1 : Branch;
        public int Evolution => Evo <= 0 ? 1 : Evo;

        public static UniqueBuild Of(Component c)
        {
            if (c == null) return null;
            var build = c.GetComponent<UniqueBuild>();
            if (build == null) build = c.gameObject.AddComponent<UniqueBuild>();
            return build;
        }
    }
}
