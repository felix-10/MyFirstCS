using System;
using System.Collections.Generic;
using UnityEngine;

namespace Veinfire
{
    [Serializable]
    public sealed class RunSnapshot
    {
        public int version = 3;
        public int map;
        public int mapStar = 1;
        public int hero;
        public int vein;
        public int unique;
        public float time;
        public int kills;
        public float x;
        public float z;
        public float hp;
        public int level = 1;
        public int xp;
        public int xpToNext;
        public int pending;
        public float moveSpeed = 5.5f;
        public float maxHp = 130f;
        public float damage = 16f;
        public float fireInterval = 0.28f;
        public int projectileCount = 1;
        public float projectileSpeed = 16f;
        public float projectileSize = 0.24f;
        public float pickupRadius = 2.2f;
        public float lifeSteal;
        public float healthRegen;
        public float armor;
        public float might = 1f;
        public float cooldownMul = 1f;
        public float area = 1f;
        public int pierce;
        public float knockback = 2.2f;
        public float duration = 1f;
        public float critChance;
        public float critDamage = 2f;
        public float dashCooldown = 2f;
        public int needle = 1;
        public int orbit;
        public int nova;
        public int frost;
        public int lash;
        public int divine;
        public int blade;
        public bool boss2;
        public bool boss5;
        public bool boss8;
        public bool boss12;
        public bool boss17;
        public int mode;
        public int modA;
        public int modB;
        public int curseA;
        public int curseB;
        public int curseC;
        public bool dualVein;
        public bool dashPulse;
        public bool weaponSplit;
        public bool hasRevive;
        public bool usedRevive;
        public bool fusedOrbit;
        public bool fusedFrost;
        public string fusedKeys = "";
        public int bossKills;
        public int uniqueBranch;
        public int uniqueEvo;
        public int rerolls = 3;
        public int replacesLeft = 8;
        public string[] titles = Array.Empty<string>();
        public string[] marks = Array.Empty<string>();
        public int[] stacks = Array.Empty<int>();
        public float[] cr = Array.Empty<float>();
        public float[] cg = Array.Empty<float>();
        public float[] cb = Array.Empty<float>();
    }

    public static class RunSave
    {
        const string Key = "vf_run_json";
        static string FilePath => System.IO.Path.Combine(Application.persistentDataPath, "veinfire_run.json");

        public static bool Exists
        {
            get
            {
                try { return System.IO.File.Exists(FilePath) || !string.IsNullOrEmpty(PlayerPrefs.GetString(Key, "")); }
                catch { return false; }
            }
        }

