using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veinfire
{
    public sealed class LevelDirector : MonoBehaviour
    {
        public event Action<int, int, int> XpChanged;
        public event Action<int, List<UpgradeOption>> LevelUpOffered;

        public PlayerCombatStats Stats;
        public PlayerHealth Health;
        public WeaponLoadout Loadout;
        public PlayerDash Dash;
        public int Level = 1;
        public int Xp;
        public int XpToNext = 32;

        public readonly List<TraitRecord> Passives = new List<TraitRecord>();
        int _pendingOffers;
        public int Rerolls = 3;

        public int PendingOffers => _pendingOffers;

        public static int BaseRerolls() => BalanceTables.Rerolls;

        public static bool IsUniqueLevel(int level)
        {
            var levels = BalanceTables.UniqueLevels;
            for (var i = 0; i < levels.Length; i++)
                if (levels[i] == level) return true;
            return false;
        }

        public void Restore(int level, int xp, int xpToNext, int pending, List<TraitRecord> passives, int rerolls = -1)
        {
            Level = Mathf.Max(1, level);
            Xp = Mathf.Max(0, xp);
            XpToNext = xpToNext > 0 ? xpToNext : XpRequiredFor(Level);
            _pendingOffers = Mathf.Max(0, pending);
            if (rerolls >= 0) Rerolls = rerolls;
            Passives.Clear();
            if (passives != null) Passives.AddRange(passives);
            XpChanged?.Invoke(Level, Xp, XpToNext);
            if (_pendingOffers > 0 && !GameSession.UpgradeLock)
                ShowNextOffer();
        }

        public static int XpRequiredFor(int level)
        {
            var lv = Mathf.Max(1, level);
            return 30 + 14 * (lv - 1);
        }

        void Awake()
        {
            XpToNext = XpRequiredFor(Level);
            Rerolls = BaseRerolls();
        }

        public void AddXp(int amount)
        {
            if (amount <= 0 || GameSession.IsGameOver) return;
            var bonus = GameSession.Combo >= 12 ? 1.2f : GameSession.Combo >= 6 ? 1.1f : 1f;
            Xp += Mathf.RoundToInt(amount * bonus);
            while (Xp >= XpToNext)
            {
                Xp -= XpToNext;
                Level += 1;
                XpToNext = XpRequiredFor(Level);
                _pendingOffers += 1;
            }

            XpChanged?.Invoke(Level, Xp, XpToNext);
            if (_pendingOffers > 0 && !GameSession.UpgradeLock)
                ShowNextOffer();
        }

        public void OfferChest()
        {
            _pendingOffers += 1;
            if (!GameSession.UpgradeLock)
                ShowNextOffer();
        }

        void ShowNextOffer(bool chime = true)
        {
            var picks = IsUniqueLevel(Level) && CanOfferUnique()
                ? BuildUniquePicks()
                : BuildMixPicks();
            GameSession.SetUpgradePause(true);
            LevelUpOffered?.Invoke(Level, picks);
            if (chime) GameFeel.LevelUp();
        }

        public void Reroll()
        {
            if (Rerolls <= 0 || !GameSession.UpgradeLock) return;
            Rerolls -= 1;
            GameFeel.Ui();
            ShowNextOffer(false);
        }

        bool CanOfferUnique()
        {
            if (Loadout == null) return false;
            if (RunConfig.HasMod(RelicMod.WeaponTrial)) return false;
            var id = UniqueCatalog.WeaponOf(GameInstaller.CurrentUnique);
            return Loadout.Owns(id) && Loadout.LevelOf(id) < 5;
        }

        List<UpgradeOption> BuildUniquePicks()
        {
            var picks = new List<UpgradeOption>();
            UniqueCatalog.FillBranchCards(picks, Loadout);
            return picks;
        }

        List<UpgradeOption> BuildMixPicks()
        {
            var weapons = new List<UpgradeOption>();
            var passives = new List<UpgradeOption>();
            var fusions = new List<UpgradeOption>();
            FusionCatalog.FillOffers(fusions, Loadout);
            var pool = BuildPool();
            for (var i = 0; i < pool.Count; i++)
            {
                if (pool[i] == null) continue;
                if (pool[i].FusionHint) continue;
                if (pool[i].IsWeapon) weapons.Add(pool[i]);
                else passives.Add(pool[i]);
            }

            var picks = new List<UpgradeOption>();
            for (var i = 0; i < fusions.Count && picks.Count < 6; i++)
                picks.Add(fusions[i]);
            var weaponSlots = Mathf.Min(3, 6 - picks.Count);
            TakeRandom(weapons, weaponSlots, picks);
            TakeRandom(passives, 6 - picks.Count, picks);
            if (picks.Count < 6)
            {
                foreach (var extra in FallbackPassives())
                {
                    if (picks.Count >= 6) break;
                    var dup = false;
                    for (var i = 0; i < picks.Count; i++)
                        if (picks[i].Title == extra.Title) { dup = true; break; }
                    if (!dup) picks.Add(extra);
                }
            }

            return picks;
        }

        static void TakeRandom(List<UpgradeOption> source, int count, List<UpgradeOption> dest)
        {
            var used = new HashSet<string>();
            for (var i = 0; i < dest.Count; i++) used.Add(dest[i].Title);
            while (dest.Count < 99 && count > 0 && source.Count > 0)
            {
                var index = UnityEngine.Random.Range(0, source.Count);
                var opt = source[index];
                source.RemoveAt(index);
                if (opt == null || !used.Add(opt.Title)) continue;
                dest.Add(opt);
                count--;
            }
        }

        public void Choose(UpgradeOption option)
        {
            if (option != null)
            {
                option.Apply();
                Record(option);
                GameFeel.Ui();
            }
            _pendingOffers = Mathf.Max(0, _pendingOffers - 1);
            if (_pendingOffers > 0)
                ShowNextOffer(false);
            else
                GameSession.SetUpgradePause(false);
            RunSave.Capture();
        }

        void Record(UpgradeOption option)
        {
            if (option.FusionHint)
            {
                Passives.Add(new TraitRecord
                {
                    Title = option.Title,
                    Mark = "合",
                    Color = new Color(1f, 0.82f, 0.32f),
                    IsWeapon = true,
                    Stacks = 1
                });
                return;
            }
            if (option.IsWeapon) return;
            for (var i = 0; i < Passives.Count; i++)
            {
                if (Passives[i].Title == option.Title)
                {
                    Passives[i].Stacks += 1;
                    return;
                }
            }

            Passives.Add(new TraitRecord
            {
                Title = option.Title,
                Mark = option.Mark,
                Color = option.Color,
                IsWeapon = false,
                Stacks = 1
            });
        }

        List<UpgradeOption> BuildPool()
        {
            var pool = new List<UpgradeOption>();
            TryAddPassive(pool, "力量", "Might ×1.18，全局伤害", () => Stats.Might *= 1.18f, "力", "通用");
            TryAddPassive(pool, "冷静", "所有冷却 -10%", () => Stats.CooldownMul *= 0.9f, "冷", "通用");
            TryAddPassive(pool, "扩张", "范围 / 弹体 +12%", () => Stats.Area *= 1.12f, "扩", "通用");
            TryAddPassive(pool, "贯穿", "子弹穿透 +1", () => Stats.Pierce += 1, "穿", "远程");
            TryAddPassive(pool, "扩容", "最大生命 +25 并回满", () => Health.RaiseMaxHp(25f, true), "命", "通用");
            TryAddPassive(pool, "吸血刺", "击中回复 +1.2 生命", () => Stats.LifeSteal += 1.2f, "吸", "通用");
            TryAddPassive(pool, "脉息", "生命回复 +1.2/秒", () => Stats.HealthRegen += 1.2f, "愈", "通用");
            TryAddPassive(pool, "磁吸", "拾取范围 +30%", () => Stats.PickupRadius *= 1.3f, "磁", "通用");
            TryAddPassive(pool, "硬化皮肤", "护甲 +2", () => Stats.Armor += 2f, "甲", "通用");
            TryAddPassive(pool, "冲击", "击退 +25%", () => Stats.Knockback *= 1.25f, "击", "远程");
            TryAddPassive(pool, "闪步", "冲刺冷却 -18%", () => { if (Dash != null) Dash.Cooldown *= 0.82f; }, "闪", "通用");
            TryAddPassive(pool, "锐眼", "暴击率 +8%", () => Stats.CritChance = Mathf.Min(1f, Stats.CritChance + 0.08f), "暴", "通用");
            TryAddPassive(pool, "弱点打击", "暴击伤害 +0.25", () => Stats.CritDamage += 0.25f, "要", "通用");
            TryAddPassive(pool, "破甲", "命中降低目标护甲 4 点，最多 2 层", () => Stats.ArmorBreak = true, "破", "通用");
            TryAddPassive(pool, "瞬吸", "拾取经验球回复 4 点生命", () => Stats.SiphonXp = true, "汲", "通用");
            TryAddPassive(pool, "疾冲", "冲刺冷却 ×0.85，无敌 +0.04 秒", () =>
            {
                if (Dash != null) Dash.Cooldown *= 0.85f;
                Stats.DashIFrameBonus += 0.04f;
            }, "疾", "通用");
            TryAddPassive(pool, "腐蚀抗性", "腐蚀地面伤害降低 40%，上限 70%", () => Stats.HazardResist = Mathf.Min(0.7f, Stats.HazardResist + 0.4f), "蚀", "通用");
            TryAddPassive(pool, "血脉蔓延", "所有血脉 DOT 持续时间 +22%", () => Stats.Duration *= 1.22f, "蔓", "持续");
            TryAddPassive(pool, "屠戮", "对血量低于 35% 的敌人伤害 +24%", () => Stats.ExecuteMul += 0.24f, "屠", "通用");
            TryAddPassive(pool, "掠影", "冲刺结束后移速短暂提升", () => Stats.FrenzyMove += 0.06f, "掠", "通用");
            TryAddPassive(pool, "远程专精", "投射物更快，远程伤害提高", () => { Stats.ProjectileSpeed *= 1.22f; Stats.Might *= 1.13f; }, "远", "远程");
            TryAddPassive(pool, "近战专精", "近战范围与伤害提高", () => { Stats.Area *= 1.16f; Stats.Might *= 1.14f; }, "近", "近战");
            TryAddPassive(pool, "连环链电", "链式电击距离与伤害提高", () => Stats.Duration *= 1.08f, "链", "远程");
            TryAddPassive(pool, "寒冰浸透", "冻结更久", () => Stats.Duration *= 1.1f, "浸", "持续");
            TryAddPassive(pool, "瘟疫扩散", "瘟疫传染半径提高", () => Stats.Area *= 1.06f, "疫", "持续");
            TryAddPassive(pool, "圣光庇佑", "圣光触发时临时护甲", () => Stats.Armor += 1f, "圣", "持续");
            TryAddPassive(pool, "混沌畸变", "混沌伤害上限提高", () => Stats.Might *= 1.06f, "混", "持续");
            TryAddPassive(pool, "盾反", "护盾时近战伤害提高", () => Stats.Might *= 1.05f, "盾", "近战");

            foreach (var wid in WeaponCatalog.Commons)
                if (WeaponCatalog.Unlocked(wid))
                    AddWeaponOffers(pool, wid, WeaponCatalog.Title(wid), WeaponCatalog.Blurb(wid));
            TryRarePassives(pool);
            PadPool(pool);
            return pool;
        }

        void PadPool(List<UpgradeOption> pool)
        {
            foreach (var extra in FallbackPassives())
            {
                if (pool.Count >= 6) return;
                var exists = false;
                for (var i = 0; i < pool.Count; i++)
                    if (pool[i].Title == extra.Title) { exists = true; break; }
                if (!exists) pool.Add(extra);
            }
        }

        List<UpgradeOption> FallbackPassives()
        {
            return new List<UpgradeOption>
            {
                new("力量", "Might ×1.18", () => Stats.Might *= 1.18f, "力"),
                new("扩容", "最大生命 +25 并回满", () => Health.RaiseMaxHp(25f, true), "命"),
                new("磁吸", "拾取范围 +30%", () => Stats.PickupRadius *= 1.3f, "磁"),
                new("冷静", "所有冷却 -10%", () => Stats.CooldownMul *= 0.9f, "冷"),
                new("脉息", "生命回复 +1.2/秒", () => Stats.HealthRegen += 1.2f, "愈")
            };
        }

        int StacksOf(string title)
        {
            for (var i = 0; i < Passives.Count; i++)
                if (Passives[i].Title == title) return Passives[i].Stacks;
            return 0;
        }

        bool KitHas(WeaponCatalog.WeaponFamily family)
        {
            if (WeaponCatalog.FamilyOf(UniqueCatalog.WeaponOf(GameInstaller.CurrentUnique)) == family)
                return true;
            if (Loadout == null) return false;
            foreach (var id in Loadout.Owned)
                if (WeaponCatalog.FamilyOf(id) == family) return true;
            return false;
        }

        bool HasBullets()
        {
            var unique = GameInstaller.CurrentUnique;
            if (unique == UniqueId.Hymn || unique == UniqueId.Spiral) return true;
            if (Loadout == null) return false;
            foreach (var id in Loadout.Owned)
            {
                if (UniqueCatalog.IsUniqueWeapon(id)) continue;
                if (WeaponCatalog.FamilyOf(id) == WeaponCatalog.WeaponFamily.Ranged && id != WeaponId.Nova)
                    return true;
            }
            return false;
        }

        bool PassiveAllowed(string title)
        {
            var unique = GameInstaller.CurrentUnique;
            var vein = GameInstaller.CurrentVein;
            switch (title)
            {
                case "冲击": return unique == UniqueId.Bolt;
                case "贯穿": return HasBullets();
                case "远程专精": return KitHas(WeaponCatalog.WeaponFamily.Ranged);
                case "近战专精":
                case "盾反": return KitHas(WeaponCatalog.WeaponFamily.Melee);
                case "瘟疫扩散": return vein == VeinId.Plague || KitHas(WeaponCatalog.WeaponFamily.Dot);
                case "寒冰浸透": return vein == VeinId.Ice || (Loadout != null && (Loadout.Owns(WeaponId.Frost) || Loadout.Owns(WeaponId.IceDisc) || Loadout.Owns(WeaponId.Aurora)));
                case "连环链电": return vein == VeinId.Thunder || unique == UniqueId.Bolt || (Loadout != null && Loadout.Owns(WeaponId.ChainBall));
                case "圣光庇佑": return vein == VeinId.Holy || unique == UniqueId.Star || (Loadout != null && (Loadout.Owns(WeaponId.HolyAura) || Loadout.Owns(WeaponId.Hammer)));
                case "混沌畸变": return vein == VeinId.Chaos || (Loadout != null && Loadout.Owns(WeaponId.ChaosShot));
                case "血脉蔓延": return true;
                default: return true;
            }
        }

        void TryAddPassive(List<UpgradeOption> pool, string title, string desc, Action apply, string mark, string category = "通用")
        {
            if (!PassiveAllowed(title)) return;
            if (StacksOf(title) >= 6) return;
            if (StacksOf(title) >= 4 && UnityEngine.Random.value >= 0.5f) return;
            var opt = new UpgradeOption(title, desc, apply, mark, false, category);
            pool.Add(opt);
            if (WeightBoost(title)) pool.Add(opt);
        }

        bool WeightBoost(string title)
        {
            var h = GameInstaller.CurrentHero;
            var v = GameInstaller.CurrentVein;
            var u = GameInstaller.CurrentUnique;
            if (h == HeroId.Liz && (title == "锐眼" || title == "弱点打击")) return true;
            if (h == HeroId.Karen && (title == "疾冲" || title == "闪步" || title == "掠影")) return true;
            if (h == HeroId.Mora && (title == "血脉蔓延" || title == "瘟疫扩散" || title == "破甲")) return true;
            if (h == HeroId.Antalo && (title == "硬化皮肤" || title == "扩容" || title == "盾反")) return true;
            if (v == VeinId.Fire && title == "血脉蔓延") return true;
            if (u == UniqueId.Hymn && title == "远程专精") return true;
            if (u == UniqueId.Blade && title == "近战专精") return true;
            return false;
        }

        public void GrantRare(string title)
        {
            GrantRareFlag(title);
            Record(new UpgradeOption(title, "稀有机制", () => { }, "稀"));
        }

        void TryRarePassives(List<UpgradeOption> pool)
        {
            if (UnityEngine.Random.value > (MetaProgress.RarePoolUnlocked ? 0.18f : 0.12f)) return;
            if (pool.Count > 8) return;
            string[] names = MetaProgress.RarePoolUnlocked
                ? new[]
                {
                    "血脉回响", "疾风冲刺", "武器分流", "迅疾血脉", "爆破血脉", "防护汲取",
                    "溃腐传导", "奔袭利刃", "固化石化", "雷霆过载", "牺牲武装", "生命涌动"
                }
                : new[] { "血脉回响", "疾风冲刺", "武器分流" };
            var pick = names[UnityEngine.Random.Range(0, names.Length)];
            if (StacksOf(pick) > 0) return;
            pool.Add(new UpgradeOption(pick, RareDesc(pick), () => GrantRareFlag(pick), "稀"));
        }

        static string RareDesc(string title)
        {
            switch (title)
            {
                case "血脉回响": return "同一目标可同时带两种血脉状态";
                case "疾风冲刺": return "冲刺对周围造成伤害";
                case "武器分流": return "专武命中有概率把部分伤害分给周围";
                case "迅疾血脉": return "触发血脉时短暂移速 +12%";
                case "爆破血脉": return "血脉爆发 +35%，DOT −20%";
                case "防护汲取": return "触发血脉获得短暂护盾，最多 3 层";
                case "溃腐传导": return "腐蚀每跳可能传播，蚀甲再 +3";
                case "奔袭利刃": return "冲刺伤害可暴击并吃力量";
                case "固化石化": return "石化更久，近战打石化目标更高";
                case "雷霆过载": return "链式电击可再跳一次";
                case "牺牲武装": return "武器伤害 +30%，最大生命 −18";
                case "生命涌动": return "每损失 20% 最大生命，全局伤害 +12%";
                default: return "稀有机制被动";
            }
        }

        void GrantRareFlag(string title)
        {
            if (title == "血脉回响") RunConfig.DualVein = true;
            if (title == "疾风冲刺") RunConfig.DashPulse = true;
            if (title == "武器分流") RunConfig.WeaponSplit = true;
            if (title == "牺牲武装" && Stats != null && Health != null)
            {
                Stats.Might *= 1.3f;
                Health.RaiseMaxHp(-18f, false);
            }
            if (title == "生命涌动") Stats.FrenzyDamage += 0.12f;
        }

        void AddUniqueOffers(List<UpgradeOption> pool)
        {
            if (Loadout == null) return;
            if (RunConfig.HasMod(RelicMod.WeaponTrial)) return;
            var unique = GameInstaller.CurrentUnique;
            var id = UniqueCatalog.WeaponOf(unique);
            if (!Loadout.Owns(id)) return;
            var level = Loadout.LevelOf(id);
            if (level >= 5) return;
            pool.Add(new UpgradeOption(
                $"{UniqueCatalog.Title(unique)} Lv.{level + 1}",
                UniqueCatalog.UpgradeBlurb(unique, level + 1),
                () => Loadout.Upgrade(id),
                "武",
                true,
                WeaponCatalog.FamilyTitle(WeaponCatalog.FamilyOf(id))));
        }

        void AddWeaponOffers(List<UpgradeOption> pool, WeaponId id, string title, string desc)
        {
            if (Loadout == null || UniqueCatalog.IsUniqueWeapon(id)) return;
            if (!Loadout.Owns(id))
            {
                if (!GameSettings.Developer && !KitHas(WeaponCatalog.FamilyOf(id))) return;
                if (!Loadout.CanUnlock)
                {
                    if (RunConfig.ReplacesLeft <= 0) return;
                    var swap = new UpgradeOption(
                        $"替换 · {title}",
                        $"武器栏已满。覆盖一件普通武器，新武器从 Lv.1 开始。本局剩余替换 {RunConfig.ReplacesLeft}/{RunConfig.MaxReplaces}。专武不能替换。\n" + desc,
                        () => { },
                        "换",
                        true,
                        WeaponCatalog.FamilyTitle(WeaponCatalog.FamilyOf(id)));
                    swap.ReplaceHint = true;
                    swap.SourceWeapon = id;
                    StampWeaponCard(swap, id, swap.Description, false);
                    pool.Add(swap);
                    return;
                }

                var opt = new UpgradeOption($"新武器 · {title}", desc, () => Loadout.Unlock(id), "武", true,
                    WeaponCatalog.FamilyTitle(WeaponCatalog.FamilyOf(id)));
                StampWeaponCard(opt, id, desc, false);
                pool.Add(opt);
                if (RunConfig.HasMod(RelicMod.WeaponTrial)) pool.Add(opt);
                return;
            }

            if (Loadout.LevelOf(id) >= 5) return;
            var lv = Loadout.LevelOf(id);
            var up = new UpgradeOption($"{title}  {lv}→{lv + 1}", desc, () => Loadout.Upgrade(id), "武", true,
                WeaponCatalog.FamilyTitle(WeaponCatalog.FamilyOf(id)));
            StampWeaponCard(up, id, desc, true);
            pool.Add(up);
        }

        void StampWeaponCard(UpgradeOption opt, WeaponId id, string desc, bool owned)
        {
            opt.SourceWeapon = id;
            var fusion = FusionCatalog.CardHint(id, Loadout);
            if (owned)
            {
                opt.OwnedHint = true;
                var lv = Loadout.LevelOf(id);
                opt.Title = $"{WeaponCatalog.Title(id)}  {lv}→{lv + 1}";
                var ownLine = "已装备 · 强化现有模块，不新占栏位。";
                if (!string.IsNullOrEmpty(fusion))
                {
                    opt.FusionHint = true;
                    opt.Description = ownLine + "\n" + fusion + "\n" + desc;
                }
                else opt.Description = ownLine + "\n" + desc;
                return;
            }

            if (string.IsNullOrEmpty(fusion)) return;
            opt.FusionHint = true;
            opt.Description = fusion + "\n" + desc;
        }
    }
}
