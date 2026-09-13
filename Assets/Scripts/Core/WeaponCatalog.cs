using UnityEngine;

namespace Veinfire
{
    public static class WeaponCatalog
    {
        public const int CommonCount = 30;

        public static readonly WeaponId[] Commons =
        {
            WeaponId.Orbit, WeaponId.Nova, WeaponId.Frost, WeaponId.Lash, WeaponId.Grenade,
            WeaponId.ShadowBlade, WeaponId.FlameRing, WeaponId.SpikeFan, WeaponId.Gravity, WeaponId.PoisonFog,
            WeaponId.IonJet, WeaponId.Spear, WeaponId.StarBurst, WeaponId.DarkOrbit, WeaponId.HolyAura,
            WeaponId.IceDisc, WeaponId.Totem, WeaponId.Homing, WeaponId.Shockwave, WeaponId.Spore,
            WeaponId.ChainBall, WeaponId.Refract, WeaponId.ChaosShot, WeaponId.MoonKnife, WeaponId.Magma,
            WeaponId.Thorns, WeaponId.ArrowRain, WeaponId.Drain, WeaponId.Aurora, WeaponId.Hammer
        };

        public static string Title(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Orbit: return "环绕毒牙";
                case WeaponId.Nova: return "血爆脉冲";
                case WeaponId.Frost: return "霜脉光环";
                case WeaponId.Lash: return "裂空鞭笞";
                case WeaponId.Grenade: return "震荡榴弹";
                case WeaponId.ShadowBlade: return "幽影飞刃";
                case WeaponId.FlameRing: return "烈焰环刃";
                case WeaponId.SpikeFan: return "尖刺弹幕";
                case WeaponId.Gravity: return "引力碎片";
                case WeaponId.PoisonFog: return "毒雾发生器";
                case WeaponId.IonJet: return "离子喷流";
                case WeaponId.Spear: return "穿刺飞矛";
                case WeaponId.StarBurst: return "碎裂星屑";
                case WeaponId.DarkOrbit: return "暗影旋刃";
                case WeaponId.HolyAura: return "圣光光环";
                case WeaponId.IceDisc: return "寒冰飞盘";
                case WeaponId.Totem: return "爆裂图腾";
                case WeaponId.Homing: return "追踪魔弹";
                case WeaponId.Shockwave: return "冲击波拳套";
                case WeaponId.Spore: return "腐臭孢子炮";
                case WeaponId.ChainBall: return "闪电链球";
                case WeaponId.Refract: return "晶光折射";
                case WeaponId.ChaosShot: return "混沌碎弹";
                case WeaponId.MoonKnife: return "月相飞刀";
                case WeaponId.Magma: return "熔岩喷吐";
                case WeaponId.Thorns: return "荆棘领域";
                case WeaponId.ArrowRain: return "幻像箭雨";
                case WeaponId.Drain: return "幽冥虹吸";
                case WeaponId.Aurora: return "极光奔流";
                case WeaponId.Hammer: return "裁决圣锤";
                default: return UniqueCatalog.Title(UniqueCatalog.OfWeapon(id));
            }
        }