        public static RunSnapshot Load()
        {
            try
            {
                if (System.IO.File.Exists(FilePath))
                {
                    var data = JsonUtility.FromJson<RunSnapshot>(System.IO.File.ReadAllText(FilePath));
                    if (data != null && data.version >= 1) return data;
                    System.IO.File.Delete(FilePath);
                    return null;
                }
            }
            catch
            {
                try { if (System.IO.File.Exists(FilePath)) System.IO.File.Delete(FilePath); } catch { }
                return null;
            }

            var json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json)) return null;
            var legacy = JsonUtility.FromJson<RunSnapshot>(json);
            return legacy != null && legacy.version >= 1 ? legacy : null;
        }

        public static void Clear()
        {
            try { if (System.IO.File.Exists(FilePath)) System.IO.File.Delete(FilePath); } catch { }
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }

        public static void Capture()
        {
            if (GameSession.IsGameOver || GameSession.IsWon) return;
            var session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();
            var levels = UnityEngine.Object.FindFirstObjectByType<LevelDirector>();
            if (session == null || player == null || levels == null) return;
            var stats = player.GetComponent<PlayerCombatStats>();
            var health = player.GetComponent<PlayerHealth>();
            var loadout = player.GetComponent<WeaponLoadout>();
            var dash = player.GetComponent<PlayerDash>();
            var spawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
            if (stats == null || health == null || health.IsDead) return;

            var data = new RunSnapshot
            {
                version = 3,
                map = (int)GameInstaller.CurrentMap,
                mapStar = RunConfig.MapStar,
                hero = (int)GameInstaller.CurrentHero,
                vein = (int)GameInstaller.CurrentVein,
                unique = (int)GameInstaller.CurrentUnique,
                time = session.Elapsed,
                kills = GameSession.Kills,
                x = player.transform.position.x,
                z = player.transform.position.z,
                hp = health.Current,
                level = levels.Level,
                xp = levels.Xp,
                xpToNext = levels.XpToNext,
                pending = levels.PendingOffers,
                moveSpeed = stats.MoveSpeed,
                maxHp = stats.MaxHp,
                damage = stats.Damage,
                fireInterval = stats.FireInterval,
                projectileCount = stats.ProjectileCount,
                projectileSpeed = stats.ProjectileSpeed,
                projectileSize = stats.ProjectileSize,
                pickupRadius = stats.PickupRadius,
                lifeSteal = stats.LifeSteal,
                healthRegen = stats.HealthRegen,
                armor = stats.Armor,
                might = stats.Might,
                cooldownMul = stats.CooldownMul,
                area = stats.Area,
                pierce = stats.Pierce,
                knockback = stats.Knockback,
                duration = stats.Duration,
                critChance = stats.CritChance,
                critDamage = stats.CritDamage,
                dashCooldown = dash != null ? dash.Cooldown : 2f,
                needle = loadout != null && loadout.Owns(WeaponId.Needle) ? loadout.LevelOf(WeaponId.Needle) : 0,
                orbit = loadout != null ? loadout.LevelOf(WeaponId.Orbit) : 0,
                nova = loadout != null ? loadout.LevelOf(WeaponId.Nova) : 0,
                frost = loadout != null ? loadout.LevelOf(WeaponId.Frost) : 0,
                lash = loadout != null ? loadout.LevelOf(WeaponId.Lash) : 0,
                divine = loadout != null && loadout.Owns(WeaponId.Divine) ? loadout.LevelOf(WeaponId.Divine) : 0,
                blade = loadout != null && loadout.Owns(WeaponId.Blade) ? loadout.LevelOf(WeaponId.Blade) : 0,
                mode = (int)RunConfig.Mode,
                modA = (int)RunConfig.ModA,
                modB = (int)RunConfig.ModB,
                curseA = (int)RunConfig.CurseA,
                curseB = (int)RunConfig.CurseB,
                curseC = (int)RunConfig.CurseC,
                dualVein = RunConfig.DualVein,
                dashPulse = RunConfig.DashPulse,
                weaponSplit = RunConfig.WeaponSplit,
                hasRevive = RunConfig.HasRevive,
                usedRevive = RunConfig.UsedRevive,
                fusedOrbit = RunConfig.FusedOrbitNova,
                fusedFrost = RunConfig.FusedFrostLash,
                fusedKeys = string.Join(",", RunConfig.FusedIds),
                bossKills = RunConfig.BossKills,
                uniqueBranch = player.GetComponent<UniqueBuild>() != null ? player.GetComponent<UniqueBuild>().Branch : 0,
                uniqueEvo = player.GetComponent<UniqueBuild>() != null ? player.GetComponent<UniqueBuild>().Evo : 0,
                rerolls = levels.Rerolls,
                replacesLeft = RunConfig.ReplacesLeft
            };

            if (spawner != null) spawner.WriteSave(data);

            var passives = levels.Passives;
            data.titles = new string[passives.Count];
            data.marks = new string[passives.Count];
            data.stacks = new int[passives.Count];
            data.cr = new float[passives.Count];
            data.cg = new float[passives.Count];
            data.cb = new float[passives.Count];
            for (var i = 0; i < passives.Count; i++)
            {
                data.titles[i] = passives[i].Title;
                data.marks[i] = passives[i].Mark;
                data.stacks[i] = passives[i].Stacks;
                data.cr[i] = passives[i].Color.r;
                data.cg[i] = passives[i].Color.g;
                data.cb[i] = passives[i].Color.b;
            }

            try
            {
                System.IO.File.WriteAllText(FilePath, JsonUtility.ToJson(data));
            }
            catch
            {
                PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            }
            GameSettings.LastMap = (MapId)data.map;
            GameSettings.LastHero = (HeroId)Mathf.Clamp(data.hero, 0, 4);
            GameSettings.LastVein = (VeinId)Mathf.Clamp(data.vein, 0, 8);
            GameSettings.LastUnique = (UniqueId)Mathf.Clamp(data.unique, 0, 4);
            GameSettings.Save();
            PlayerPrefs.Save();
        }

        public static void ApplyConfig(RunSnapshot data)
        {
            if (data == null) return;
            RunConfig.Mode = (GameModeId)Mathf.Clamp(data.mode, 0, 3);
            RunConfig.ModA = (RelicMod)Mathf.Clamp(data.modA, 0, 7);
            RunConfig.ModB = (RelicMod)Mathf.Clamp(data.modB, 0, 7);
            RunConfig.CurseA = (CurseId)Mathf.Clamp(data.curseA, 0, 10);
            RunConfig.CurseB = (CurseId)Mathf.Clamp(data.curseB, 0, 10);
            RunConfig.CurseC = (CurseId)Mathf.Clamp(data.curseC, 0, 10);
            RunConfig.DualVein = data.dualVein;
            RunConfig.DashPulse = data.dashPulse;
            RunConfig.WeaponSplit = data.weaponSplit;
            RunConfig.HasRevive = data.hasRevive;
            RunConfig.UsedRevive = data.usedRevive;
            RunConfig.FusedIds.Clear();
            RunConfig.FusedOrbitNova = false;
            RunConfig.FusedFrostLash = false;
            if (!string.IsNullOrEmpty(data.fusedKeys))
            {
                var parts = data.fusedKeys.Split(',');
                for (var i = 0; i < parts.Length; i++)
                    RunConfig.MarkFusion(parts[i].Trim());
            }
            if (data.fusedOrbit) RunConfig.MarkFusion("orbit_nova");
            if (data.fusedFrost) RunConfig.MarkFusion("frost_lash");
            RunConfig.BossKills = data.bossKills;
            RunConfig.MapStar = data.version < 3
                ? (data.map == 0 ? 2 : 2)
                : Mathf.Clamp(data.mapStar, 1, 5);
            RunConfig.MapStar = MapCatalog.ClampStar((MapId)Mathf.Clamp(data.map, 0, 4), RunConfig.MapStar);
            RunConfig.ReplacesLeft = data.version < 2
                ? RunConfig.MaxReplaces
                : Mathf.Clamp(data.replacesLeft, 0, RunConfig.MaxReplaces);
        }

        public static void Apply(RunSnapshot data)
        {
            if (data == null) return;
            ApplyConfig(data);
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();
            var session = UnityEngine.Object.FindFirstObjectByType<GameSession>();
            var levels = UnityEngine.Object.FindFirstObjectByType<LevelDirector>();
            var spawner = UnityEngine.Object.FindFirstObjectByType<EnemySpawner>();
            if (player == null || session == null || levels == null) return;

            var stats = player.GetComponent<PlayerCombatStats>();
            var health = player.GetComponent<PlayerHealth>();
            var loadout = player.GetComponent<WeaponLoadout>();
            var dash = player.GetComponent<PlayerDash>();

            stats.MoveSpeed = data.moveSpeed;
            stats.MaxHp = data.maxHp;
            stats.Damage = data.damage;
            stats.FireInterval = data.fireInterval;
            stats.ProjectileCount = Mathf.Max(1, data.projectileCount);
            stats.ProjectileSpeed = data.projectileSpeed;
            stats.ProjectileSize = data.projectileSize;
            stats.PickupRadius = data.pickupRadius;
            stats.LifeSteal = data.lifeSteal;
            stats.HealthRegen = Mathf.Max(0f, data.healthRegen);
            stats.Armor = data.armor;
            stats.Might = data.might;
            stats.CooldownMul = data.cooldownMul;
            stats.Area = data.area;
            stats.Pierce = data.pierce;
            stats.Knockback = data.knockback;
            stats.Duration = data.duration;
            stats.CritChance = Mathf.Clamp01(data.critChance);
            stats.CritDamage = data.critDamage > 0.01f ? data.critDamage : 2f;
            if (dash != null) dash.Cooldown = data.dashCooldown;

            loadout?.RestoreLevels(data.needle, data.orbit, data.nova, data.frost, data.lash, data.divine, data.blade);
            var build = UniqueBuild.Of(player);
            if (build != null)
            {
                build.Branch = data.uniqueBranch;
                build.Evo = data.uniqueEvo;
            }
            health.Restore(data.hp, data.maxHp);
            player.transform.position = new Vector3(data.x, player.transform.position.y, data.z);
            session.RestoreClock(data.time, data.kills);
            spawner?.RestoreTime(data.time, data.boss2, data.boss5, data.boss8, data.boss12, data.boss17);

            var passives = new List<TraitRecord>();
            var n = data.titles != null ? data.titles.Length : 0;
            for (var i = 0; i < n; i++)
            {
                passives.Add(new TraitRecord
                {
                    Title = data.titles[i],
                    Mark = i < data.marks.Length ? data.marks[i] : "◆",
                    Stacks = i < data.stacks.Length ? data.stacks[i] : 1,
                    Color = new Color(
                        i < data.cr.Length ? data.cr[i] : 0.5f,
                        i < data.cg.Length ? data.cg[i] : 0.5f,
                        i < data.cb.Length ? data.cb[i] : 0.8f),
                    IsWeapon = false
                });
            }

            levels.Restore(data.level, data.xp, data.xpToNext, data.pending, passives, data.rerolls);
        }

        public static string Summary()
        {
            var data = Load();
            if (data == null) return "";
            var m = Mathf.FloorToInt(data.time / 60f);
            var s = Mathf.FloorToInt(data.time % 60f);
            return $"{HeroCatalog.Title((HeroId)data.hero)}  {VeinCatalog.Title((VeinId)data.vein)}  {MapCatalog.Title((MapId)data.map)}  等级 {data.level}  {m:00}:{s:00}";
        }
    }
}
