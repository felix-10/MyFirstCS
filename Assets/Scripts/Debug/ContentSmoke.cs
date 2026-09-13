using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace Veinfire
{
    public sealed class ContentSmoke : MonoBehaviour
    {
        readonly StringBuilder _log = new StringBuilder();
        int _fail;
        int _pass;
        bool _loggedError;

        public static string ReportPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "SmokeReport.txt"));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorPrefs.GetBool("Veinfire.Smoke", false)) return;
            UnityEditor.EditorPrefs.SetBool("Veinfire.Smoke", false);
#else
            if (!File.Exists(Path.Combine(Application.dataPath, "..", ".smoke-once"))) return;
#endif
            if (FindFirstObjectByType<ContentSmoke>() != null) return;
            var go = new GameObject("ContentSmoke");
            DontDestroyOnLoad(go);
            go.AddComponent<ContentSmoke>();
        }

        void OnEnable() => Application.logMessageReceived += OnLog;
        void OnDisable() => Application.logMessageReceived -= OnLog;

        void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            _loggedError = true;
            _log.AppendLine("  LOG " + type + ": " + condition);
        }

        IEnumerator Start()
        {
            GameSettings.Load();
            GameSettings.Developer = true;
            GameSettings.Save();
            RunConfig.ClearModifiers();
            RunConfig.Mode = GameModeId.Standard;
            MainMenu.Hide();
            GameSession.SetMenu(false);

            AuditStatic();

            for (var m = 0; m < 3; m++)
            {
                var map = (MapId)m;
                Func<string> check = CheckMapCity;
                if (map == MapId.BoneCloister) check = CheckMapDeadland;
                if (map == MapId.AshMarsh) check = CheckMapPlain;
                yield return Probe("地图 " + MapCatalog.Title(map), map, HeroId.Geralt, VeinId.Fire, UniqueId.Hymn, check, 1.2f);
            }

            for (var h = 0; h < HeroCatalog.Count; h++)
            {
                var hero = (HeroId)h;
                yield return Probe("角色 " + HeroCatalog.Title(hero), MapId.VeinWaste, hero, VeinId.Fire, UniqueId.Hymn,
                    () => CheckHero(hero), 0.4f, false);
            }

            for (var h = 0; h < HeroCatalog.Count; h++)
            for (var u = 0; u < UniqueCatalog.Count; u++)
            {
                var hero = (HeroId)h;
                var unique = (UniqueId)u;
                var wait = unique == UniqueId.Bolt || unique == UniqueId.Star ? 2.2f : 1.6f;
                yield return Probe(
                    HeroCatalog.Title(hero) + " / " + UniqueCatalog.Title(unique),
                    MapId.VeinWaste, hero, VeinId.Fire, unique,
                    () => CheckUnique(unique), wait);
            }

            for (var h = 0; h < HeroCatalog.Count; h++)
            for (var v = 0; v < VeinCatalog.Count; v++)
            {
                var hero = (HeroId)h;
                var vein = (VeinId)v;
                yield return Probe(
                    HeroCatalog.Title(hero) + " / " + VeinCatalog.Title(vein),
                    MapId.VeinWaste, hero, vein, UniqueId.Hymn,
                    () => CheckVein(vein), 1.6f);
            }

            for (var i = 0; i < 4; i++)
            {
                var mode = (GameModeId)i;
                RunConfig.ClearModifiers();
                RunConfig.Mode = mode;
                yield return Probe("模式 " + RunConfig.ModeTitle(mode), MapId.VeinWaste, HeroId.Geralt, VeinId.Fire, UniqueId.Hymn,
                    () => CheckMode(mode), 1.4f);
            }

            for (var i = 1; i <= 7; i++)
            {
                var mod = (RelicMod)i;
                RunConfig.ClearModifiers();
                RunConfig.Mode = GameModeId.Standard;
                RunConfig.ModA = mod;
                yield return Probe("模组 " + RunConfig.ModTitle(mod), MapId.VeinWaste, HeroId.Geralt, VeinId.Fire, UniqueId.Hymn,
                    () => CheckMod(mod), 1.2f);
            }

            for (var i = 1; i <= 10; i++)
            {
                var curse = (CurseId)i;
                RunConfig.ClearModifiers();
                RunConfig.Mode = GameModeId.Standard;
                RunConfig.CurseA = curse;
                yield return Probe("诅咒 " + RunConfig.CurseTitle(curse), MapId.VeinWaste, HeroId.Geralt, VeinId.Fire, UniqueId.Hymn,
                    () => CheckCurse(curse), 1.2f);
            }

            RunConfig.ClearModifiers();
            RunConfig.Mode = GameModeId.Standard;
            yield return Probe("解锁近战武器", MapId.VeinWaste, HeroId.Geralt, VeinId.Fire, UniqueId.Blade,
                () => CheckUnlockFamily(WeaponCatalog.WeaponFamily.Melee), 1.2f);
            yield return Probe("解锁远程武器", MapId.VeinWaste, HeroId.Geralt, VeinId.Fire, UniqueId.Hymn,
                () => CheckUnlockFamily(WeaponCatalog.WeaponFamily.Ranged), 1.2f);
            yield return Probe("解锁持续武器", MapId.VeinWaste, HeroId.Geralt, VeinId.Plague, UniqueId.Hymn,
                () => CheckUnlockFamily(WeaponCatalog.WeaponFamily.Dot), 1.2f);

            WriteReport();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        void AuditStatic()
        {
            Expect("角色目录=5", HeroCatalog.Count == 5);
            Expect("血脉目录=9", VeinCatalog.Count == 9);
            Expect("专武目录=5", UniqueCatalog.Count == 5);
            Expect("普通武器=30", WeaponCatalog.Commons.Length == 30);
            Expect("地图枚举=5", Enum.GetNames(typeof(MapId)).Length == 5);
            Expect("模式=4", Enum.GetNames(typeof(GameModeId)).Length == 4);
            Expect("模组含高级4件", RelicMod.ShieldWall == (RelicMod)7);
            Expect("诅咒含10套", CurseId.ChaosMutate == (CurseId)10);
        }

        IEnumerator Probe(string name, MapId map, HeroId hero, VeinId vein, UniqueId unique, Func<string> check,
            float wait, bool dummy = true)
        {
            _loggedError = false;
            try
            {
                GameInstaller.StartRun(map, hero, vein, unique);
            }
            catch (Exception e)
            {
                Fail(name, "StartRun 异常 " + e.Message);
                yield break;
            }

            yield return null;
            yield return null;
            if (dummy) PlaceDummy();
            if (wait > 0f) yield return new WaitForSeconds(wait);

            string result;
            try
            {
                result = check != null ? check() : "";
            }
            catch (Exception e)
            {
                Fail(name, "检查异常 " + e.Message);
                yield break;
            }

            if (_loggedError)
            {
                Fail(name, "运行中有 Error/Exception");
                yield break;
            }

            if (!string.IsNullOrEmpty(result)) Fail(name, result);
            else Pass(name);
        }

        void PlaceDummy()
        {
            var player = FindFirstObjectByType<PlayerMotor>();
            if (player == null || FindFirstObjectByType<EnemySpawner>() == null) return;
            var at = player.transform.position + player.transform.forward * 4.5f;
            at.y = player.transform.position.y;
            EnemySpawner.SpawnSupport(EnemyKind.Swarm, at, 3);
            EnemySpawner.SpawnSupport(EnemyKind.Tank, at, 1);
        }

        string CheckCore()
        {
            if (FindFirstObjectByType<PlayerMotor>() == null) return "没有玩家";
            if (FindFirstObjectByType<EnemySpawner>() == null) return "没有刷怪器";
            if (FindFirstObjectByType<LevelDirector>() == null) return "没有升级导演";
            if (GameObject.Find(MapCatalog.RootName) == null) return "没有地图根节点";
            return "";
        }

        string CheckMapCity()
        {
            var miss = CheckCore();
            if (miss != "") return miss;
            if (GameObject.Find("MapSolid") == null) return "末日城市没有固体障碍";
            if (BalanceTables.Spec(MapId.VeinWaste, 2).hp < 1.3f) return "末日城市怪血倍率不对";
            return "";
        }

        string CheckMapDeadland()
        {
            var miss = CheckCore();
            if (miss != "") return miss;
            if (FindFirstObjectByType<MapHazard>() == null) return "生命绝地没有腐蚀垫";
            return "";
        }

        string CheckMapPlain()
        {
            var miss = CheckCore();
            if (miss != "") return miss;
            if (BalanceTables.Spec(MapId.AshMarsh, 2).damage < 1.3f) return "神之平原伤害倍率不对";
            return "";
        }

        string CheckHero(HeroId id)
        {
            var miss = CheckCore();
            if (miss != "") return miss;
            var stats = FindFirstObjectByType<PlayerCombatStats>();
            var passives = FindFirstObjectByType<HeroPassives>();
            if (stats == null || passives == null) return "没有角色属性或被动组件";
            if (GameInstaller.CurrentHero != id) return "当前角色不是 " + HeroCatalog.Title(id);
            switch (id)
            {
                case HeroId.Liz:
                    if (stats.CritChance < 0.14f) return "利兹暴击未生效";
                    break;
                case HeroId.Antalo:
                    if (stats.Armor < 4.9f || stats.MaxHp < 160f) return "安塔洛生存属性不对";
                    break;
                case HeroId.Karen:
                    if (stats.MoveSpeed < 5.9f) return "卡伦移速不对";
                    break;
                case HeroId.Mora:
                    if (stats.Damage > 12.5f) return "莫拉直伤减免未生效";
                    break;
                default:
                    if (stats.MaxHp < 129f) return "杰罗特生命不对";
                    break;
            }

            return "";
        }

        string CheckUnique(UniqueId id)
        {
            var miss = CheckCore();
            if (miss != "") return miss;
            var player = FindFirstObjectByType<PlayerMotor>();
            if (player == null) return "没有玩家";
            var go = player.gameObject;
            switch (id)
            {
                case UniqueId.Bolt:
                    if (go.GetComponent<LightningWeapon>() == null || !go.GetComponent<LightningWeapon>().enabled)
                        return "神罚组件未启用";
                    break;
                case UniqueId.Blade:
                    if (go.GetComponent<TemorisSword>() == null || !go.GetComponent<TemorisSword>().enabled)
                        return "长剑组件未启用";
                    break;
                case UniqueId.Spiral:
                    if (go.GetComponent<SpiralHuntWeapon>() == null || !go.GetComponent<SpiralHuntWeapon>().enabled)
                        return "螺旋组件未启用";
                    break;
                case UniqueId.Star:
                    if (go.GetComponent<StarfallInstrument>() == null || !go.GetComponent<StarfallInstrument>().enabled)
                        return "星陨组件未启用";
                    break;
                default:
                    var hymn = go.GetComponent<AutoAimWeapon>();
                    if (hymn == null || !hymn.enabled || hymn.ProjectilePrefab == null)
                        return "地狱绝唱未启用或没有弹体";
                    break;
            }

            if (FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length == 0)
                return "没有测伤用敌人";
            if (RunConfig.DamageDealt <= 0.01f)
                return "等待窗口内输出为 0（打不到敌人）";
            return "";
        }

        string CheckVein(VeinId vein)
        {
            var miss = CheckUnique(GameInstaller.CurrentUnique);
            if (miss != "") return miss;
            var dummy = LivingEnemy();
            if (dummy == null)
            {
                PlaceDummy();
                dummy = LivingEnemy();
            }

            if (dummy == null) return "没有测血脉用敌人";
            VeinCatalog.OnPlayerHit(dummy, 24f, dummy.transform.position);
            if (vein == VeinId.Water && dummy.Current > 0f && !dummy.IsWet)
                return "水之血脉没有附上湿润";
            return "";
        }

        string CheckMode(GameModeId mode)
        {
            var miss = CheckUnique(UniqueId.Hymn);
            if (miss != "") return miss;
            if (RunConfig.Mode != mode) return "模式没有套上";
            if (mode == GameModeId.BossRush)
            {
                var trash = 0;
                var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
                for (var i = 0; i < enemies.Length; i++)
                    if (enemies[i] != null && enemies[i].Kind != EnemyKind.Swarm && enemies[i].Kind != EnemyKind.Tank)
                        trash++;
            }

            return "";
        }

        string CheckMod(RelicMod mod)
        {
            var miss = CheckCore();
            if (miss != "") return miss;
            if (!RunConfig.HasMod(mod)) return "模组没有套上";
            return "";
        }

        string CheckCurse(CurseId curse)
        {
            var miss = CheckCore();
            if (miss != "") return miss;
            if (!RunConfig.HasCurse(curse)) return "诅咒没有套上";
            if (RunConfig.EmberMul < 1.19f) return "诅咒残烬倍率没有上去";
            return "";
        }

        string CheckUnlockFamily(WeaponCatalog.WeaponFamily family)
        {
            var miss = CheckCore();
            if (miss != "") return miss;
            var loadout = FindFirstObjectByType<WeaponLoadout>();
            if (loadout == null) return "没有武器栏";
            var added = 0;
            foreach (var id in WeaponCatalog.Commons)
            {
                if (WeaponCatalog.FamilyOf(id) != family) continue;
                if (loadout.Owns(id)) continue;
                if (!loadout.CanUnlock) break;
                if (!loadout.Unlock(id)) return "无法解锁 " + WeaponCatalog.Title(id);
                added++;
            }

            if (added == 0) return "该分类没有解锁到任何普通武器";
            return "";
        }

        static EnemyHealth LivingEnemy()
        {
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (var i = 0; i < enemies.Length; i++)
                if (enemies[i] != null && enemies[i].Current > 1f)
                    return enemies[i];
            return enemies.Length > 0 ? enemies[0] : null;
        }

        void Expect(string name, bool ok)
        {
            if (ok) Pass("静态 " + name);
            else Fail("静态 " + name, "不满足");
        }

        void Pass(string name)
        {
            _pass++;
            _log.AppendLine("[PASS] " + name);
        }

        void Fail(string name, string why)
        {
            _fail++;
            _log.AppendLine("[FAIL] " + name + "  —  " + why);
        }

        void WriteReport()
        {
            var head = "Veinfire 全内容冒烟\n通过 " + _pass + "  失败 " + _fail +
                       "\n覆盖：5角色×5专武、5角色×9血脉、3地图、4模式、7模组、10诅咒、3类普通武器解锁\n\n";
            _log.Insert(0, head);
            File.WriteAllText(ReportPath, _log.ToString());
            Debug.Log(_log.ToString());
        }
    }
}
