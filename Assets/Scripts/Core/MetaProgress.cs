using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Veinfire
{
    public enum GameModeId
    {
        Standard = 0,
        Short = 1,
        Endless = 2,
        BossRush = 3
    }

    public enum RelicMod
    {
        None = 0,
        VeinResonance = 1,
        GaleField = 2,
        WeaponTrial = 3,
        RotSpread = 4,
        DashWay = 5,
        FleshOffering = 6,
        ShieldWall = 7
    }

    public enum CurseId
    {
        None = 0,
        Armor = 1,
        Swift = 2,
        Bleed = 3,
        Miasma = 4,
        EliteFury = 5,
        SparseLoot = 6,
        DeathEcho = 7,
        FrostCalamity = 8,
        PlagueSpread = 9,
        ChaosMutate = 10
    }

    public enum MetaNodeId
    {
        WeaponTrial, GaleField, Fusion, Area,
        VeinResonance, Duration, Resist,
        Pickup, LifeSteal, ModeShort, ModeEndless, ModeBossRush,
        CurseArmor, CurseSwift, CurseBleed,
        UniquePack, WeaponPack1, WeaponPack2, WeaponPack3, FusionPack, ModPack2, RelicPack, WeaponHaste, CardReroll,
        VeinPack1, VeinPack2, VeinLinks, RareVein, VeinMods, VeinDot,
        HeroPack, EventPack, CursePack2, MaxHp, HunterCrit, ArmorBonus
    }

    [Serializable]
    public sealed class RunArchiveRow
    {
        public int mode, map, star, hero, vein, unique;
        public bool won;
        public float time;
        public int modA, modB, curseA, curseB, curseC;
    }

    [Serializable]
    public sealed class MetaData
    {
        public int version = 2;
        public int MetaFileVersion = 26;
        public int embers;
        public bool classic;
        public float pickupBonus;
        public float lifeStealBonus;
        public float areaBonus;
        public float durationBonus;
        public float resistBonus;
        public float hasteBonus;
        public float veinDotBonus;
        public float hpBonus;
        public float critBonus;
        public float armorBonus;
        public bool unlockWeaponTrial;
        public bool unlockGaleField;
        public bool unlockVeinResonance;
        public bool unlockFusion;
        public bool unlockShort;
        public bool unlockEndless;
        public bool unlockBossRush;
        public bool unlockCurseArmor;
        public bool unlockCurseSwift;
        public bool unlockCurseBleed;
        public bool unlockUniquePack;
        public bool unlockWeapon1;
        public bool unlockWeapon2;
        public bool unlockWeapon3;
        public bool unlockFusionPack;
        public bool unlockMod2;
        public bool unlockRelicPack;
        public bool unlockVeinPack1;
        public bool unlockVeinPack2;
        public bool unlockVeinLinks;
        public bool unlockRareVein;
        public bool unlockVeinMods;
        public bool unlockHeroPack;
        public bool unlockEventPack;
        public bool unlockCursePack2;
        public bool unlockReroll;
        public int wins;
        public int deaths;
        public int bestKills;
        public float bestTime;
        public bool achWin;
        public bool achDeath;
        public bool achBoss;
        public bool achKills;
        public bool seenSwarm;
        public bool seenRunner;
        public bool seenTank;
        public bool seenSpitter;
        public bool seenExploder;
        public bool seenElite;
        public bool seenBoss;
        public int starVein;
        public int starBone;
        public int starAsh;
        public bool caveClear;
        public bool infernalClear;
        public string farmKey = "";
        public long farmUtc;
        public int farmHits;
        public bool achFuse;
        public bool achReroll;
        public bool achStar3;
        public RunArchiveRow[] archive = Array.Empty<RunArchiveRow>();
    }

    public static class MetaProgress
    {
        const string FileName = "veinfire_meta.json";
        static MetaData _data;

        public static MetaData Data
        {
            get
            {
                if (_data == null) Load();
                return _data;
            }
        }

        static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static void Load()
        {
            try
            {
                if (File.Exists(Path))
                    _data = JsonUtility.FromJson<MetaData>(File.ReadAllText(Path));
            }
            catch
            {
                _data = null;
            }

            if (_data == null || _data.version < 2)
                _data = new MetaData();
            if (_data.archive == null) _data.archive = Array.Empty<RunArchiveRow>();
            _data.classic = false;
            _data.MetaFileVersion = 26;
            ClampBonuses();
        }

        public static void Save()
        {
            try
            {
                File.WriteAllText(Path, JsonUtility.ToJson(Data, true));
            }
            catch
            {
                PlayerPrefs.SetInt("vf_embers", Data.embers);
                PlayerPrefs.Save();
            }
        }

        public static bool Classic => false;

        public static void SetClassic(bool on) { }

        public static int Settle(bool won, float time, int kills, int bosses)
        {
            var spec = MapCatalog.Current;
            var baseEmber = Mathf.Max(20, spec.ember);
            var mul = Mathf.Clamp(RunConfig.EmberMul, 1f, BalanceTables.Bal.emberCurseClamp != null && BalanceTables.Bal.emberCurseClamp.Length > 1
                ? BalanceTables.Bal.emberCurseClamp[1] : 2.2f);
            var modeMul = RunConfig.Mode == GameModeId.Short ? 0.85f : RunConfig.Mode == GameModeId.Endless ? 1.1f : 1f;
            var decay = FarmDecay(spec);
            var raw = (baseEmber + kills * 0.18f + time / 8f + bosses * 22f) * mul * modeMul * decay;
            var floor = Mathf.RoundToInt(baseEmber * BalanceTables.Bal.failEmberFloor);
            var gain = Mathf.Max(won ? Mathf.Max(40, floor) : floor, Mathf.RoundToInt(raw));
            Data.embers += gain;

            if (RunConfig.DeveloperRun)
            {
                Save();
                return gain;
            }

            if (won) Data.wins += 1;
            else Data.deaths += 1;
            Data.bestKills = Mathf.Max(Data.bestKills, kills);
            Data.bestTime = Mathf.Max(Data.bestTime, time);
            if (won) Data.achWin = true;
            if (!won) Data.achDeath = true;
            if (bosses > 0) Data.achBoss = true;
            if (kills >= 100) Data.achKills = true;
            if (RunConfig.FusionSlots >= 2) Data.achFuse = true;
            if (RunConfig.RerollsLeftAtEnd == 0) Data.achReroll = true;
            if (RunConfig.MapStar >= 3 && won) Data.achStar3 = true;
            if (won) MarkMapClear(GameInstaller.CurrentMap, RunConfig.MapStar);
            PushArchive(won, time);
            Save();
            return gain;
        }

        static float FarmDecay(MapSpec spec)
        {
            if (RunConfig.Mode == GameModeId.Endless) return 1f;
            var key = RunConfig.Mode == GameModeId.BossRush
                ? $"boss-{spec.MapId}-{spec.star}"
                : $"{spec.MapId}-{spec.star}";
            var now = DateTime.UtcNow.Ticks;
            var window = TimeSpan.FromMinutes(BalanceTables.Bal.farmWindowMin).Ticks;
            var clear = TimeSpan.FromMinutes(BalanceTables.Bal.farmClearMin).Ticks;
            if (string.IsNullOrEmpty(Data.farmKey) || Data.farmKey != key || now - Data.farmUtc > clear)
            {
                Data.farmKey = key;
                Data.farmUtc = now;
                Data.farmHits = 1;
                return 1f;
            }

            if (now - Data.farmUtc <= window)
                Data.farmHits += 1;
            else
            {
                Data.farmUtc = now;
                Data.farmHits = 1;
                return 1f;
            }

            Data.farmUtc = now;
            var extra = Mathf.Max(0, Data.farmHits - 1);
            return Mathf.Max(BalanceTables.Bal.farmFloor, 1f - extra * BalanceTables.Bal.farmDecay);
        }

        static void MarkMapClear(MapId id, int star)
        {
            switch (id)
            {
                case MapId.BoneCloister: Data.starBone = Mathf.Max(Data.starBone, star); break;
                case MapId.AshMarsh: Data.starAsh = Mathf.Max(Data.starAsh, star); break;
                case MapId.CaveHollow: Data.caveClear = true; break;
                case MapId.InfernalRuin: Data.infernalClear = true; break;
                default: Data.starVein = Mathf.Max(Data.starVein, star); break;
            }
        }

        public static bool MapUnlocked(MapId id, int star)
        {
            if (GameSettings.Developer) return true;
            star = MapCatalog.ClampStar(id, star);
            if (id == MapId.CaveHollow)
                return Data.starVein >= 3 || Data.starBone >= 3 || Data.starAsh >= 3;
            if (id == MapId.InfernalRuin) return Data.caveClear;
            if (star <= 1) return true;
            var beaten = id == MapId.BoneCloister ? Data.starBone : id == MapId.AshMarsh ? Data.starAsh : Data.starVein;
            return beaten >= star - 1;
        }

        public static bool ResetTree()
        {
            var cost = Mathf.Max(1, BalanceTables.Bal.metaResetCost);
            if (Data.embers < cost) return false;
            Data.embers -= cost;
            Data.pickupBonus = Data.lifeStealBonus = Data.areaBonus = 0f;
            Data.durationBonus = Data.resistBonus = Data.hasteBonus = 0f;
            Data.veinDotBonus = Data.hpBonus = Data.critBonus = Data.armorBonus = 0f;
            Save();
            return true;
        }

        static void ClampBonuses()
        {
            var cap = BalanceTables.Bal.metaCaps;
            Data.hasteBonus = Mathf.Min(cap.haste, Data.hasteBonus);
            Data.veinDotBonus = Mathf.Min(cap.veinDot, Data.veinDotBonus);
            Data.hpBonus = Mathf.Min(cap.hp, Data.hpBonus);
            Data.critBonus = Mathf.Min(cap.crit, Data.critBonus);
            Data.armorBonus = Mathf.Min(cap.armor, Data.armorBonus);
        }

        static void PushArchive(bool won, float time)
        {
            var row = new RunArchiveRow
            {
                mode = (int)RunConfig.Mode,
                map = (int)GameInstaller.CurrentMap,
                star = RunConfig.MapStar,
                hero = (int)GameInstaller.CurrentHero,
                vein = (int)GameInstaller.CurrentVein,
                unique = (int)GameInstaller.CurrentUnique,
                won = won,
                time = time,
                modA = (int)RunConfig.ModA,
                modB = (int)RunConfig.ModB,
                curseA = (int)RunConfig.CurseA,
                curseB = (int)RunConfig.CurseB,
                curseC = (int)RunConfig.CurseC
            };
            var list = new List<RunArchiveRow>(Data.archive ?? Array.Empty<RunArchiveRow>()) { row };
            while (list.Count > 30) list.RemoveAt(0);
            Data.archive = list.ToArray();
        }

        public static bool HeroUnlocked(HeroId id) => GameSettings.Developer || (int)id <= 2 || Data.unlockHeroPack;
        public static bool VeinUnlocked(VeinId id)
        {
            if (GameSettings.Developer || (int)id <= 2) return true;
            if (id == VeinId.Corrode || id == VeinId.Thunder) return Data.unlockVeinPack1;
            return Data.unlockVeinPack2;
        }

        public static bool UniqueUnlocked(UniqueId id) => GameSettings.Developer || (int)id <= 2 || Data.unlockUniquePack;

        public static bool BuyPickup() => BuyNode(MetaNodeId.Pickup);
        public static bool BuyLifeSteal() => BuyNode(MetaNodeId.LifeSteal);

        public static bool FusionUnlocked => GameSettings.Developer || Data.unlockFusion;

        public static bool ModeUnlocked(GameModeId id)
        {
            if (GameSettings.Developer) return true;
            switch (id)
            {
                case GameModeId.Short: return Data.unlockShort;
                case GameModeId.Endless: return Data.unlockEndless;
                case GameModeId.BossRush: return Data.unlockBossRush;
                default: return true;
            }
        }

        public static bool ModUnlocked(RelicMod id)
        {
            if (id == RelicMod.None || GameSettings.Developer) return true;
            switch (id)
            {
                case RelicMod.WeaponTrial: return Data.unlockWeaponTrial;
                case RelicMod.GaleField: return Data.unlockGaleField;
                case RelicMod.VeinResonance: return Data.unlockVeinResonance;
                case RelicMod.RotSpread:
                case RelicMod.DashWay:
                case RelicMod.FleshOffering:
                case RelicMod.ShieldWall: return Data.unlockMod2 || Data.unlockVeinMods;
                default: return false;
            }
        }

        public static bool CurseUnlocked(CurseId id)
        {
            if (id == CurseId.None || GameSettings.Developer) return true;
            switch (id)
            {
                case CurseId.Armor: return Data.unlockCurseArmor;
                case CurseId.Swift: return Data.unlockCurseSwift;
                case CurseId.Bleed: return Data.unlockCurseBleed;
                default: return Data.unlockCursePack2;
            }
        }

        public static bool RelicPackUnlocked => GameSettings.Developer || Data.unlockRelicPack;
        public static bool RarePoolUnlocked => GameSettings.Developer || Data.unlockRareVein;

        public static MetaNodeId[] BranchNodes(int branch)
        {
            switch (branch)
            {
                case 1: return new[]
                {
                    MetaNodeId.VeinResonance, MetaNodeId.VeinPack1, MetaNodeId.VeinPack2, MetaNodeId.VeinLinks,
                    MetaNodeId.RareVein, MetaNodeId.VeinMods, MetaNodeId.VeinDot
                };
                case 2: return new[]
                {
                    MetaNodeId.ModeShort, MetaNodeId.HeroPack, MetaNodeId.ModeEndless, MetaNodeId.CurseArmor,
                    MetaNodeId.EventPack, MetaNodeId.ModeBossRush, MetaNodeId.CursePack2, MetaNodeId.MaxHp,
                    MetaNodeId.HunterCrit, MetaNodeId.ArmorBonus
                };
                default: return new[]
                {
                    MetaNodeId.Fusion, MetaNodeId.UniquePack, MetaNodeId.WeaponPack1, MetaNodeId.WeaponPack2,
                    MetaNodeId.WeaponPack3, MetaNodeId.FusionPack, MetaNodeId.WeaponTrial, MetaNodeId.ModPack2,
                    MetaNodeId.RelicPack, MetaNodeId.WeaponHaste
                };
            }
        }

        public static string BranchTitle(int branch)
        {
            switch (branch)
            {
                case 1: return "血脉分支";
                case 2: return "生存分支";
                default: return "军械分支";
            }
        }

        public static string BranchBlurb(int branch)
        {
            switch (branch)
            {
                case 1: return "解锁血脉模组，以及状态时长、腐蚀抗性等微加成。";
                case 2: return "解锁模式、诅咒，以及生命 / 暴击 / 护甲微加成。";
                default: return "解锁武器、融合、模组，以及冷却微加成。";
            }
        }

        public static bool BuyNode(MetaNodeId id)
        {
            if (Classic) return false;
            var cost = NodeCost(id);
            if (Data.embers < cost) return false;
            if (!PrereqMet(id) || NodeMaxed(id)) return false;

            Data.embers -= cost;
            switch (id)
            {
                case MetaNodeId.WeaponTrial:
                    Data.unlockWeaponTrial = true;
                    Data.unlockGaleField = true;
                    Data.unlockVeinResonance = true;
                    break;
                case MetaNodeId.GaleField: Data.unlockGaleField = true; break;
                case MetaNodeId.Fusion: Data.unlockFusion = true; break;
                case MetaNodeId.VeinResonance: Data.unlockVeinResonance = true; break;
                case MetaNodeId.ModeShort: Data.unlockShort = true; break;
                case MetaNodeId.ModeEndless: Data.unlockEndless = true; break;
                case MetaNodeId.ModeBossRush: Data.unlockBossRush = true; break;
                case MetaNodeId.CurseArmor:
                    Data.unlockCurseArmor = true;
                    Data.unlockCurseSwift = true;
                    Data.unlockCurseBleed = true;
                    break;
                case MetaNodeId.CurseSwift: Data.unlockCurseSwift = true; break;
                case MetaNodeId.CurseBleed: Data.unlockCurseBleed = true; break;
                case MetaNodeId.Pickup: Data.pickupBonus = Mathf.Min(0.6f, Data.pickupBonus + 0.2f); break;
                case MetaNodeId.LifeSteal: Data.lifeStealBonus = Mathf.Min(0.3f, Data.lifeStealBonus + 0.1f); break;
                case MetaNodeId.Area: Data.areaBonus = Mathf.Min(0.09f, Data.areaBonus + 0.03f); break;
                case MetaNodeId.Duration: Data.durationBonus = Mathf.Min(0.15f, Data.durationBonus + 0.05f); break;
                case MetaNodeId.Resist: Data.resistBonus = Mathf.Min(0.24f, Data.resistBonus + 0.08f); break;
                case MetaNodeId.UniquePack: Data.unlockUniquePack = true; break;
                case MetaNodeId.WeaponPack1: Data.unlockWeapon1 = true; break;
                case MetaNodeId.WeaponPack2: Data.unlockWeapon2 = true; break;
                case MetaNodeId.WeaponPack3: Data.unlockWeapon3 = true; break;
                case MetaNodeId.FusionPack: Data.unlockFusionPack = true; break;
                case MetaNodeId.ModPack2: Data.unlockMod2 = true; break;
                case MetaNodeId.RelicPack: Data.unlockRelicPack = true; break;
                case MetaNodeId.WeaponHaste:
                    Data.hasteBonus = GameSettings.Developer
                        ? BalanceTables.Bal.metaCaps.haste
                        : Mathf.Min(BalanceTables.Bal.metaCaps.haste, Data.hasteBonus + 0.02f);
                    break;
                case MetaNodeId.VeinPack1: Data.unlockVeinPack1 = true; break;
                case MetaNodeId.VeinPack2: Data.unlockVeinPack2 = true; break;
                case MetaNodeId.VeinLinks: Data.unlockVeinLinks = true; break;
                case MetaNodeId.RareVein: Data.unlockRareVein = true; break;
                case MetaNodeId.VeinMods: Data.unlockVeinMods = true; break;
                case MetaNodeId.VeinDot:
                    Data.veinDotBonus = GameSettings.Developer
                        ? BalanceTables.Bal.metaCaps.veinDot
                        : Mathf.Min(BalanceTables.Bal.metaCaps.veinDot, Data.veinDotBonus + 0.04f);
                    break;
                case MetaNodeId.HeroPack: Data.unlockHeroPack = true; break;
                case MetaNodeId.EventPack: Data.unlockEventPack = true; break;
                case MetaNodeId.CursePack2: Data.unlockCursePack2 = true; Data.unlockCurseArmor = Data.unlockCurseSwift = Data.unlockCurseBleed = true; break;
                case MetaNodeId.MaxHp:
                    Data.hpBonus = GameSettings.Developer
                        ? BalanceTables.Bal.metaCaps.hp
                        : Mathf.Min(BalanceTables.Bal.metaCaps.hp, Data.hpBonus + 5f);
                    break;
                case MetaNodeId.HunterCrit:
                    Data.critBonus = GameSettings.Developer
                        ? BalanceTables.Bal.metaCaps.crit
                        : Mathf.Min(BalanceTables.Bal.metaCaps.crit, Data.critBonus + 0.015f);
                    break;
                case MetaNodeId.ArmorBonus:
                    Data.armorBonus = GameSettings.Developer
                        ? BalanceTables.Bal.metaCaps.armor
                        : Mathf.Min(BalanceTables.Bal.metaCaps.armor, Data.armorBonus + 0.5f);
                    break;
                case MetaNodeId.CardReroll: Data.unlockReroll = true; break;
            }

            Save();
            return true;
        }

        public static bool NodeMaxed(MetaNodeId id)
        {
            if (GameSettings.Developer) return true;

            switch (id)
            {
                case MetaNodeId.WeaponTrial: return Data.unlockWeaponTrial;
                case MetaNodeId.GaleField: return Data.unlockGaleField;
                case MetaNodeId.Fusion: return Data.unlockFusion;
                case MetaNodeId.VeinResonance: return Data.unlockVeinResonance;
                case MetaNodeId.ModeShort: return Data.unlockShort;
                case MetaNodeId.ModeEndless: return Data.unlockEndless;
                case MetaNodeId.ModeBossRush: return Data.unlockBossRush;
                case MetaNodeId.CurseArmor: return Data.unlockCurseArmor;
                case MetaNodeId.CurseSwift: return Data.unlockCurseSwift;
                case MetaNodeId.CurseBleed: return Data.unlockCurseBleed;
                case MetaNodeId.Pickup: return Data.pickupBonus >= 0.6f - 0.001f;
                case MetaNodeId.LifeSteal: return Data.lifeStealBonus >= 0.3f - 0.001f;
                case MetaNodeId.Area: return Data.areaBonus >= 0.09f - 0.001f;
                case MetaNodeId.Duration: return Data.durationBonus >= 0.15f - 0.001f;
                case MetaNodeId.Resist: return Data.resistBonus >= 0.24f - 0.001f;
                case MetaNodeId.UniquePack: return Data.unlockUniquePack;
                case MetaNodeId.WeaponPack1: return Data.unlockWeapon1;
                case MetaNodeId.WeaponPack2: return Data.unlockWeapon2;
                case MetaNodeId.WeaponPack3: return Data.unlockWeapon3;
                case MetaNodeId.FusionPack: return Data.unlockFusionPack;
                case MetaNodeId.ModPack2: return Data.unlockMod2;
                case MetaNodeId.RelicPack: return Data.unlockRelicPack;
                case MetaNodeId.WeaponHaste: return Data.hasteBonus >= BalanceTables.Bal.metaCaps.haste - 0.001f;
                case MetaNodeId.VeinPack1: return Data.unlockVeinPack1;
                case MetaNodeId.VeinPack2: return Data.unlockVeinPack2;
                case MetaNodeId.VeinLinks: return Data.unlockVeinLinks;
                case MetaNodeId.RareVein: return Data.unlockRareVein;
                case MetaNodeId.VeinMods: return Data.unlockVeinMods;
                case MetaNodeId.VeinDot: return Data.veinDotBonus >= BalanceTables.Bal.metaCaps.veinDot - 0.001f;
                case MetaNodeId.HeroPack: return Data.unlockHeroPack;
                case MetaNodeId.EventPack: return Data.unlockEventPack;
                case MetaNodeId.CursePack2: return Data.unlockCursePack2;
                case MetaNodeId.MaxHp: return Data.hpBonus >= BalanceTables.Bal.metaCaps.hp - 0.001f;
                case MetaNodeId.HunterCrit: return Data.critBonus >= BalanceTables.Bal.metaCaps.crit - 0.001f;
                case MetaNodeId.ArmorBonus: return Data.armorBonus >= BalanceTables.Bal.metaCaps.armor - 0.001f;
                case MetaNodeId.CardReroll: return Data.unlockReroll;
                default: return true;
            }
        }

        public static bool PrereqMet(MetaNodeId id)
        {
            if (GameSettings.Developer) return true;
            switch (id)
            {
                case MetaNodeId.UniquePack: return Data.unlockFusion;
                case MetaNodeId.WeaponPack1: return Data.unlockUniquePack;
                case MetaNodeId.CardReroll: return Data.unlockWeapon1;
                case MetaNodeId.WeaponPack2: return Data.unlockWeapon1;
                case MetaNodeId.WeaponPack3: return Data.unlockWeapon2;
                case MetaNodeId.FusionPack: return Data.unlockFusion;
                case MetaNodeId.ModPack2: return Data.unlockWeaponTrial;
                case MetaNodeId.RelicPack: return Data.unlockMod2;
                case MetaNodeId.ModeEndless:
                case MetaNodeId.HeroPack: return Data.unlockShort;
                case MetaNodeId.ModeBossRush: return Data.unlockEndless;
                case MetaNodeId.EventPack: return Data.unlockEndless;
                case MetaNodeId.CursePack2: return Data.unlockBossRush;
                case MetaNodeId.VeinPack1: return Data.unlockVeinResonance;
                case MetaNodeId.VeinPack2: return Data.unlockVeinPack1;
                case MetaNodeId.VeinLinks:
                case MetaNodeId.RareVein:
                case MetaNodeId.VeinMods: return Data.unlockVeinPack2;
                default: return true;
            }
        }

        static bool IsStatNode(MetaNodeId id)
        {
            switch (id)
            {
                case MetaNodeId.Pickup:
                case MetaNodeId.LifeSteal:
                case MetaNodeId.Area:
                case MetaNodeId.Duration:
                case MetaNodeId.Resist:
                case MetaNodeId.WeaponHaste:
                case MetaNodeId.VeinDot:
                case MetaNodeId.MaxHp:
                case MetaNodeId.HunterCrit:
                case MetaNodeId.ArmorBonus:
                    return true;
                default:
                    return false;
            }
        }

        public static int NodeCost(MetaNodeId id)
        {
            if (GameSettings.Developer && IsStatNode(id)) return 0;
            switch (id)
            {
                case MetaNodeId.WeaponTrial: return 60;
                case MetaNodeId.GaleField: return 55;
                case MetaNodeId.Fusion: return 80;
                case MetaNodeId.Area: return 35;
                case MetaNodeId.VeinResonance: return 50;
                case MetaNodeId.Duration: return 35;
                case MetaNodeId.Resist: return 40;
                case MetaNodeId.Pickup: return 40;
                case MetaNodeId.LifeSteal: return 50;
                case MetaNodeId.ModeShort: return 35;
                case MetaNodeId.ModeEndless: return 75;
                case MetaNodeId.ModeBossRush: return 90;
                case MetaNodeId.CurseArmor: return 40;
                case MetaNodeId.CurseSwift: return 45;
                case MetaNodeId.CurseBleed: return 55;
                case MetaNodeId.CardReroll: return 45;
                case MetaNodeId.UniquePack: return 70;
                case MetaNodeId.WeaponPack1: return 50;
                case MetaNodeId.WeaponPack2: return 65;
                case MetaNodeId.WeaponPack3: return 80;
                case MetaNodeId.FusionPack: return 75;
                case MetaNodeId.ModPack2: return 60;
                case MetaNodeId.RelicPack: return 85;
                case MetaNodeId.WeaponHaste: return 45;
                case MetaNodeId.VeinPack1: return 50;
                case MetaNodeId.VeinPack2: return 70;
                case MetaNodeId.VeinLinks: return 55;
                case MetaNodeId.RareVein: return 60;
                case MetaNodeId.VeinMods: return 55;
                case MetaNodeId.VeinDot: return 40;
                case MetaNodeId.HeroPack: return 55;
                case MetaNodeId.EventPack: return 65;
                case MetaNodeId.CursePack2: return 80;
                case MetaNodeId.MaxHp: return 40;
                case MetaNodeId.HunterCrit: return 40;
                case MetaNodeId.ArmorBonus: return 40;
                default: return 50;
            }
        }

        public static string NodeTitle(MetaNodeId id)
        {
            switch (id)
            {
                case MetaNodeId.WeaponTrial: return "模组解锁Ⅰ";
                case MetaNodeId.GaleField: return "解锁模组 · 疾风战场";
                case MetaNodeId.Fusion: return "解锁 · 普通武器组合进化";
                case MetaNodeId.Area: return "范围微扩 +0.03";
                case MetaNodeId.VeinResonance: return "血脉基础联动";
                case MetaNodeId.Duration: return "状态时长 +0.05";
                case MetaNodeId.Resist: return "腐蚀抗性 +0.08";
                case MetaNodeId.Pickup: return "拾取半径 +0.2";
                case MetaNodeId.LifeSteal: return "吸血 +0.1";
                case MetaNodeId.ModeShort: return "解锁模式 · 速攻生存";
                case MetaNodeId.ModeEndless: return "解锁模式 · 无尽鏖战";
                case MetaNodeId.ModeBossRush: return "解锁模式 · Boss狂袭";
                case MetaNodeId.CurseArmor: return "诅咒解锁Ⅰ";
                case MetaNodeId.CurseSwift: return "解锁诅咒 · 迅疾";
                case MetaNodeId.CurseBleed: return "解锁诅咒 · 流血";
                case MetaNodeId.UniquePack: return "解锁专武Ⅳ";
                case MetaNodeId.WeaponPack1: return "普通武器包Ⅰ";
                case MetaNodeId.WeaponPack2: return "普通武器包Ⅱ";
                case MetaNodeId.WeaponPack3: return "普通武器包Ⅲ";
                case MetaNodeId.FusionPack: return "高级融合配方";
                case MetaNodeId.ModPack2: return "模组解锁Ⅱ";
                case MetaNodeId.RelicPack: return "遗物扩充";
                case MetaNodeId.WeaponHaste: return "武器娴熟 −0.02";
                case MetaNodeId.VeinPack1: return "血脉包Ⅰ";
                case MetaNodeId.VeinPack2: return "血脉包Ⅱ";
                case MetaNodeId.VeinLinks: return "血脉‑专武联动";
                case MetaNodeId.RareVein: return "稀有血脉被动池";
                case MetaNodeId.VeinMods: return "血脉向模组";
                case MetaNodeId.VeinDot: return "血脉浸透 +4%";
                case MetaNodeId.HeroPack: return "解锁角色 · 卡伦 / 莫拉";
                case MetaNodeId.EventPack: return "事件池扩充";
                case MetaNodeId.CursePack2: return "诅咒解锁Ⅱ";
                case MetaNodeId.MaxHp: return "肉体淬炼 +5";
                case MetaNodeId.HunterCrit: return "猎手直觉 +1.5%";
                case MetaNodeId.ArmorBonus: return "硬化底子 +0.5";
                case MetaNodeId.CardReroll: return "卡运 · 每局刷新 +2";
                default: return "";
            }
        }

        public static string NodeStatus(MetaNodeId id)
        {
            if (NodeMaxed(id)) return "已解锁";
            if (!PrereqMet(id)) return "前置锁定";
            if (Data.embers < NodeCost(id)) return "残烬不足";
            if (id == MetaNodeId.Pickup && Data.pickupBonus > 0.01f) return "可升级";
            if (id == MetaNodeId.LifeSteal && Data.lifeStealBonus > 0.01f) return "可升级";
            if (id == MetaNodeId.Area && Data.areaBonus > 0.001f) return "可升级";
            if (id == MetaNodeId.Duration && Data.durationBonus > 0.001f) return "可升级";
            if (id == MetaNodeId.Resist && Data.resistBonus > 0.001f) return "可升级";
            if (id == MetaNodeId.WeaponHaste && Data.hasteBonus > 0.001f) return "可升级";
            if (id == MetaNodeId.VeinDot && Data.veinDotBonus > 0.001f) return "可升级";
            if (id == MetaNodeId.MaxHp && Data.hpBonus > 0.01f) return "可升级";
            if (id == MetaNodeId.HunterCrit && Data.critBonus > 0.001f) return "可升级";
            if (id == MetaNodeId.ArmorBonus && Data.armorBonus > 0.01f) return "可升级";
            return "未解锁";
        }

        public static bool NodeReady(MetaNodeId id)
        {
            return !Classic && !NodeMaxed(id) && PrereqMet(id) && Data.embers >= NodeCost(id);
        }

        public static string NodeDetail(MetaNodeId id)
        {
            switch (id)
            {
                case MetaNodeId.WeaponTrial:
                    return "解锁基础模组：武器试炼、疾风战场、血脉共振。\n可在开战页勾选 0-2 个。不直接加伤害或生命。";
                case MetaNodeId.GaleField:
                    return "本局修饰：疾风战场。\n你移速 +15%，怪物移速 +10%。\n对局更锋利，也更容易被贴身。\n解锁后可在开局作为模组装备。";
                case MetaNodeId.Fusion:
                    return "允许普通武器组合进化。\n环绕毒牙与血爆脉冲均 5 级 → 熔核环爆。\n霜脉光环与裂空鞭笞均 5 级 → 霜裂长鞭。\n合体占 1 栏。前置无。";
                case MetaNodeId.Area:
                    return "全局武器范围永久 +0.03，上限 0.09。\n属于克制微加成，不会改变底层数值曲线。\n可重复研究 3 次。";
                case MetaNodeId.VeinResonance:
                    return "本局修饰：血脉共振。\n你的血脉效果小幅增强，部分怪物开局也会带随机灼烧或湿润。\n风险与收益并存，解锁后作为模组装备。";
                case MetaNodeId.Duration:
                    return "灼烧、湿润等状态持续时间永久 +0.05，上限 0.15。\n不提高伤害倍率，只拉长状态窗口。可重复 3 次。";
                case MetaNodeId.Resist:
                    return "开局腐蚀抗性 +0.08，上限 0.24。\n局内仍可叠到硬上限 70%。适合生命绝地。";
                case MetaNodeId.Pickup:
                    return "全局拾取半径 +0.2，上限 0.6。\n文档示例微加成：方便吸球，不增加输出。可重复 3 次，每次 40 残烬。";
                case MetaNodeId.LifeSteal:
                    return "全局吸血 +0.1，上限 0.3。\n文档示例微加成：每击微量回血，无法靠它碾压高难。可重复 3 次，每次 50 残烬。";
                case MetaNodeId.ModeShort:
                    return "解锁游戏模式：速攻生存。\n胜利条件改为存活 10:00，时间轴压缩。\n适合快速刷残烬。不解锁不影响标准 20 分钟。";
                case MetaNodeId.ModeEndless:
                    return "解锁游戏模式：无尽鏖战。\n没有时间胜利，Boss 循环刷新，记录最长存活。\n需先解锁速攻生存。";
                case MetaNodeId.ModeBossRush:
                    return "解锁游戏模式：Boss 狂袭。\n连续击杀全部 Boss 序列即胜，不刷普通杂兵。";
                case MetaNodeId.CurseArmor:
                    return "解锁基础诅咒：坚铠、迅疾、流血。开战页可选 0-3 个。";
                case MetaNodeId.CurseSwift:
                    return "解锁挑战诅咒：迅疾。\n怪物移速 +15%，本局残烬 ×1.25。";
                case MetaNodeId.CurseBleed:
                    return "解锁挑战诅咒：流血。\n生命回复效果 −40%，本局残烬 ×1.35。";
                case MetaNodeId.UniquePack:
                    return "解锁专武：幽猎螺旋、星陨仪。与开局三件专武互斥，最高 5 级。";
                case MetaNodeId.WeaponPack1:
                    return "解锁普通武器第 5-12 号。栏位仍为 1 专武 + 7 普通。";
                case MetaNodeId.WeaponPack2:
                    return "解锁普通武器第 13-22 号。";
                case MetaNodeId.WeaponPack3:
                    return "解锁普通武器第 23-30 号，合计 30 把。";
                case MetaNodeId.FusionPack:
                    return "解锁其余高级融合配方。两把 5 级普通武器才可融合。";
                case MetaNodeId.ModPack2:
                    return "解锁高级模组：溃烂蔓延、奔袭之道、血肉献祭、固化壁垒。";
                case MetaNodeId.RelicPack:
                    return "Boss 掉落完整 10 件局内遗物池。本局生效，结算销毁。";
                case MetaNodeId.WeaponHaste:
                    return "全局武器冷却倍率 −0.02，上限 −0.06。玩家每次 45 残烬，可重复 3 次。开发者模式视为已满级，不写进客户存档。";
                case MetaNodeId.VeinPack1:
                    return "解锁腐蚀血脉、雷霆血脉。";
                case MetaNodeId.VeinPack2:
                    return "解锁寒冰、瘟疫、圣光、混沌血脉。合计 9 套。";
                case MetaNodeId.VeinLinks:
                    return "解锁全部血脉‑专武联动规则。";
                case MetaNodeId.RareVein:
                    return "升级卡池加入完整 12 张稀有机制被动。";
                case MetaNodeId.VeinMods:
                    return "血脉向模组（溃烂蔓延等）可在开战页选用。";
                case MetaNodeId.VeinDot:
                    return "血脉 DOT 伤害全局 +4%，上限 +16%。玩家每次 40 残烬，可重复 4 次。开发者模式视为已满级，不写进客户存档。";
                case MetaNodeId.HeroPack:
                    return "解锁卡伦（冲刺反击）、莫拉（DOT 溃烂）。";
                case MetaNodeId.EventPack:
                    return "局内事件池按完整时间轴触发。";
                case MetaNodeId.CursePack2:
                    return "解锁诅咒 4-10，合计 10 套。";
                case MetaNodeId.MaxHp:
                    return "全局最大生命 +5，上限 +20。玩家每次 40 残烬，可重复 4 次。开发者模式视为已满级，不写进客户存档。";
                case MetaNodeId.HunterCrit:
                    return "全局暴击率 +1.5%，上限 +6%。玩家每次 40 残烬，可重复 4 次。开发者模式视为已满级，不写进客户存档。";
                case MetaNodeId.ArmorBonus:
                    return "全局护甲 +0.5，上限 +2。玩家每次 40 残烬，可重复 4 次。开发者模式视为已满级，不写进客户存档。";
                default:
                    return "";
            }
        }

        public static string NodePrereq(MetaNodeId id)
        {
            switch (id)
            {
                case MetaNodeId.UniquePack: return "前置：军械 · 普通武器组合进化";
                case MetaNodeId.WeaponPack1: return "前置：军械 · 专武Ⅳ";
                case MetaNodeId.WeaponPack2: return "前置：军械 · 普通武器包Ⅰ";
                case MetaNodeId.WeaponPack3: return "前置：军械 · 普通武器包Ⅱ";
                case MetaNodeId.FusionPack: return "前置：军械 · 普通武器组合进化";
                case MetaNodeId.ModPack2: return "前置：军械 · 模组解锁Ⅰ";
                case MetaNodeId.RelicPack: return "前置：军械 · 模组解锁Ⅱ";
                case MetaNodeId.ModeEndless:
                case MetaNodeId.HeroPack: return "前置：生存 · 速攻生存";
                case MetaNodeId.ModeBossRush:
                case MetaNodeId.EventPack: return "前置：生存 · 无尽鏖战";
                case MetaNodeId.CursePack2: return "前置：生存 · Boss 狂袭";
                case MetaNodeId.VeinPack1: return "前置：血脉 · 血脉基础联动";
                case MetaNodeId.VeinPack2: return "前置：血脉 · 血脉包Ⅰ";
                case MetaNodeId.VeinLinks:
                case MetaNodeId.RareVein:
                case MetaNodeId.VeinMods: return "前置：血脉 · 血脉包Ⅱ";
                default: return "无前置";
            }
        }

        public static string NodeCapLine(MetaNodeId id)
        {
            switch (id)
            {
                case MetaNodeId.Pickup: return $"  ·  {LivePickup:0.0}/0.6";
                case MetaNodeId.LifeSteal: return $"  ·  {LiveLifeSteal:0.0}/0.3";
                case MetaNodeId.Area: return $"  ·  {LiveArea:0.00}/0.09";
                case MetaNodeId.Duration: return $"  ·  {LiveDuration:0.00}/0.15";
                case MetaNodeId.Resist: return $"  ·  {LiveResist:0.00}/0.24";
                case MetaNodeId.WeaponHaste: return $"  ·  {LiveHaste:0.00}/{BalanceTables.Bal.metaCaps.haste:0.00}";
                case MetaNodeId.VeinDot: return $"  ·  {LiveVeinDot:0.00}/{BalanceTables.Bal.metaCaps.veinDot:0.00}";
                case MetaNodeId.MaxHp: return $"  ·  {LiveHp:0}/{BalanceTables.Bal.metaCaps.hp:0}";
                case MetaNodeId.HunterCrit: return $"  ·  {LiveCrit * 100f:0.0}/{BalanceTables.Bal.metaCaps.crit * 100f:0}%";
                case MetaNodeId.ArmorBonus: return $"  ·  {LiveArmor:0.0}/{BalanceTables.Bal.metaCaps.armor:0}";
                default: return "";
            }
        }

        public static void See(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Runner: Data.seenRunner = true; break;
                case EnemyKind.Tank: Data.seenTank = true; break;
                case EnemyKind.Spitter: Data.seenSpitter = true; break;
                case EnemyKind.Exploder: Data.seenExploder = true; break;
                case EnemyKind.Elite: Data.seenElite = true; break;
                case EnemyKind.Boss: Data.seenBoss = true; break;
                default: Data.seenSwarm = true; break;
            }
            Save();
        }

        public static float LivePickup => GameSettings.Developer ? 0.6f : Data.pickupBonus;
        public static float LiveLifeSteal => GameSettings.Developer ? 0.3f : Data.lifeStealBonus;
        public static float LiveArea => GameSettings.Developer ? 0.09f : Data.areaBonus;
        public static float LiveDuration => GameSettings.Developer ? 0.15f : Data.durationBonus;
        public static float LiveResist => GameSettings.Developer ? 0.24f : Data.resistBonus;
        public static float LiveHaste => GameSettings.Developer ? BalanceTables.Bal.metaCaps.haste : Data.hasteBonus;
        public static float LiveVeinDot => GameSettings.Developer ? BalanceTables.Bal.metaCaps.veinDot : Data.veinDotBonus;
        public static float LiveHp => GameSettings.Developer ? BalanceTables.Bal.metaCaps.hp : Data.hpBonus;
        public static float LiveCrit => GameSettings.Developer ? BalanceTables.Bal.metaCaps.crit : Data.critBonus;
        public static float LiveArmor => GameSettings.Developer ? BalanceTables.Bal.metaCaps.armor : Data.armorBonus;

        public static void ApplyBonuses(PlayerCombatStats stats)
        {
            if (stats == null) return;
            stats.PickupRadius += LivePickup;
            stats.LifeSteal += LiveLifeSteal;
            stats.Area += LiveArea;
            stats.Duration += LiveDuration;
            stats.HazardResist = Mathf.Min(0.7f, stats.HazardResist + LiveResist);
            stats.CooldownMul *= Mathf.Max(1f - BalanceTables.Bal.metaCaps.haste, 1f - Mathf.Min(BalanceTables.Bal.metaCaps.haste, LiveHaste));
            stats.MaxHp += Mathf.Min(BalanceTables.Bal.metaCaps.hp, LiveHp);
            stats.CritChance += Mathf.Min(BalanceTables.Bal.metaCaps.crit, LiveCrit);
            stats.Armor += Mathf.Min(BalanceTables.Bal.metaCaps.armor, LiveArmor);
        }

        public static void ClampLobbyMap(ref MapId map, ref int star)
        {
            star = MapCatalog.ClampStar(map, star);
            if (MapUnlocked(map, star)) return;
            map = MapId.VeinWaste;
            star = 1;
        }

        public static void SanitizeRunPicks()
        {
            if (!ModeUnlocked(RunConfig.Mode)) RunConfig.Mode = GameModeId.Standard;
            if (!ModUnlocked(RunConfig.ModA)) RunConfig.ModA = RelicMod.None;
            if (!ModUnlocked(RunConfig.ModB)) RunConfig.ModB = RelicMod.None;
            if (!CurseUnlocked(RunConfig.CurseA)) RunConfig.CurseA = CurseId.None;
            if (!CurseUnlocked(RunConfig.CurseB)) RunConfig.CurseB = CurseId.None;
            if (!CurseUnlocked(RunConfig.CurseC)) RunConfig.CurseC = CurseId.None;
            if (UnityEngine.Object.FindFirstObjectByType<GameSession>() != null) return;
            var map = GameInstaller.CurrentMap;
            var star = RunConfig.MapStar;
            ClampLobbyMap(ref map, ref star);
            GameInstaller.SetMap(map, star);
            if (!HeroUnlocked(GameSettings.LastHero)) GameSettings.LastHero = HeroId.Geralt;
            if (!VeinUnlocked(GameSettings.LastVein)) GameSettings.LastVein = VeinId.Fire;
            if (!UniqueUnlocked(GameSettings.LastUnique)) GameSettings.LastUnique = UniqueId.Hymn;
            ClampLobbyMap(ref GameSettings.LastMap, ref GameSettings.LastStar);
        }

        public static void SanitizeInRunAssets()
        {
            if (GameSettings.Developer) return;
            SanitizeRunPicks();
            var loadout = UnityEngine.Object.FindFirstObjectByType<WeaponLoadout>();
            var levels = UnityEngine.Object.FindFirstObjectByType<LevelDirector>();
            if (loadout != null)
            {
                if (!UniqueUnlocked(GameInstaller.CurrentUnique))
                {
                    GameInstaller.SetUnique(UniqueId.Hymn);
                    UniqueCatalog.Apply(loadout.gameObject);
                }
                var drop = new System.Collections.Generic.List<WeaponId>();
                foreach (var id in loadout.Owned)
                {
                    if (UniqueCatalog.IsUniqueWeapon(id)) continue;
                    if (!WeaponCatalog.Unlocked(id)) drop.Add(id);
                }
                for (var i = 0; i < drop.Count; i++)
                    loadout.Drop(drop[i], levels != null ? levels.Passives : null);
            }

            if (levels != null && !RarePoolUnlocked)
            {
                for (var i = levels.Passives.Count - 1; i >= 0; i--)
                {
                    var title = levels.Passives[i].Title;
                    if (title == "血脉回响" || title == "疾风冲刺" || title == "武器分流" || title == "迅疾血脉"
                        || title == "爆破血脉" || title == "防护汲取" || title == "溃腐传导" || title == "奔袭利刃"
                        || title == "固化石化" || title == "雷霆过载" || title == "牺牲武装" || title == "生命涌动")
                        levels.Passives.RemoveAt(i);
                }
                RunConfig.DualVein = RunConfig.DashPulse = RunConfig.WeaponSplit = false;
            }

            if (!RelicPackUnlocked)
            {
                RunConfig.RelicName = "";
                RunConfig.RelicUntil = 0f;
            }
        }
    }

    public static class RunConfig
    {
        public static bool DeveloperRun;
        public static int MapStar = 1;
        public static int RerollsLeftAtEnd;
        public static GameModeId Mode = GameModeId.Standard;
        public static RelicMod ModA;
        public static RelicMod ModB;
        public static CurseId CurseA;
        public static CurseId CurseB;
        public static CurseId CurseC;
        public static bool DualVein;
        public static bool DashPulse;
        public static bool WeaponSplit;
        public static bool HasRevive;
        public static bool UsedRevive;
        public static bool FusedOrbitNova;
        public static bool FusedFrostLash;
        public static readonly List<string> FusedIds = new List<string>();

        public static int FusionSlots => FusedIds.Count;

        public static bool HasFusion(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            for (var i = 0; i < FusedIds.Count; i++)
                if (FusedIds[i] == id) return true;
            return false;
        }

        public static void MarkFusion(string id)
        {
            if (string.IsNullOrEmpty(id) || HasFusion(id)) return;
            FusedIds.Add(id);
            if (id == "orbit_nova") FusedOrbitNova = true;
            if (id == "frost_lash") FusedFrostLash = true;
        }

        public static void UnmarkFusion(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            for (var i = FusedIds.Count - 1; i >= 0; i--)
                if (FusedIds[i] == id) FusedIds.RemoveAt(i);
            if (id == "orbit_nova") FusedOrbitNova = false;
            if (id == "frost_lash") FusedFrostLash = false;
        }
        public static string RelicName = "";
        public static float RelicUntil;
        public static float EventUntil;
        public static string EventName = "";
        public static float DamageDealt;
        public static float DamageTaken;
        public static int BossKills;
        public static int LastEmbers;
        public static float PlayerSlowUntil;
        public const int MaxReplaces = 8;
        public static int ReplacesLeft = MaxReplaces;

        public static bool RelicOn(string name) =>
            RelicUntil > Time.time && RelicName == name;

        public static string BuildTag()
        {
            var h = GameInstaller.CurrentHero;
            var v = GameInstaller.CurrentVein;
            var u = GameInstaller.CurrentUnique;
            if (h == HeroId.Geralt && u == UniqueId.Hymn && v == VeinId.Fire) return "弹幕灼烧流";
            if (h == HeroId.Karen && u == UniqueId.Blade && v == VeinId.Thunder) return "雷霆连锁冲刺流";
            if (h == HeroId.Liz && u == UniqueId.Bolt && v == VeinId.Earth) return "暴击石化爆发流";
            if (h == HeroId.Mora && u == UniqueId.Hymn && v == VeinId.Corrode) return "腐蚀溃烂传染流";
            if (h == HeroId.Geralt && u == UniqueId.Spiral && v == VeinId.Ice) return "寒冰冻结控制流";
            if (h == HeroId.Mora && u == UniqueId.Bolt && v == VeinId.Plague) return "瘟疫持续传染流";
            if (h == HeroId.Antalo && u == UniqueId.Star && v == VeinId.Holy) return "圣光护盾苟活流";
            if (h == HeroId.Liz && u == UniqueId.Spiral && v == VeinId.Chaos) return "混沌畸变摇摆流";
            if (h == HeroId.Antalo && u == UniqueId.Blade && v == VeinId.Water) return "近战残血搏命流";
            if (h == HeroId.Geralt && u == UniqueId.Star && v == VeinId.Thunder) return "多武器分流混合流";
            return "自定义构筑";
        }

        public static void ResetRun()
        {
            DualVein = DashPulse = WeaponSplit = false;
            HasRevive = UsedRevive = false;
            FusedOrbitNova = FusedFrostLash = false;
            FusedIds.Clear();
            RelicName = "";
            RelicUntil = 0f;
            EventUntil = 0f;
            EventName = "";
            DamageDealt = 0f;
            DamageTaken = 0f;
            BossKills = 0;
            LastEmbers = 0;
            PlayerSlowUntil = 0f;
            ReplacesLeft = MaxReplaces;
            DeveloperRun = false;
            MapStar = 1;
            RerollsLeftAtEnd = BalanceTables.Rerolls;
        }

        public static void ClearModifiers()
        {
            Mode = GameModeId.Standard;
            ModA = ModB = RelicMod.None;
            CurseA = CurseB = CurseC = CurseId.None;
        }

        public static bool HasMod(RelicMod id)
        {
            if (id == RelicMod.None) return false;
            return ModA == id || ModB == id;
        }

        public static bool HasCurse(CurseId id)
        {
            if (id == CurseId.None) return false;
            return CurseA == id || CurseB == id || CurseC == id;
        }

        public static float EmberMul
        {
            get
            {
                var m = 1f;
                if (HasCurse(CurseId.Armor)) m *= 1.2f;
                if (HasCurse(CurseId.Swift)) m *= 1.25f;
                if (HasCurse(CurseId.Bleed)) m *= 1.35f;
                if (HasCurse(CurseId.Miasma)) m *= 1.2f;
                if (HasCurse(CurseId.EliteFury)) m *= 1.3f;
                if (HasCurse(CurseId.SparseLoot)) m *= 1.22f;
                if (HasCurse(CurseId.DeathEcho)) m *= 1.38f;
                if (HasCurse(CurseId.FrostCalamity)) m *= 1.28f;
                if (HasCurse(CurseId.PlagueSpread)) m *= 1.32f;
                if (HasCurse(CurseId.ChaosMutate)) m *= 1.4f;
                return Mathf.Min(2.2f, m);
            }
        }

        public static float EnemyArmorAdd => HasCurse(CurseId.Armor) ? 5f : 0f;
        public static float EnemySpeedMul => HasCurse(CurseId.Swift) ? 1.15f : 1f;
        public static float RegenMul => HasCurse(CurseId.Bleed) ? 0.6f : 1f;
        public static float PlayerMoveMul => HasMod(RelicMod.GaleField) ? 1.15f : 1f;
        public static float MonsterMoveMul => HasMod(RelicMod.GaleField) ? 1.1f : 1f;

        public static string ModeTitle(GameModeId id)
        {
            switch (id)
            {
                case GameModeId.Short: return "速攻生存";
                case GameModeId.Endless: return "无尽鏖战";
                case GameModeId.BossRush: return "Boss狂袭";
                default: return "标准生存";
            }
        }

        public static string ModeBlurb(GameModeId id)
        {
            switch (id)
            {
                case GameModeId.Short: return "存活 10:00。适合快速刷残烬。";
                case GameModeId.Endless: return "无时间胜利。尽量活更久，Boss 循环。";
                case GameModeId.BossRush: return "连续击杀全部 Boss 序列即胜。";
                default: return "存活 20:00。默认标准生存。";
            }
        }

        public static string ModTitle(RelicMod id)
        {
            switch (id)
            {
                case RelicMod.VeinResonance: return "血脉共振";
                case RelicMod.GaleField: return "疾风战场";
                case RelicMod.RotSpread: return "溃烂蔓延";
                case RelicMod.DashWay: return "奔袭之道";
                case RelicMod.FleshOffering: return "血肉献祭";
                case RelicMod.ShieldWall: return "固化壁垒";
                case RelicMod.WeaponTrial: return "武器试炼";
                default: return "无";
            }
        }

        public static string ModBlurb(RelicMod id)
        {
            switch (id)
            {
                case RelicMod.VeinResonance: return "血脉效果增强，部分敌人也会带随机血脉。";
                case RelicMod.GaleField: return "你移速 +15%，怪物移速 +10%。";
                case RelicMod.RotSpread: return "腐蚀 DOT 传染距离大幅提升，玩家护甲 −4。";
                case RelicMod.DashWay: return "冲刺伤害 +45%，冲刺冷却 +20%。";
                case RelicMod.FleshOffering: return "击杀短时提高伤害，但会损失少量生命。";
                case RelicMod.ShieldWall: return "护盾效果 +35%，全部输出 −15%。";
                case RelicMod.WeaponTrial: return "专武不再出现强化卡，普通武器更易抽出。";
                default: return "不启用模组。";
            }
        }

        public static string CurseTitle(CurseId id)
        {
            switch (id)
            {
                case CurseId.Miasma: return "腐蚀瘴气";
                case CurseId.EliteFury: return "狂暴精英";
                case CurseId.SparseLoot: return "贫瘠拾取";
                case CurseId.DeathEcho: return "死亡回响";
                case CurseId.FrostCalamity: return "冰封灾厄";
                case CurseId.PlagueSpread: return "瘟疫侵染";
                case CurseId.ChaosMutate: return "混沌畸变灾变";
                case CurseId.Armor: return "坚铠诅咒";
                case CurseId.Swift: return "迅疾诅咒";
                case CurseId.Bleed: return "流血诅咒";
                default: return "无";
            }
        }

        public static string CurseBlurb(CurseId id)
        {
            switch (id)
            {
                case CurseId.Miasma: return "地图周期性生成腐蚀地面。残烬 ×1.2";
                case CurseId.EliteFury: return "精英伤害 +20%。残烬 ×1.3";
                case CurseId.SparseLoot: return "掉落减少，怪物血量略降。残烬 ×1.22";
                case CurseId.DeathEcho: return "怪物死亡产生小型爆炸。残烬 ×1.38";
                case CurseId.FrostCalamity: return "敌人攻击可能冻结你。残烬 ×1.28";
                case CurseId.PlagueSpread: return "怪物死亡可能释放瘟疫区。残烬 ×1.32";
                case CurseId.ChaosMutate: return "敌人随机获得血脉 buff。残烬 ×1.40";
                case CurseId.Armor: return "全体怪物护甲 +5。残烬 ×1.2";
                case CurseId.Swift: return "怪物移速 +15%。残烬 ×1.25";
                case CurseId.Bleed: return "生命回复效果 −40%。残烬 ×1.35";
                default: return "不启用诅咒。";
            }
        }

        public static string ModsLine()
        {
            var parts = new List<string>();
            if (ModA != RelicMod.None) parts.Add(ModTitle(ModA));
            if (ModB != RelicMod.None && ModB != ModA) parts.Add(ModTitle(ModB));
            if (CurseA != CurseId.None) parts.Add(CurseTitle(CurseA));
            if (CurseB != CurseId.None && CurseB != CurseA) parts.Add(CurseTitle(CurseB));
            if (CurseC != CurseId.None && CurseC != CurseA && CurseC != CurseB) parts.Add(CurseTitle(CurseC));
            return parts.Count == 0 ? "无模组 / 无诅咒" : string.Join("  ·  ", parts);
        }
    }
}