        public static string Blurb(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Orbit: return "绕身旋转碰敌。升级加刃数和转速。";
                case WeaponId.Nova: return "自身范围血爆。升级缩短间隔并扩大半径。";
                case WeaponId.Frost: return "近身寒霜光环。升级加半径、减速和持续伤害。";
                case WeaponId.Lash: return "抽向近敌的鞭影。升级加长度，高阶额外抽第二鞭。";
                case WeaponId.Grenade: return "投掷延迟爆炸弹。升级加爆炸半径，不是加弹数。";
                case WeaponId.ShadowBlade: return "折返穿透飞刃。升级加穿透和折返距离。";
                case WeaponId.FlameRing: return "燃烧环刃飞出再收回。升级加灼烧和环体。";
                case WeaponId.SpikeFan: return "近距扇形尖刺。升级加密尖刺并加快射速。";
                case WeaponId.Gravity: return "环绕碎片持续拉敌。升级加碎片数和拉力。";
                case WeaponId.PoisonFog: return "缠身扩张毒雾。升级加扩散速度和持续时间。";
                case WeaponId.IonJet: return "短距离子喷流。升级加束宽和射程。";
                case WeaponId.Spear: return "高速穿刺长矛。升级加穿透，满级才双矛。";
                case WeaponId.StarBurst: return "四周炸开星屑。升级加星屑数量。";
                case WeaponId.DarkOrbit: return "黑暗环绕刃。升级加刃数并附加减速。";
                case WeaponId.HolyAura: return "近身圣光叠甲。升级加光环伤害和半径。";
                case WeaponId.IceDisc: return "寒冰飞盘命中分裂。升级加碎冰数量。";
                case WeaponId.Totem: return "落地图腾定时爆开。升级加图腾存活时间。";
                case WeaponId.Homing: return "追踪魔弹。升级加转向，高阶才第二发。";
                case WeaponId.Shockwave: return "近身气浪向外推开。升级加冲击圈和击退。";
                case WeaponId.Spore: return "孢子弹落地留传染区。升级加区域持续时间。";
                case WeaponId.ChainBall: return "白光电弧在敌人之间连跳，同时击中整条链。升级加跳跃人数。";
                case WeaponId.Refract: return "碰壁折射光束。升级加折射次数。";
                case WeaponId.ChaosShot: return "螺旋畸变碎弹。升级加扭曲，满级双弹。";
                case WeaponId.MoonKnife: return "环身向外放射飞刀。升级加放射段数。";
                case WeaponId.Magma: return "扇形熔岩喷射并灼烧。升级加锥角和射程。";
                case WeaponId.Thorns: return "脚下长出荆棘。升级加生成频率和刺丛。";
                case WeaponId.ArrowRain: return "箭矢从天落下覆盖一片。升级加箭雨密度。";
                case WeaponId.Drain: return "虹吸拉近敌人并回血。升级加吸取强度。";
                case WeaponId.Aurora: return "长距极光幕。升级加幕宽、长度和冻结。";
                case WeaponId.Hammer: return "落锤砸随机敌人。升级加落锤次数。";
                default: return UniqueCatalog.Blurb(UniqueCatalog.OfWeapon(id));
            }
        }

        public enum WeaponFamily
        {
            Melee,
            Ranged,
            Dot
        }

        public static WeaponFamily FamilyOf(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Blade:
                case WeaponId.Orbit:
                case WeaponId.DarkOrbit:
                case WeaponId.Shockwave:
                case WeaponId.Thorns:
                case WeaponId.Gravity:
                case WeaponId.Hammer:
                case WeaponId.Lash:
                case WeaponId.Nova:
                    return WeaponFamily.Melee;
                case WeaponId.Frost:
                case WeaponId.PoisonFog:
                case WeaponId.Spore:
                case WeaponId.HolyAura:
                case WeaponId.Drain:
                    return WeaponFamily.Dot;
                default:
                    return WeaponFamily.Ranged;
            }
        }

        public static string FamilyTitle(WeaponFamily family)
        {
            switch (family)
            {
                case WeaponFamily.Melee: return "近战";
                case WeaponFamily.Dot: return "持续";
                default: return "远程";
            }
        }

        public static bool Unlocked(WeaponId id)
        {
            if (GameSettings.Developer) return true;
            if (UniqueCatalog.IsUniqueWeapon(id)) return MetaProgress.UniqueUnlocked(UniqueCatalog.OfWeapon(id));
            var i = IndexOf(id);
            if (i < 4) return true;
            if (i < 12) return MetaProgress.Data.unlockWeapon1;
            if (i < 22) return MetaProgress.Data.unlockWeapon2;
            return MetaProgress.Data.unlockWeapon3;
        }

        public static int IndexOf(WeaponId id)
        {
            for (var i = 0; i < Commons.Length; i++)
                if (Commons[i] == id) return i;
            return 99;
        }
    }

    public struct FusionRecipe
    {
        public string Id;
        public WeaponId A;
        public WeaponId B;
        public string Title;
        public string Effect;
        public bool Pack;
    }

    public static class FusionCatalog
    {
        public static readonly FusionRecipe[] All =
        {
            Rec("orbit_nova", WeaponId.Orbit, WeaponId.Nova, "熔核环爆", "环刃 +2，血爆保持满级。占 1 栏。", false),
            Rec("frost_lash", WeaponId.Frost, WeaponId.Lash, "霜裂长鞭", "鞭影命中附带减速。占 1 栏。", false),
            Rec("totem_nade", WeaponId.Totem, WeaponId.Grenade, "暴虐图腾", "图腾改为榴弹爆，范围更大并点燃。", false),
            Rec("totem_shock", WeaponId.Totem, WeaponId.Shockwave, "震地巫柱", "图腾每次脉冲打出冲击波并击退。", false),
            Rec("nade_magma", WeaponId.Grenade, WeaponId.Magma, "榴岩浆爆", "榴弹爆炸半径增大并点燃。", true),
            Rec("spike_spear", WeaponId.SpikeFan, WeaponId.Spear, "贯刺连弩", "尖刺获得穿透，飞矛穿透再加 2。", true),
            Rec("shadow_dark", WeaponId.ShadowBlade, WeaponId.DarkOrbit, "影刃双生", "飞刃命中减速，暗影旋刃刃数 +1。", true),
            Rec("holy_hammer", WeaponId.HolyAura, WeaponId.Hammer, "圣裁连锤", "落锤次数 +1，圣光叠甲更快。", true),
            Rec("ice_aurora", WeaponId.IceDisc, WeaponId.Aurora, "极寒极光", "飞盘冻结更久，极光幕更宽。", true),
            Rec("fog_spore", WeaponId.PoisonFog, WeaponId.Spore, "疫雾孢潮", "毒雾与孢子区持续时间显著延长。", true),
            Rec("ion_chain", WeaponId.IonJet, WeaponId.ChainBall, "离子电链", "链球多跳 3 人，喷流略加宽。", true),
            Rec("homing_moon", WeaponId.Homing, WeaponId.MoonKnife, "寻月飞刃", "追踪弹额外 1 发，飞刀放射更密。", true),
            Rec("thorns_gravity", WeaponId.Thorns, WeaponId.Gravity, "荆棘引力井", "荆棘多一丛，引力碎片拉力增强。", true),
            Rec("drain_chaos", WeaponId.Drain, WeaponId.ChaosShot, "虹吸畸变", "虹吸回血提高，混沌碎弹额外 1 发。", true),
            Rec("flame_magma", WeaponId.FlameRing, WeaponId.Magma, "焚轮熔喷", "环刃灼烧更久，熔岩锥更宽。", true),
            Rec("flame_orbit", WeaponId.FlameRing, WeaponId.Orbit, "双环焚刃", "毒牙 +1，环刃体型增大。", true),
            Rec("star_arrow", WeaponId.StarBurst, WeaponId.ArrowRain, "星矢流雨", "星屑与箭雨更密。", true),
            Rec("star_chaos", WeaponId.StarBurst, WeaponId.ChaosShot, "畸变星爆", "星屑螺旋扭曲，碎弹 +1。", true),
            Rec("ion_refract", WeaponId.IonJet, WeaponId.Refract, "折射离子束", "喷流加长，折射次数 +2。", true),
            Rec("spike_arrow", WeaponId.SpikeFan, WeaponId.ArrowRain, "刺雨连弩", "尖刺加密，箭雨再加一圈。", true),
            Rec("lash_shock", WeaponId.Lash, WeaponId.Shockwave, "裂地震鞭", "冲击圈更大，鞭影附带击退。", true),
            Rec("frost_ice", WeaponId.Frost, WeaponId.IceDisc, "霜盘光环", "飞盘冻结更久，霜环略扩大。", true),
            Rec("holy_aurora", WeaponId.HolyAura, WeaponId.Aurora, "圣光极幕", "圣光叠甲更快，极光幕更宽。", true)
        };

        static FusionRecipe Rec(string id, WeaponId a, WeaponId b, string title, string effect, bool pack)
        {
            return new FusionRecipe { Id = id, A = a, B = b, Title = title, Effect = effect, Pack = pack };
        }

        public static bool On(string id) => RunConfig.HasFusion(id);

        public enum RowState
        {
            Locked,
            Missing,
            Progress,
            Ready,
            Done,
            Spent
        }

        public static FusionRecipe Find(string id)
        {
            for (var i = 0; i < All.Length; i++)
                if (All[i].Id == id) return All[i];
            return default;
        }

        public static bool WeaponBound(WeaponId id)
        {
            for (var i = 0; i < All.Length; i++)
                if (On(All[i].Id) && Involves(All[i], id)) return true;
            return false;
        }

        public static bool RecipeAvailable(FusionRecipe r)
        {
            if (!RecipeOpen(r) || On(r.Id)) return false;
            return !WeaponBound(r.A) && !WeaponBound(r.B);
        }

        public static RowState StateOf(FusionRecipe r, WeaponLoadout loadout)
        {
            if (On(r.Id)) return RowState.Done;
            if (!MetaProgress.FusionUnlocked || !RecipeOpen(r)) return RowState.Locked;
            if (WeaponBound(r.A) || WeaponBound(r.B)) return RowState.Spent;
            var a = loadout != null && loadout.Owns(r.A);
            var b = loadout != null && loadout.Owns(r.B);
            if (a && b && loadout.LevelOf(r.A) >= 5 && loadout.LevelOf(r.B) >= 5) return RowState.Ready;
            if (a || b) return RowState.Progress;
            return RowState.Missing;
        }

        public static string StateTitle(RowState s)
        {
            switch (s)
            {
                case RowState.Done: return "已组合";
                case RowState.Ready: return "可合成";
                case RowState.Progress: return "可推进";
                case RowState.Spent: return "已占用";
                case RowState.Locked: return "未解锁";
                default: return "未获得";
            }
        }

        public static bool RecipeOpen(FusionRecipe r)
        {
            if (!MetaProgress.FusionUnlocked) return false;
            if (r.Pack && !GameSettings.Developer && !MetaProgress.Data.unlockFusionPack) return false;
            return true;
        }

        public static WeaponId Partner(FusionRecipe r, WeaponId id)
        {
            if (r.A == id) return r.B;
            if (r.B == id) return r.A;
            return id;
        }

        public static bool Involves(FusionRecipe r, WeaponId id) => r.A == id || r.B == id;

        public static string CardHint(WeaponId id, WeaponLoadout loadout)
        {
            if (loadout == null || !MetaProgress.FusionUnlocked || WeaponBound(id)) return "";
            var lines = new System.Text.StringBuilder();
            var ready = 0;
            var n = 0;
            for (var i = 0; i < All.Length; i++)
            {
                var r = All[i];
                if (!RecipeAvailable(r) || !Involves(r, id)) continue;
                var other = Partner(r, id);
                if (!loadout.Owns(other) || WeaponBound(other)) continue;
                n++;
                var bothMax = loadout.LevelOf(r.A) >= 5 && loadout.LevelOf(r.B) >= 5;
                if (bothMax) ready++;
                lines.Append(bothMax ? "可选 " : "推进 ");
                lines.Append(WeaponCatalog.Title(other));
                lines.Append(" → ");
                lines.Append(r.Title);
                lines.Append("：");
                lines.Append(r.Effect);
                lines.Append('\n');
            }

            if (n == 0) return "";
            string head;
            if (ready > 1) head = "【组合】多套已满级，升级时自选其一。合成后本武器不再与其它武器结合。\n";
            else if (n > 1) head = "【组合】可与多件武器配对。双方满级后升级卡自选合成，选定后其它路线关闭。\n";
            else if (ready == 1) head = "【组合】双方已满级，升级卡将出现组合技能。\n";
            else head = "【组合】\n";
            return head + lines.ToString().TrimEnd();
        }

        public static void FillOffers(System.Collections.Generic.List<UpgradeOption> pool, WeaponLoadout loadout)
        {
            if (!MetaProgress.FusionUnlocked || loadout == null || pool == null) return;
            for (var i = 0; i < All.Length; i++)
            {
                var r = All[i];
                if (!RecipeAvailable(r)) continue;
                if (loadout.LevelOf(r.A) < 5 || loadout.LevelOf(r.B) < 5) continue;
                var copy = r;
                var opt = new UpgradeOption(
                    $"组合技能 · {r.Title}",
                    $"{WeaponCatalog.Title(r.A)} + {WeaponCatalog.Title(r.B)}\n{r.Effect}\n选定后这两件武器不再与其它配方结合。占 1 栏。",
                    () => Apply(copy.Id, loadout),
                    "合",
                    true,
                    "组合");
                opt.FusionHint = true;
                opt.Color = new Color(1f, 0.82f, 0.32f);
                pool.Add(opt);
            }
        }

        public static void Apply(string id, WeaponLoadout loadout)
        {
            var recipe = Find(id);
            if (string.IsNullOrEmpty(recipe.Id)) return;
            if (WeaponBound(recipe.A) || WeaponBound(recipe.B)) return;
            RunConfig.MarkFusion(id);
            if (loadout == null) return;
            if (id == "orbit_nova" || id == "flame_orbit")
            {
                var orbit = loadout.GetComponent<OrbitWeapon>();
                if (orbit != null)
                {
                    orbit.Fangs += id == "orbit_nova" ? 2 : 1;
                    orbit.Rebuild();
                }
                var nova = loadout.GetComponent<NovaWeapon>();
                if (id == "orbit_nova" && nova != null) nova.Level = 5;
            }

            var halos = loadout.GetComponents<ArmHalo>();
            for (var i = 0; i < halos.Length; i++)
                if (halos[i] != null) halos[i].Rebuild();
        }

        public static void UnbindWeapon(WeaponId id, WeaponLoadout loadout, System.Collections.Generic.List<TraitRecord> passives)
        {
            for (var i = 0; i < All.Length; i++)
            {
                var r = All[i];
                if (!On(r.Id) || !Involves(r, id)) continue;
                if ((r.Id == "orbit_nova" || r.Id == "flame_orbit") && loadout != null)
                {
                    var orbit = loadout.GetComponent<OrbitWeapon>();
                    if (orbit != null && id != WeaponId.Orbit)
                    {
                        orbit.Fangs = Mathf.Max(2, orbit.Fangs - (r.Id == "orbit_nova" ? 2 : 1));
                        orbit.Rebuild();
                    }
                }
                RunConfig.UnmarkFusion(r.Id);
                if (passives == null) continue;
                for (var p = passives.Count - 1; p >= 0; p--)
                    if (passives[p] != null && !string.IsNullOrEmpty(passives[p].Title) && passives[p].Title.IndexOf(r.Title, System.StringComparison.Ordinal) >= 0)
                        passives.RemoveAt(p);
            }
        }
    }
}
