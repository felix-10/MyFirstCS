using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Veinfire
{
    public sealed class GameInstaller : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBuild()
        {
            if (FindFirstObjectByType<MainMenu>() != null) return;
            if (FindFirstObjectByType<GameSession>() != null) return;
            GameFeel.Ensure();
            GameSettings.Load();
            GameSettings.ApplyAudio();
            MetaProgress.Load();
            MainMenu.ShowTitle();
        }

        public static MapId CurrentMap { get; private set; } = MapId.VeinWaste;
        public static HeroId CurrentHero { get; private set; } = HeroId.Geralt;
        public static VeinId CurrentVein { get; private set; } = VeinId.Fire;

        public static UniqueId CurrentUnique { get; private set; } = UniqueId.Hymn;

        public static void SetMap(MapId map, int star)
        {
            CurrentMap = map;
            RunConfig.MapStar = MapCatalog.ClampStar(map, star);
        }

        public static void SetUnique(UniqueId unique) => CurrentUnique = unique;

        public static void StartRun(MapId map, HeroId hero, VeinId vein, UniqueId unique)
        {
            var star = MapCatalog.ClampStar(map, RunConfig.MapStar);
            CurrentMap = map;
            CurrentHero = hero;
            CurrentVein = vein;
            CurrentUnique = unique;
            RunConfig.ResetRun();
            RunConfig.MapStar = star;
            RunConfig.DeveloperRun = GameSettings.Developer;
            MetaProgress.SanitizeRunPicks();
            if (!MetaProgress.MapUnlocked(map, RunConfig.MapStar) && !GameSettings.Developer)
            {
                CurrentMap = MapId.VeinWaste;
                RunConfig.MapStar = 1;
            }
            RunSave.Clear();
            GameSettings.MarkPlayed(map, hero, vein, unique);
            GameSettings.LastStar = RunConfig.MapStar;
            WipeRunObjects();
            GameSession.ClearRunFlags();
            BuildWorld(CurrentMap);
        }

        public static void ContinueRun()
        {
            var data = RunSave.Load();
            if (data == null)
            {
                MainMenu.ShowTitle();
                return;
            }

            CurrentMap = (MapId)Mathf.Clamp(data.map, 0, 4);
            CurrentHero = (HeroId)Mathf.Clamp(data.hero, 0, 4);
            CurrentVein = (VeinId)Mathf.Clamp(data.vein, 0, 8);
            CurrentUnique = (UniqueId)Mathf.Clamp(data.unique, 0, 4);
            RunSave.ApplyConfig(data);
            WipeRunObjects();
            GameSession.ClearRunFlags();
            BuildWorld(CurrentMap);
            RunSave.Apply(data);
        }

        public static void WipeRunObjects()
        {
            Time.timeScale = 1f;
            var seen = new HashSet<GameObject>();
            Collect(seen, FindObjectsByType<PlayerMotor>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<Projectile>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<EnemyBolt>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<XpOrb>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<Pickup>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<WorldHealthBar>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<DamagePopup>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<HudView>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<GameSession>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            Collect(seen, FindObjectsByType<LevelDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None));

            var named = GameObject.Find("Veinfire");
            if (named != null) seen.Add(named);
            var hud = GameObject.Find("HUD");
            if (hud != null) seen.Add(hud);
            var prefabs = GameObject.Find("_Prefabs");
            if (prefabs != null) seen.Add(prefabs);
            var map = GameObject.Find(MapCatalog.RootName);
            if (map != null) seen.Add(map);
            var fill = GameObject.Find("FillLight");
            if (fill != null) seen.Add(fill);

            foreach (var go in seen)
            {
                if (go == null) continue;
                if (go.name == "VeinfireReset" || go.name == "VeinfireAudio" || go.name == "MainMenu") continue;
                go.name = "_wiped";
                go.SetActive(false);
                Destroy(go);
            }
        }

        static void Collect(HashSet<GameObject> seen, Component[] components)
        {
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i] != null)
                    seen.Add(components[i].gameObject);
            }
        }

        public static void BuildWorld()
        {
            BuildWorld(CurrentMap);
        }

        public static void BuildWorld(MapId map)
        {
            CurrentMap = map;
            Physics.autoSyncTransforms = true;
            GameFeel.Ensure();
            EnsureLight();
            var projectile = MakeProjectilePrefab();
            var orb = MakeOrbPrefab();
            var pickup = MakePickupPrefab();
            var bolt = MakeBoltPrefab();
            var player = MakePlayer(projectile);
            MakeGround();
            MapCatalog.Build(map);
            MakeCamera(player.transform);
            MakeSpawner(player.transform, orb, pickup, bolt);
            BindSession(player);
            ArenaLook.Apply(map, player.transform, Camera.main);
        }

        static void BindExistingPlayer(GameObject player)
        {
            EnsureLight();
            EnsurePlayerKit(player, MakeProjectilePrefab());
            DisableGroundPhysics();
            if (FindFirstObjectByType<EnemySpawner>() == null)
                MakeSpawner(player.transform, MakeOrbPrefab(), MakePickupPrefab(), MakeBoltPrefab());
            if (FindFirstObjectByType<CameraFollow>() == null)
                MakeCamera(player.transform);
            else
                FindFirstObjectByType<CameraFollow>().Target = player.transform;
            if (FindFirstObjectByType<LevelDirector>() == null)
                BindSession(player);
            GameFeel.Ensure();
            ArenaLook.Apply(CurrentMap, player.transform, Camera.main);
        }

        static void EnsurePlayerKit(GameObject player, Projectile projectile)
        {
            if (player.GetComponent<PlayerCombatStats>() == null)
                player.AddComponent<PlayerCombatStats>();
            if (player.GetComponent<PlayerHealth>() == null)
                player.AddComponent<PlayerHealth>();
            if (player.GetComponent<Rigidbody>() == null)
                player.AddComponent<Rigidbody>();
            if (player.GetComponent<PlayerDash>() == null)
                player.AddComponent<PlayerDash>();
            if (player.GetComponent<HitFlash>() == null)
                player.AddComponent<HitFlash>();

            var weapon = player.GetComponent<AutoAimWeapon>() ?? player.AddComponent<AutoAimWeapon>();
            if (weapon.ProjectilePrefab == null)
                weapon.ProjectilePrefab = projectile;

            var loadout = player.GetComponent<WeaponLoadout>() ?? player.AddComponent<WeaponLoadout>();
            loadout.BindNeedle(weapon, projectile);
            UniqueCatalog.Apply(player);
            if (player.GetComponent<HeroPassives>() == null)
                player.AddComponent<HeroPassives>();
            var body = player.GetComponent<Rigidbody>();
            if (body != null) World.PrepareCharacterBody(body);
            World.MakeTriggerVolume(player);
        }

        static void BindSession(GameObject player)
        {
            var hudGo = GameObject.Find("HUD");
            Canvas canvas = hudGo != null ? hudGo.GetComponent<Canvas>() : null;
            if (canvas == null || !canvas.gameObject.activeInHierarchy || canvas.gameObject.name != "HUD")
                canvas = MakeCanvas();
            canvas.sortingOrder = 200;
            var hud = canvas.GetComponent<HudView>() ?? HudView.Create(canvas.transform);

            var systems = GameObject.Find("Veinfire");
            if (systems == null || !systems.activeInHierarchy)
                systems = new GameObject("Veinfire");
            var levels = systems.GetComponent<LevelDirector>() ?? systems.AddComponent<LevelDirector>();
            levels.Stats = player.GetComponent<PlayerCombatStats>();
            levels.Health = player.GetComponent<PlayerHealth>();
            levels.Loadout = player.GetComponent<WeaponLoadout>();
            levels.Dash = player.GetComponent<PlayerDash>();

            if (systems.GetComponent<InRunDirector>() == null)
                systems.AddComponent<InRunDirector>();
            var stats = player.GetComponent<PlayerCombatStats>();
            MetaProgress.ApplyBonuses(stats);
            if (RunConfig.HasMod(RelicMod.RotSpread) && stats != null) stats.Armor -= 4f;
            if (RunConfig.HasMod(RelicMod.ShieldWall) && stats != null) stats.Might *= 0.85f;
            var dash = player.GetComponent<PlayerDash>();
            if (RunConfig.HasMod(RelicMod.DashWay) && dash != null) dash.Cooldown *= 1.2f;

            var session = systems.GetComponent<GameSession>() ?? systems.AddComponent<GameSession>();
            session.Bind(levels.Health, hud, levels.Loadout, levels.Dash);
            hud.Bind(levels.Health, levels);

            var spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null) spawner.Hud = hud;
        }

        static GameObject MakePlayer(Projectile projectile)
        {
            var go = PrimitiveFactory.Capsule("Player", HeroCatalog.Tint(CurrentHero), true);
            go.transform.position = new Vector3(0f, 1f, 0f);
            var stats = go.AddComponent<PlayerCombatStats>();
            HeroCatalog.Apply(CurrentHero, stats);
            go.AddComponent<PlayerHealth>();
            go.AddComponent<PlayerMotor>();
            go.AddComponent<PlayerDash>();
            go.AddComponent<HitFlash>();
            EnsurePlayerKit(go, projectile);
            return go;
        }

        static void MakeSpawner(Transform player, XpOrb orb, Pickup pickup, EnemyBolt bolt)
        {
            var go = new GameObject("EnemySpawner");
            var spawner = go.AddComponent<EnemySpawner>();
            spawner.Player = player;
            spawner.OrbPrefab = orb;
            spawner.PickupPrefab = pickup;
            spawner.BoltPrefab = bolt;
            spawner.SwarmPrefab = MakeEnemy("Swarm", new Color(0.35f, 0.75f, 0.42f));
            spawner.RunnerPrefab = MakeEnemy("Runner", new Color(0.45f, 0.95f, 0.55f));
            spawner.TankPrefab = MakeEnemy("Tank", new Color(0.2f, 0.45f, 0.28f));
            spawner.ExploderPrefab = MakeEnemy("Exploder", new Color(1f, 0.5f, 0.15f));
            spawner.SpitterPrefab = MakeEnemy("Spitter", new Color(0.55f, 0.35f, 0.85f));
            spawner.ElitePrefab = MakeEnemy("Elite", new Color(0.95f, 0.85f, 0.2f));
            spawner.BossPrefab = MakeEnemy("Boss", new Color(0.7f, 0.08f, 0.18f));
        }

        static EnemyChase MakeEnemy(string name, Color color)
        {
            var go = PrimitiveFactory.Capsule(name, color, true);
            go.transform.position = new Vector3(9999f, 1f, 9999f);
            go.SetActive(false);
            go.AddComponent<HitFlash>();
            go.AddComponent<EnemyHealth>();
            go.AddComponent<ContactDamager>();
            var chase = go.AddComponent<EnemyChase>();
            return chase;
        }

        static Projectile MakeProjectilePrefab()
        {
            var go = PrimitiveFactory.Sphere("Projectile", new Color(1f, 0.85f, 0.35f), 0.22f, true);
            go.transform.position = new Vector3(9999f, 1f, 9999f);
            go.GetComponent<Rigidbody>().isKinematic = true;
            var projectile = go.AddComponent<Projectile>();
            PrimitiveFactory.AddTrail(go, new Color(1f, 0.82f, 0.28f), 0.14f, 0.16f);
            go.SetActive(false);
            return projectile;
        }

        static EnemyBolt MakeBoltPrefab()
        {
            var go = PrimitiveFactory.Sphere("EnemyBolt", new Color(0.85f, 0.35f, 1f), 0.28f, true);
            go.transform.position = new Vector3(9999f, 1f, 9999f);
            go.GetComponent<Rigidbody>().isKinematic = true;
            var col = go.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            var bolt = go.AddComponent<EnemyBolt>();
            PrimitiveFactory.AddTrail(go, new Color(0.85f, 0.35f, 1f), 0.08f, 0.12f);
            go.SetActive(false);
            return bolt;
        }

        static XpOrb MakeOrbPrefab()
        {
            var go = PrimitiveFactory.Sphere("XpOrb", new Color(0.45f, 0.75f, 1f), 0.28f, true);
            go.transform.position = new Vector3(9999f, 0.4f, 9999f);
            go.GetComponent<Rigidbody>().isKinematic = true;
            var orb = go.AddComponent<XpOrb>();
            go.SetActive(false);
            return orb;
        }

        static Pickup MakePickupPrefab()
        {
            var go = PrimitiveFactory.Sphere("Pickup", new Color(0.2f, 0.95f, 0.35f), 0.42f, true);
            go.transform.position = new Vector3(9999f, 0.4f, 9999f);
            go.GetComponent<Rigidbody>().isKinematic = true;
            var pickup = go.AddComponent<Pickup>();
            go.SetActive(false);
            return pickup;
        }

        static void MakeCamera(Transform target)
        {
            Camera camera;
            if (Camera.main != null)
            {
                camera = Camera.main;
            }
            else
            {
                var go = new GameObject("Main Camera");
                camera = go.AddComponent<Camera>();
                go.tag = "MainCamera";
                go.AddComponent<AudioListener>();
            }

            camera.orthographic = false;
            camera.fieldOfView = 50f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 120f;
            camera.backgroundColor = new Color(0.07f, 0.03f, 0.05f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            if (camera.GetComponent<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();

            var follow = camera.GetComponent<CameraFollow>() ?? camera.gameObject.AddComponent<CameraFollow>();
            follow.Target = target;
            follow.Offset = new Vector3(0f, 14f, -11f);
            camera.transform.position = target.position + follow.Offset;
            camera.transform.LookAt(target.position + Vector3.up * 0.8f);
        }

        static void MakeGround()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(8f, 1f, 8f);
                PrimitiveFactory.Paint(ground, new Color(0.16f, 0.06f, 0.08f));
            }

            DisableGroundPhysics();
        }

        static void DisableGroundPhysics()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null) return;
            foreach (var collider in ground.GetComponentsInChildren<Collider>())
                Object.Destroy(collider);
        }

        static void EnsureLight()
        {
            if (FindFirstObjectByType<Light>() != null) return;
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static Canvas MakeCanvas()
        {
            var go = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            return canvas;
        }
    }
}
