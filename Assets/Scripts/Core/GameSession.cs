using UnityEngine;

namespace Veinfire
{
    public sealed class GameSession : MonoBehaviour
    {
        public static bool IsPaused { get; private set; }
        public static bool IsGameOver { get; private set; }
        public static bool IsWon { get; private set; }
        public static bool UpgradeLock { get; private set; }
        public static int Kills { get; private set; }
        public static int Combo { get; private set; }

        public static bool InMenu { get; private set; }

        public PlayerHealth Player;
        public HudView Hud;
        public WeaponLoadout Loadout;
        public PlayerDash Dash;

        public void Bind(PlayerHealth player, HudView hud, WeaponLoadout loadout, PlayerDash dash)
        {
            if (Player != null) Player.Died -= OnDied;
            Player = player;
            Hud = hud;
            Loadout = loadout;
            Dash = dash;
            if (Player != null) Player.Died += OnDied;
        }
        float _time;
        static bool _userPause;
        static float _comboUntil;
        static GameSession _instance;

        static bool _sheetOpen;

        float _saveIn = 12f;

        public static float Clock => _instance != null ? _instance._time : 0f;

        public float Elapsed => _time;

        void OnEnable()
        {
            _instance = this;
            _userPause = false;
            IsGameOver = false;
            IsWon = false;
            UpgradeLock = false;
            _sheetOpen = false;
            Kills = 0;
            Combo = 0;
            RefreshPause();
        }

        void OnDisable()
        {
            if (Player != null) Player.Died -= OnDied;
            if (_instance == this) _instance = null;
            _userPause = false;
            UpgradeLock = false;
            IsGameOver = false;
            Time.timeScale = 1f;
            IsPaused = InMenu;
        }

        void Update()
        {
            if (!IsPaused && Time.timeScale != 1f)
                Time.timeScale = 1f;

            if (!IsPaused && !IsGameOver)
            {
                _time += Time.deltaTime;
                if (Time.time > _comboUntil) Combo = 0;
                _saveIn -= Time.deltaTime;
                if (_saveIn <= 0f)
                {
                    _saveIn = 12f;
                    RunSave.Capture();
                }
            }

            Hud?.SetTimer(_time);
            Hud?.SetStatus(StatusLine());

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (InMenu) return;
                ToggleSheet();
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!InMenu) OpenSheetTab(2);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (!InMenu) OpenSheetTab(3);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_sheetOpen)
                {
                    ToggleSheet();
                    return;
                }
                if (UpgradeLock) return;
                if (!InMenu) MainMenu.ShowPause();
            }

            if ((IsGameOver || IsWon) && WantRestart())
                Restart();
        }

        static bool WantRestart()
        {
            return Input.GetKeyDown(KeyCode.R)
                   || Input.GetKeyDown(KeyCode.Space)
                   || Input.GetKeyDown(KeyCode.Return);
        }

        string StatusLine()
        {
            var dash = Dash != null ? Dash.CooldownLeft : 0f;
            var dashText = dash <= 0.05f ? "冲刺就绪" : $"冲刺 {dash:0.0}s";
            var weapons = Loadout != null ? Loadout.Summary() : "";
            var combo = Combo > 1 ? $"  连斩 x{Combo}" : "";
            return $"击杀 {Kills}{combo}    {dashText}    {MapCatalog.Title(GameInstaller.CurrentMap, RunConfig.MapStar)}\nTab 档案  Q/E分页  F1战况 F2修饰    Esc 菜单\n{weapons}\n流派  {RunConfig.BuildTag()}";
        }

        public static void ClearRunFlags()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            IsGameOver = false;
            IsWon = false;
            UpgradeLock = false;
            Kills = 0;
            Combo = 0;
            _userPause = false;
            _sheetOpen = false;
            InMenu = false;
            _comboUntil = 0f;
        }

        public void RestoreClock(float time, int kills)
        {
            _time = Mathf.Max(0f, time);
            Kills = Mathf.Max(0, kills);
        }

        public static void RegisterKill(EnemyKind kind, int xp)
        {
            Kills += 1;
            Combo = Time.time < _comboUntil ? Combo + 1 : 1;
            _comboUntil = Time.time + 1.7f;
            if (RunConfig.HasMod(RelicMod.FleshOffering))
            {
                var stats = UnityEngine.Object.FindFirstObjectByType<PlayerCombatStats>();
                if (stats != null) stats.Might = Mathf.Min(2.2f, stats.Might * 1.015f);
                if (Kills % 12 == 0)
                    UnityEngine.Object.FindFirstObjectByType<PlayerHealth>()?.Damage(6f, true);
            }
        }

        public static void SetUpgradePause(bool paused)
        {
            UpgradeLock = paused;
            if (paused)
            {
                _sheetOpen = false;
                _userPause = false;
                _instance?.Hud?.ShowSheet(false);
            }

            RefreshPause();
        }

        public static void ToggleSheet()
        {
            if (IsGameOver) return;
            _sheetOpen = !_sheetOpen;
            if (_sheetOpen) _userPause = false;
            RefreshPause();
            _instance?.Hud?.ShowSheet(_sheetOpen);
        }

        public static void OpenSheetTab(int tab)
        {
            if (IsGameOver || InMenu) return;
            _sheetOpen = true;
            _userPause = false;
            RefreshPause();
            _instance?.Hud?.ShowSheet(true, tab);
        }

        public static void SetMenu(bool on)
        {
            InMenu = on;
            if (on)
            {
                _userPause = false;
                _sheetOpen = false;
                _instance?.Hud?.ShowSheet(false);
            }

            RefreshPause();
        }

        static void RefreshPause()
        {
            IsPaused = _userPause || UpgradeLock || IsGameOver || _sheetOpen || InMenu;
            Time.timeScale = 1f;
        }

        public static void Win()
        {
            if (IsGameOver) return;
            IsWon = true;
            IsGameOver = true;
            _userPause = false;
            _sheetOpen = false;
            RefreshPause();
            _instance?.Hud?.ShowSheet(false);
            var levels = _instance != null ? _instance.GetComponent<LevelDirector>() : null;
            if (levels == null) levels = UnityEngine.Object.FindFirstObjectByType<LevelDirector>();
            RunConfig.RerollsLeftAtEnd = levels != null ? levels.Rerolls : 0;
            RunConfig.LastEmbers = MetaProgress.Settle(true, _instance != null ? _instance._time : 0f, Kills, RunConfig.BossKills);
            _instance?.Hud?.ShowWin(_instance._time, Kills);
            RunSave.Clear();
        }

        void OnDied()
        {
            if (IsWon) return;
            IsGameOver = true;
            _userPause = false;
            _sheetOpen = false;
            RefreshPause();
            Hud?.ShowSheet(false);
            var levels = FindFirstObjectByType<LevelDirector>();
            RunConfig.RerollsLeftAtEnd = levels != null ? levels.Rerolls : 0;
            RunConfig.LastEmbers = MetaProgress.Settle(false, _time, Kills, RunConfig.BossKills);
            Hud?.ShowGameOver(_time, Kills);
            RunSave.Clear();
        }

        public static void Restart()
        {
            if (RunReset.Busy) return;
            Time.timeScale = 1f;
            _userPause = false;
            UpgradeLock = false;
            RunReset.Begin();
        }
    }
}
