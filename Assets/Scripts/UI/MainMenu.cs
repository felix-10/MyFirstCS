using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Veinfire
{
    public sealed class MainMenu : MonoBehaviour
    {
        enum Page { Title, Deploy, PickHero, PickMap, PickVein, PickUnique, Settings, Pause, Confirm, Mode, Mods, Meta, Codex }

        static MainMenu _i;
        Canvas _canvas;
        Page _page;
        Transform _title;
        Transform _deploy;
        Transform _pickHero;
        Transform _pickMap;
        Transform _pickVein;
        Transform _pickUnique;
        Transform _settings;
        Transform _pause;
        Transform _confirm;
        Transform _mode;
        Transform _mods;
        Transform _meta;
        Transform _codex;
        Text _confirmText;
        Text _emberLabel;
        Text _classicLabel;
        Text _modeHint;
        Text _setupSummary;
        Text _setupBackLabel;
        Text _setupNextLabel;
        readonly Text[] _setupTrail = new Text[5];
        readonly Image[] _setupTrailEdge = new Image[5];
        readonly Image[] _setupTrailInner = new Image[5];
        readonly Text[] _setupTrailArrow = new Text[4];
        readonly Button[] _setupTrailBtn = new Button[5];
        float _setupClickAt;
        int _setupClickKey;
        int _setupStep;
        int _setupReached;
        bool _leftLocked;
        GameModeId _modeDraft;
        GameObject _panelMode;
        GameObject _leftHero;
        GameObject _leftVein;
        GameObject _leftUnique;
        GameObject _btnPrev;
        readonly Image[] _setupHeroEdges = new Image[5];
        readonly Image[] _setupHeroInners = new Image[5];
        readonly Text[] _setupHeroLabels = new Text[5];
        readonly Image[] _setupVeinEdges = new Image[9];
        readonly Image[] _setupVeinInners = new Image[9];
        readonly Text[] _setupVeinLabels = new Text[9];
        readonly Image[] _setupUniqueEdges = new Image[5];
        readonly Image[] _setupUniqueInners = new Image[5];
        readonly Text[] _setupUniqueLabels = new Text[5];
        Button _playBtnSetup;
        Image _playInner;
        UiHover _playHover;
        GameObject _panelMap;
        GameObject _panelMod;
        GameObject _panelCurse;
        readonly Image[] _setupMapEdges = new Image[13];
        readonly Image[] _setupMapInners = new Image[13];
        readonly Text[] _setupMapLabels = new Text[13];
        readonly Text[] _modeLabels = new Text[4];
        readonly Image[] _modEdges = new Image[7];
        readonly Image[] _modInners = new Image[7];
        readonly Text[] _modLabels = new Text[7];
        readonly Image[] _curseEdges = new Image[10];
        readonly Image[] _curseInners = new Image[10];
        readonly Text[] _curseLabels = new Text[10];
        Text _metaBody;
        int _metaBranch;
        readonly GameObject[] _metaNodeGo = new GameObject[10];
        readonly Text[] _metaNodeText = new Text[10];
        readonly Text[] _metaNodeChip = new Text[10];
        readonly Image[] _metaNodeEdge = new Image[10];
        readonly Image[] _metaNodeInner = new Image[10];
        readonly UiHover[] _metaNodeHover = new UiHover[10];
        readonly Image[] _metaTabInner = new Image[3];
        Text _metaDetailTitle;
        Text _metaDetailBody;
        Text _metaBuyLabel;
        Image _metaBuyInner;
        int _metaPick;
        int _codexTab;
        int _codexPage;
        readonly GameObject[] _codexGo = new GameObject[8];
        readonly Text[] _codexTitle = new Text[8];
        readonly Text[] _codexMark = new Text[8];
        readonly Image[] _codexEdge = new Image[8];
        readonly Image[] _codexInner = new Image[8];
        readonly UiHover[] _codexHover = new UiHover[8];
        readonly Image[] _codexTabInner = new Image[4];
        Text _codexDetail;
        Text _modALabel;
        Text _modBLabel;
        Text _curseALabel;
        Text _curseBLabel;
        Text _curseCLabel;
        Text _modsHint;
        readonly Image[] _modeEdges = new Image[4];
        readonly Image[] _modeInners = new Image[4];
        Image _continueImg;
        Text _continueLabel;
        Text _musicLabel;
        Text _sfxLabel;
        Text _heroSlotText;
        Text _mapSlotText;
        Text _veinSlotText;
        Text _uniqueSlotText;
        Image _heroSlotEdge;
        Image _heroSlotInner;
        Image _mapSlotEdge;
        Image _mapSlotInner;
        Image _veinSlotEdge;
        Image _veinSlotInner;
        Image _uniqueSlotEdge;
        Image _uniqueSlotInner;
        Text _deployHint;
        Button _startBtn;
        Image _startInner;
        readonly Image[] _heroEdges = new Image[3];
        readonly Image[] _heroInners = new Image[3];
        readonly Image[] _mapEdges = new Image[3];
        readonly Image[] _mapInners = new Image[3];
        readonly Image[] _veinEdges = new Image[3];
        readonly Image[] _veinInners = new Image[3];
        readonly Image[] _uniqueEdges = new Image[3];
        readonly Image[] _uniqueInners = new Image[3];
        HeroId _hero;
        MapId _map;
        int _mapStar = 1;
        VeinId _vein;
        UniqueId _unique;
        HeroId _heroDraft;
        MapId _mapDraft;
        int _mapStarDraft = 1;
        VeinId _veinDraft;
        UniqueId _uniqueDraft;
        bool _heroReady;
        bool _mapReady;
        bool _veinReady;
        bool _uniqueReady;
        bool _pauseMode;

        public static void ShowTitle()
        {
            Ensure();
            _i._pauseMode = false;
            GameSession.SetMenu(true);
            _i.Open(Page.Title);
        }

        public static void ShowPause()
        {
            if (GameSession.UpgradeLock || GameSession.IsGameOver) return;
            Ensure();
            _i._pauseMode = true;
            RunSave.Capture();
            GameSession.SetMenu(true);
            _i.Open(Page.Pause);
        }

        public static void Hide()
        {
            if (_i == null) return;
            GameSession.SetMenu(false);
            _i._canvas.gameObject.SetActive(false);
        }

        static void Ensure()
        {
            if (_i != null)
            {
                _i._canvas.gameObject.SetActive(true);
                return;
            }

            GameFeel.Ensure();
            AudioHub.Ensure();
            GameSettings.Load();
            GameSettings.ApplyAudio();

            var host = new GameObject("MainMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenu));
            Object.DontDestroyOnLoad(host);
            if (Object.FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            _i = host.GetComponent<MainMenu>();
            _i.Build(host);
        }

        void Build(GameObject host)
        {
            _canvas = host.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 400;
            var scaler = host.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Stretch(ImageGo(host.transform, "Bg", UiStyle.Void).transform);
            var veil = ImageGo(host.transform, "Veil", new Color(0.02f, 0.12f, 0.16f, 0.18f));
            Stretch(veil.transform);
            veil.GetComponent<Image>().raycastTarget = false;
            UiStyle.Corners(host.transform);

            _title = MakePage(host.transform, "Title");
            var title = MakeLabel(_title, "VEINFIRE", new Vector2(0f, 280f), 68);
            title.color = UiStyle.Accent;
            var sub = MakeLabel(_title, "夜火枢纽  /  Rogue-Lite", new Vector2(0f, 210f), 20);
            sub.color = UiStyle.Muted;
            _emberLabel = MakeLabel(_title, "", new Vector2(0f, 155f), 20);
            _emberLabel.color = UiStyle.Accent;
            MakeBtn(_title, "开始游戏", new Vector2(0f, 70f), RequestNewGame);
            var cont = MakeBtn(_title, "继续游戏", new Vector2(0f, 0f), ContinueLast);
            _continueImg = cont.transform.Find("Inner").GetComponent<Image>();
            _continueLabel = cont.GetComponentInChildren<Text>();
            MakeBtn(_title, "元研究", new Vector2(0f, -70f), () => Open(Page.Meta));
            MakeBtn(_title, "图鉴成就", new Vector2(0f, -140f), () => Open(Page.Codex));
            MakeBtn(_title, "新手引导", new Vector2(0f, -210f), () => TutorialManager.Show());
            MakeBtn(_title, "设置", new Vector2(0f, -280f), () => Open(Page.Settings));
            MakeBtn(_title, "退出", new Vector2(0f, -350f), QuitGame);

            _deploy = MakePage(host.transform, "Deploy");
            MakeLabel(_deploy, "部署协议", new Vector2(0f, 420f), 36);
            _deployHint = MakeLabel(_deploy, "指定操作员、血脉与专武后，按确认开始进入对局", new Vector2(0f, 365f), 18);
            _deployHint.color = UiStyle.Muted;
            MakeSlot(_deploy, "操作员", new Vector2(-380f, 155f), OpenHeroPick, out _heroSlotEdge, out _heroSlotInner, out _heroSlotText, 350f, 200f);
            MakeSlot(_deploy, "战场", new Vector2(380f, 155f), OpenMapPick, out _mapSlotEdge, out _mapSlotInner, out _mapSlotText, 350f, 200f);
            MakeSlot(_deploy, "血脉觉醒", new Vector2(-380f, -55f), OpenVeinPick, out _veinSlotEdge, out _veinSlotInner, out _veinSlotText, 350f, 200f);
            MakeSlot(_deploy, "专武", new Vector2(380f, -55f), OpenUniquePick, out _uniqueSlotEdge, out _uniqueSlotInner, out _uniqueSlotText, 350f, 200f);
            var start = MakeBtn(_deploy, "确认开始", new Vector2(0f, -250f), ConfirmDeploy, 480f);
            _startBtn = start.GetComponent<Button>();
            _startInner = start.transform.Find("Inner").GetComponent<Image>();
            MakeBtn(_deploy, "返回", new Vector2(0f, -330f), BackFromDeploy);

            _pickHero = MakePage(host.transform, "PickHero");
            MakeLabel(_pickHero, "指定操作员", new Vector2(0f, 360f), 36);
            MakeLabel(_pickHero, "点选一名角色，再确认返回", new Vector2(0f, 300f), 18).color = UiStyle.Muted;
            MakeHeroCard(HeroId.Geralt, -420f);
            MakeHeroCard(HeroId.Liz, 0f);
            MakeHeroCard(HeroId.Antalo, 420f);
            MakeBtn(_pickHero, "确认", new Vector2(-160f, -280f), ConfirmHero, 280f);
            MakeBtn(_pickHero, "取消", new Vector2(160f, -280f), () => Open(Page.Mode), 280f);

            _pickMap = MakePage(host.transform, "PickMap");
            MakeLabel(_pickMap, "指定战场", new Vector2(0f, 360f), 36);
            MakeLabel(_pickMap, "点选一张地图，再确认返回", new Vector2(0f, 300f), 18).color = UiStyle.Muted;
            MakeMapCard(MapId.VeinWaste, -420f);
            MakeMapCard(MapId.BoneCloister, 0f);
            MakeMapCard(MapId.AshMarsh, 420f);
            MakeBtn(_pickMap, "确认", new Vector2(-160f, -280f), ConfirmMap, 280f);
            MakeBtn(_pickMap, "取消", new Vector2(160f, -280f), () => Open(Page.Mode), 280f);

            _pickVein = MakePage(host.transform, "PickVein");
            MakeLabel(_pickVein, "血脉觉醒", new Vector2(0f, 360f), 36);
            MakeLabel(_pickVein, "点选一条血脉，再确认返回", new Vector2(0f, 300f), 18).color = UiStyle.Muted;
            MakeVeinCard(VeinId.Fire, -420f);
            MakeVeinCard(VeinId.Water, 0f);
            MakeVeinCard(VeinId.Earth, 420f);
            MakeBtn(_pickVein, "确认", new Vector2(-160f, -280f), ConfirmVein, 280f);
            MakeBtn(_pickVein, "取消", new Vector2(160f, -280f), () => Open(Page.Mode), 280f);

            _pickUnique = MakePage(host.transform, "PickUnique");
            MakeLabel(_pickUnique, "选择专武", new Vector2(0f, 360f), 36);
            MakeLabel(_pickUnique, "点选一件开局武器，再确认返回", new Vector2(0f, 300f), 18).color = UiStyle.Muted;
            MakeUniqueCard(UniqueId.Hymn, -420f);
            MakeUniqueCard(UniqueId.Bolt, 0f);
            MakeUniqueCard(UniqueId.Blade, 420f);
            MakeBtn(_pickUnique, "确认", new Vector2(-160f, -280f), ConfirmUnique, 280f);
            MakeBtn(_pickUnique, "取消", new Vector2(160f, -280f), () => Open(Page.Mode), 280f);

            _settings = MakePage(host.transform, "Settings");
            MakeLabel(_settings, "设置", new Vector2(0f, 280f), 40);
            _musicLabel = MakeLabel(_settings, "音乐", new Vector2(0f, 160f), 24);
            MakeBtn(_settings, "音乐 -", new Vector2(-160f, 90f), () => NudgeMusic(-0.1f), 140f);
            MakeBtn(_settings, "音乐 +", new Vector2(160f, 90f), () => NudgeMusic(0.1f), 140f);
            _sfxLabel = MakeLabel(_settings, "音效", new Vector2(0f, 10f), 24);
            MakeBtn(_settings, "音效 -", new Vector2(-160f, -60f), () => NudgeSfx(-0.1f), 140f);
            MakeBtn(_settings, "音效 +", new Vector2(160f, -60f), () => NudgeSfx(0.1f), 140f);
            _classicLabel = MakeLabel(_settings, "", new Vector2(0f, -140f), 18);
            MakeBtn(_settings, "切换开发者 / 客户", new Vector2(0f, -210f), ToggleDeveloper, 440f, 52f,
                "开发者：全内容可用，不改客户解锁进度。客户：按元研究逐项解锁。");
            MakeBtn(_settings, "返回", new Vector2(0f, -300f), () => Open(_pauseMode ? Page.Pause : Page.Title));

            _mode = MakePage(host.transform, "Setup");
            MakeLabel(_mode, "开战配置", new Vector2(0f, 455f), 28).color = UiStyle.Accent;

            var leftBox = UiStyle.Frame(_mode, "LeftBox", new Vector2(500f, 620f), new Vector2(-560f, 40f), UiStyle.Panel);
            string[] trail = { "模式", "地图", "角色", "血脉", "专武" };
            for (var i = 0; i < 5; i++)
            {
                var x = -186f + i * 93f;
                var chip = UiStyle.Frame(leftBox.transform, "T" + i, new Vector2(72f, 36f), new Vector2(x, 278f), UiStyle.Panel);
                _setupTrailEdge[i] = chip.GetComponent<Image>();
                _setupTrailInner[i] = chip.transform.Find("Inner").GetComponent<Image>();
                var label = UiStyle.Label(chip.transform, "L", trail[i], 15, UiStyle.Muted, TextAnchor.MiddleCenter);
                var lr = label.rectTransform;
                lr.anchorMin = Vector2.zero;
                lr.anchorMax = Vector2.one;
                lr.offsetMin = Vector2.zero;
                lr.offsetMax = Vector2.zero;
                _setupTrail[i] = label;
                var step = i;
                var btn = chip.AddComponent<Button>();
                btn.targetGraphic = _setupTrailInner[i];
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => JumpSetupStep(step));
                _setupTrailBtn[i] = btn;
                var hover = chip.AddComponent<UiHover>();
                hover.Edge = _setupTrailEdge[i];
                hover.Inner = _setupTrailInner[i];
                hover.SetPalette(UiStyle.AccentDim, UiStyle.Panel, UiStyle.Accent, UiStyle.PanelHi, false);
                if (i >= 4) continue;
                var arrow = UiStyle.Label(leftBox.transform, "A" + i, "→", 18, UiStyle.Muted, TextAnchor.MiddleCenter);
                var ar = arrow.rectTransform;
                ar.anchorMin = ar.anchorMax = ar.pivot = new Vector2(0.5f, 0.5f);
                ar.anchoredPosition = new Vector2(x + 46.5f, 278f);
                ar.sizeDelta = new Vector2(28f, 36f);
                _setupTrailArrow[i] = arrow;
                arrow.raycastTarget = false;
            }

            _panelMode = MakeScrollPanel(leftBox.transform, "模式", Vector2.zero, new Vector2(200f, 200f), out var modeList).gameObject;
            HideScrollHead(_panelMode.transform);
            StretchPad(_panelMode.transform, 72f, 10f, 76f);
            for (var i = 0; i < 4; i++)
            {
                var id = (GameModeId)i;
                var row = MakeListRow(modeList, RunConfig.ModeTitle(id), 100f, () => HighlightMode(id));
                _modeEdges[i] = row.GetComponent<Image>();
                _modeInners[i] = row.transform.Find("Inner").GetComponent<Image>();
                _modeLabels[i] = row.transform.Find("L").GetComponent<Text>();
            }

            _panelMap = MakeScrollPanel(leftBox.transform, "地图", Vector2.zero, new Vector2(200f, 200f), out var mapList).gameObject;
            HideScrollHead(_panelMap.transform);
            StretchPad(_panelMap.transform, 72f, 10f, 76f);
            var maps = BalanceTables.Maps;
            for (var i = 0; i < maps.Length && i < 13; i++)
            {
                var idx = i;
                var spec = maps[i];
                var row = MakeListRow(mapList, spec.title, 88f, () => HighlightMapSlot(idx));
                _setupMapEdges[i] = row.GetComponent<Image>();
                _setupMapInners[i] = row.transform.Find("Inner").GetComponent<Image>();
                _setupMapLabels[i] = row.transform.Find("L").GetComponent<Text>();
            }

            _leftHero = MakeScrollPanel(leftBox.transform, "角色", Vector2.zero, new Vector2(200f, 200f), out var heroList).gameObject;
            HideScrollHead(_leftHero.transform);
            StretchPad(_leftHero.transform, 72f, 10f, 76f);
            for (var i = 0; i < 5; i++)
            {
                var id = (HeroId)i;
                var row = MakeListRow(heroList, HeroCatalog.Title(id), 110f, () => HighlightHero(id));
                _setupHeroEdges[i] = row.GetComponent<Image>();
                _setupHeroInners[i] = row.transform.Find("Inner").GetComponent<Image>();
                _setupHeroLabels[i] = row.transform.Find("L").GetComponent<Text>();
            }

            _leftVein = MakeScrollPanel(leftBox.transform, "血脉", Vector2.zero, new Vector2(200f, 200f), out var veinList).gameObject;
            HideScrollHead(_leftVein.transform);
            StretchPad(_leftVein.transform, 72f, 10f, 76f);
            for (var i = 0; i < 9; i++)
            {
                var id = (VeinId)i;
                var row = MakeListRow(veinList, VeinCatalog.Title(id), 110f, () => HighlightVein(id));
                _setupVeinEdges[i] = row.GetComponent<Image>();
                _setupVeinInners[i] = row.transform.Find("Inner").GetComponent<Image>();
                _setupVeinLabels[i] = row.transform.Find("L").GetComponent<Text>();
            }

            _leftUnique = MakeScrollPanel(leftBox.transform, "专武", Vector2.zero, new Vector2(200f, 200f), out var uniqueList).gameObject;
            HideScrollHead(_leftUnique.transform);
            StretchPad(_leftUnique.transform, 72f, 10f, 76f);
            for (var i = 0; i < 5; i++)
            {
                var id = (UniqueId)i;
                var row = MakeListRow(uniqueList, UniqueCatalog.Title(id), 110f, () => HighlightUnique(id));
                _setupUniqueEdges[i] = row.GetComponent<Image>();
                _setupUniqueInners[i] = row.transform.Find("Inner").GetComponent<Image>();
                _setupUniqueLabels[i] = row.transform.Find("L").GetComponent<Text>();
            }

            _btnPrev = MakeBtn(leftBox.transform, "上一项", new Vector2(-110f, -268f), SetupBack, 200f, 48f);
            _setupBackLabel = _btnPrev.transform.Find("L").GetComponent<Text>();
            var confirm = MakeBtn(leftBox.transform, "确认", new Vector2(110f, -268f), SetupNext, 210f, 48f);
            _setupNextLabel = confirm.transform.Find("L").GetComponent<Text>();
            for (var i = 0; i < 5; i++)
            {
                if (_setupTrailEdge[i] != null)
                    _setupTrailEdge[i].transform.SetAsLastSibling();
            }
            if (_btnPrev != null) _btnPrev.transform.SetAsLastSibling();
            confirm.transform.SetAsLastSibling();

            var mid = UiStyle.Frame(_mode, "Summary", new Vector2(500f, 620f), new Vector2(0f, 40f), UiStyle.Panel);
            var midTitle = UiStyle.Label(mid.transform, "ST", "已选", 22, UiStyle.Accent, TextAnchor.UpperCenter);
            var mtr = midTitle.rectTransform;
            mtr.anchorMin = new Vector2(0f, 1f);
            mtr.anchorMax = new Vector2(1f, 1f);
            mtr.pivot = new Vector2(0.5f, 1f);
            mtr.anchoredPosition = new Vector2(0f, -12f);
            mtr.sizeDelta = new Vector2(-20f, 36f);
            _setupSummary = UiStyle.Label(mid.transform, "SB", "", 18, UiStyle.Text, TextAnchor.UpperLeft);
            var msr = _setupSummary.rectTransform;
            msr.anchorMin = Vector2.zero;
            msr.anchorMax = Vector2.one;
            msr.offsetMin = new Vector2(22f, 16f);
            msr.offsetMax = new Vector2(-22f, -20f);

            var rightBox = UiStyle.Frame(_mode, "RightBox", new Vector2(500f, 620f), new Vector2(560f, 40f), UiStyle.Panel);
            _panelMod = MakeScrollPanel(rightBox.transform, "模组  （最多 2，可不选）", Vector2.zero, new Vector2(200f, 200f), out var modList).gameObject;
            StretchSplit(_panelMod.transform, true);
            RelicMod[] mods =
            {
                RelicMod.VeinResonance, RelicMod.GaleField, RelicMod.WeaponTrial,
                RelicMod.RotSpread, RelicMod.DashWay, RelicMod.FleshOffering, RelicMod.ShieldWall
            };
            for (var i = 0; i < mods.Length; i++)
            {
                var id = mods[i];
                var row = MakeListRow(modList, RunConfig.ModTitle(id), 88f, () => ToggleMod(id));
                _modEdges[i] = row.GetComponent<Image>();
                _modInners[i] = row.transform.Find("Inner").GetComponent<Image>();
                _modLabels[i] = row.transform.Find("L").GetComponent<Text>();
            }

            _panelCurse = MakeScrollPanel(rightBox.transform, "诅咒  （最多 3，可不选）", Vector2.zero, new Vector2(200f, 200f), out var curseList).gameObject;
            StretchSplit(_panelCurse.transform, false);
            CurseId[] curses =
            {
                CurseId.Armor, CurseId.Swift, CurseId.Bleed, CurseId.Miasma, CurseId.EliteFury,
                CurseId.SparseLoot, CurseId.DeathEcho, CurseId.FrostCalamity, CurseId.PlagueSpread, CurseId.ChaosMutate
            };
            for (var i = 0; i < curses.Length; i++)
            {
                var id = curses[i];
                var row = MakeListRow(curseList, RunConfig.CurseTitle(id), 88f, () => ToggleCurse(id));
                _curseEdges[i] = row.GetComponent<Image>();
                _curseInners[i] = row.transform.Find("Inner").GetComponent<Image>();
                _curseLabels[i] = row.transform.Find("L").GetComponent<Text>();
            }

            var play = MakeBtn(_mode, "开始游戏", new Vector2(0f, -420f), TryStartRun, 420f, 60f);
            _playBtnSetup = play.GetComponent<Button>();
            _playInner = play.transform.Find("Inner").GetComponent<Image>();
            _playHover = play.GetComponent<UiHover>();
            _playHover.TipTitle = "开始游戏";
            MakeLabel(_mode, "单击预览｜双击快速确认当前选项", new Vector2(-560f, -455f), 16).color = UiStyle.Muted;

            _meta = MakePage(host.transform, "Meta");
            MakeLabel(_meta, "元研究树", new Vector2(0f, 430f), 36);
            _metaBody = MakeLabel(_meta, "", new Vector2(0f, 372f), 18);
            _metaBody.color = UiStyle.Muted;
            _metaBody.rectTransform.sizeDelta = new Vector2(1400f, 56f);
            var tabArsenal = MakeBtn(_meta, "军械", new Vector2(-520f, 300f), () => SetMetaBranch(0), 220f, 56f, MetaProgress.BranchBlurb(0));
            var tabVein = MakeBtn(_meta, "血脉", new Vector2(-280f, 300f), () => SetMetaBranch(1), 220f, 56f, MetaProgress.BranchBlurb(1));
            var tabSurvive = MakeBtn(_meta, "生存", new Vector2(-40f, 300f), () => SetMetaBranch(2), 220f, 56f, MetaProgress.BranchBlurb(2));
            _metaTabInner[0] = tabArsenal.transform.Find("Inner").GetComponent<Image>();
            _metaTabInner[1] = tabVein.transform.Find("Inner").GetComponent<Image>();
            _metaTabInner[2] = tabSurvive.transform.Find("Inner").GetComponent<Image>();
            for (var i = 0; i < 10; i++)
            {
                var idx = i;
                var go = MakeBtn(_meta, "", new Vector2(-400f, 220f - i * 48f), () => SelectMetaNode(idx), 520f, 46f);
                _metaNodeGo[i] = go;
                _metaNodeEdge[i] = go.GetComponent<Image>();
                _metaNodeInner[i] = go.transform.Find("Inner").GetComponent<Image>();
                _metaNodeText[i] = go.transform.Find("L").GetComponent<Text>();
                _metaNodeText[i].fontSize = 18;
                _metaNodeText[i].alignment = TextAnchor.MiddleLeft;
                var lr = _metaNodeText[i].rectTransform;
                lr.offsetMin = new Vector2(22f, 6f);
                lr.offsetMax = new Vector2(-120f, -6f);
                _metaNodeChip[i] = MakeChip(go.transform);
                _metaNodeHover[i] = go.GetComponent<UiHover>();
            }

            var detail = UiStyle.Frame(_meta, "Detail", new Vector2(620f, 520f), new Vector2(430f, 20f), UiStyle.Panel);
            _metaDetailTitle = UiStyle.Label(detail.transform, "DT", "点选左侧节点", 26, UiStyle.Accent, TextAnchor.UpperLeft);
            var dtr = _metaDetailTitle.rectTransform;
            dtr.anchorMin = new Vector2(0f, 1f);
            dtr.anchorMax = new Vector2(1f, 1f);
            dtr.pivot = new Vector2(0.5f, 1f);
            dtr.anchoredPosition = new Vector2(0f, -22f);
            dtr.sizeDelta = new Vector2(-40f, 40f);
            _metaDetailBody = UiStyle.Label(detail.transform, "DB", "点选研究项查看完整效果，再按确认研究扣除残烬。", 18, UiStyle.Text, TextAnchor.UpperLeft);
            var dbr = _metaDetailBody.rectTransform;
            dbr.anchorMin = Vector2.zero;
            dbr.anchorMax = Vector2.one;
            dbr.offsetMin = new Vector2(24f, 90f);
            dbr.offsetMax = new Vector2(-24f, -72f);
            var buy = MakeBtn(detail.transform, "确认研究", new Vector2(0f, -210f), ConfirmMetaBuy, 360f, 56f, "确认后才会扣除残烬。");
            _metaBuyInner = buy.transform.Find("Inner").GetComponent<Image>();
            _metaBuyLabel = buy.transform.Find("L").GetComponent<Text>();
            MakeBtn(_meta, "重置研究树", new Vector2(200f, 300f), ConfirmMetaReset, 280f, 56f,
                "消耗残烬，清空局外微加成（攻速上限、血脉 DOT、生命等），保留内容解锁。开发者模式不伪造进度，重置只动当前存档里的研究层数。");
            MakeBtn(_meta, "返回", new Vector2(0f, -430f), () => Open(Page.Title));

            _codex = MakePage(host.transform, "Codex");
            MakeLabel(_codex, "收藏与成就", new Vector2(0f, 430f), 36);
            var c0 = MakeBtn(_codex, "敌人", new Vector2(-540f, 350f), () => SetCodexTab(0), 200f, 52f, "击杀后收录。");
            var c1 = MakeBtn(_codex, "构筑", new Vector2(-180f, 350f), () => SetCodexTab(1), 200f, 52f, "角色、血脉、专武与武器。");
            var c2 = MakeBtn(_codex, "成就", new Vector2(180f, 350f), () => SetCodexTab(2), 200f, 52f, "完成条件与解锁状态。");
            var c3 = MakeBtn(_codex, "对局档案", new Vector2(540f, 350f), () => SetCodexTab(3), 200f, 52f, "最近 30 局摘要，不可回放。");
            _codexTabInner[0] = c0.transform.Find("Inner").GetComponent<Image>();
            _codexTabInner[1] = c1.transform.Find("Inner").GetComponent<Image>();
            _codexTabInner[2] = c2.transform.Find("Inner").GetComponent<Image>();
            _codexTabInner[3] = c3.transform.Find("Inner").GetComponent<Image>();
            MakeBtn(_codex, "上一页", new Vector2(-220f, -430f), () => ShiftCodex(-1), 220f, 48f, "浏览更多图鉴条目。");
            MakeBtn(_codex, "下一页", new Vector2(220f, -430f), () => ShiftCodex(1), 220f, 48f, "浏览更多图鉴条目。");
            for (var i = 0; i < 8; i++)
            {
                var idx = i;
                var col = i % 4;
                var row = i / 4;
                var go = MakeBtn(_codex, "", new Vector2(-540f + col * 360f, 160f - row * 170f), () => PreviewCodex(idx), 330f, 150f);
                go.GetComponentInChildren<Text>().fontSize = 20;
                _codexGo[i] = go;
                _codexEdge[i] = go.GetComponent<Image>();
                _codexInner[i] = go.transform.Find("Inner").GetComponent<Image>();
                _codexTitle[i] = go.transform.Find("L").GetComponent<Text>();
                _codexTitle[i].alignment = TextAnchor.MiddleLeft;
                var cr = _codexTitle[i].rectTransform;
                cr.offsetMin = new Vector2(18f, 18f);
                cr.offsetMax = new Vector2(-18f, -36f);
                _codexMark[i] = MakeChip(go.transform);
                _codexHover[i] = go.GetComponent<UiHover>();
            }

            var cdetail = UiStyle.Frame(_codex, "CDetail", new Vector2(1600f, 150f), new Vector2(0f, -300f), UiStyle.Panel);
            _codexDetail = UiStyle.Label(cdetail.transform, "CD", "点选卡片查看说明。青色为已解锁，暗底为未解锁。", 20, UiStyle.Text, TextAnchor.UpperLeft);
            var cdr = _codexDetail.rectTransform;
            cdr.anchorMin = Vector2.zero;
            cdr.anchorMax = Vector2.one;
            cdr.offsetMin = new Vector2(24f, 16f);
            cdr.offsetMax = new Vector2(-24f, -16f);
            MakeBtn(_codex, "返回", new Vector2(0f, -430f), () => Open(Page.Title));

            _pause = MakePage(host.transform, "Pause");
            MakeLabel(_pause, "暂停", new Vector2(0f, 180f), 48);
            MakeBtn(_pause, "继续游戏", new Vector2(0f, 40f), ResumeRun);
            MakeBtn(_pause, "设置", new Vector2(0f, -40f), () => Open(Page.Settings));
            MakeBtn(_pause, "返回标题", new Vector2(0f, -120f), BackToTitle);

            _confirm = MakePage(host.transform, "Confirm");
            _confirmText = MakeLabel(_confirm, "覆盖提醒", new Vector2(0f, 80f), 24);
            _confirmText.rectTransform.sizeDelta = new Vector2(980f, 160f);
            MakeBtn(_confirm, "确定，开始新游戏", new Vector2(0f, -80f), OpenModeOrDeploy);
            MakeBtn(_confirm, "取消", new Vector2(0f, -160f), () => Open(Page.Title));
            UiTip.Bind(_canvas);
        }

        void Update()
        {
            if (_pauseMode && _page == Page.Pause && Input.GetKeyDown(KeyCode.Escape))
                ResumeRun();
            if (_page == Page.PickHero && Input.GetKeyDown(KeyCode.Escape))
                Open(Page.Mode);
            if (_page == Page.PickMap && Input.GetKeyDown(KeyCode.Escape))
                Open(Page.Mode);
            if (_page == Page.PickVein && Input.GetKeyDown(KeyCode.Escape))
                Open(Page.Mode);
            if (_page == Page.PickUnique && Input.GetKeyDown(KeyCode.Escape))
                Open(Page.Mode);
            if (_page == Page.Mode && Input.GetKeyDown(KeyCode.Escape))
                SetupBack();
            if ((_page == Page.Meta || _page == Page.Codex) && Input.GetKeyDown(KeyCode.Escape))
                Open(Page.Title);
        }

        void Open(Page page)
        {
            _page = page;
            _canvas.gameObject.SetActive(true);
            _title.gameObject.SetActive(page == Page.Title);
            _deploy.gameObject.SetActive(page == Page.Deploy);
            _pickHero.gameObject.SetActive(page == Page.PickHero);
            _pickMap.gameObject.SetActive(page == Page.PickMap);
            if (_pickVein != null) _pickVein.gameObject.SetActive(page == Page.PickVein);
            if (_pickUnique != null) _pickUnique.gameObject.SetActive(page == Page.PickUnique);
            _settings.gameObject.SetActive(page == Page.Settings);
            _pause.gameObject.SetActive(page == Page.Pause);
            if (_confirm != null) _confirm.gameObject.SetActive(page == Page.Confirm);
            if (_mode != null) _mode.gameObject.SetActive(page == Page.Mode);
            if (_meta != null) _meta.gameObject.SetActive(page == Page.Meta);
            if (_codex != null) _codex.gameObject.SetActive(page == Page.Codex);
            if (page == Page.Confirm && _confirmText != null)
                _confirmText.text = $"已有进行中的存档：{RunSave.Summary()}\n开始新游戏会覆盖它，且无法恢复。";
            if (page == Page.Deploy) RefreshDeploy();
            if (page == Page.PickHero) RefreshHeroPick();
            if (page == Page.PickMap) RefreshMapPick();
            if (page == Page.PickVein) RefreshVeinPick();
            if (page == Page.PickUnique) RefreshUniquePick();
            if (page == Page.Mode) RefreshSetup();
            if (page == Page.Meta) RefreshMeta();
            if (page == Page.Codex) RefreshCodex();
            RefreshContinue();
            RefreshSettings();
            RefreshHub();
        }

        void RefreshContinue()
        {
            var on = GameSettings.HasContinue;
            if (_continueImg != null)
                _continueImg.color = on ? UiStyle.PanelHi : new Color(0.05f, 0.08f, 0.1f, 0.85f);
            if (_continueLabel != null)
                _continueLabel.text = on ? $"继续游戏  ·  {RunSave.Summary()}" : "继续游戏  ·  暂无存档";
        }

        void RefreshSettings()
        {
            if (_musicLabel != null) _musicLabel.text = $"音乐  {Mathf.RoundToInt(GameSettings.Music * 100f)}%";
            if (_sfxLabel != null) _sfxLabel.text = $"音效  {Mathf.RoundToInt(GameSettings.Sfx * 100f)}%";
            if (_classicLabel != null)
                _classicLabel.text = GameSettings.Developer
                    ? "当前：开发者模式\n角色、血脉、专武、武器、融合、模式、模组、诅咒全部开放。\n升级卡不受套装限制。不写入客户解锁进度。"
                    : "当前：客户 / 玩家模式\n按元研究逐项解锁角色、血脉、专武、武器、模式、模组与诅咒。";
        }

        void RefreshHub()
        {
            if (_emberLabel == null) return;
            MetaProgress.Load();
            _emberLabel.text = GameSettings.Developer
                ? $"开发者  ·  残烬  {MetaProgress.Data.embers}    胜 {MetaProgress.Data.wins} / 败 {MetaProgress.Data.deaths}"
                : $"残烬  {MetaProgress.Data.embers}    胜 {MetaProgress.Data.wins} / 败 {MetaProgress.Data.deaths}";
        }

        void RequestNewGame()
        {
            GameFeel.Ui();
            if (RunSave.Exists) Open(Page.Confirm);
            else OpenModeOrDeploy();
        }

        void OpenModeOrDeploy()
        {
            MetaProgress.SanitizeRunPicks();
            _leftLocked = false;
            _setupStep = 0;
            _setupReached = 0;
            _mapReady = _heroReady = _veinReady = _uniqueReady = false;
            _modeDraft = MetaProgress.ModeUnlocked(RunConfig.Mode) ? RunConfig.Mode : GameModeId.Standard;
            _mapDraft = GameSettings.LastMap;
            _mapStarDraft = GameSettings.LastStar;
            _heroDraft = GameSettings.LastHero;
            _veinDraft = GameSettings.LastVein;
            _uniqueDraft = GameSettings.LastUnique;
            Open(Page.Mode);
        }

        void ContinueToDeploy()
        {
            var map = _map;
            var star = _mapStar;
            var mapOn = _mapReady;
            var hero = _hero;
            var heroOn = _heroReady;
            var vein = _vein;
            var veinOn = _veinReady;
            var unique = _unique;
            var uniqueOn = _uniqueReady;
            OpenModeOrDeploy();
            if (mapOn) { _map = map; _mapStar = star; _mapReady = true; }
            if (heroOn) { _hero = hero; _heroReady = true; }
            if (veinOn) { _vein = vein; _veinReady = true; }
            if (uniqueOn) { _unique = unique; _uniqueReady = true; }
            RefreshDeploy();
        }

        void BackFromDeploy()
        {
            if (_pauseMode)
            {
                Open(Page.Pause);
                return;
            }

            _setupStep = 4;
            _setupReached = 4;
            _leftLocked = true;
            Open(Page.Mode);
        }

        void SetupBack()
        {
            if (_setupStep <= 0)
            {
                Open(Page.Title);
                return;
            }

            _leftLocked = false;
            _setupStep -= 1;
            GameFeel.Ui();
            RefreshSetup();
        }

        void SetupNext() => AdvanceSetup(true);

        void AdvanceSetup(bool lockLast)
        {
            if (_leftLocked)
            {
                _leftLocked = false;
                GameFeel.Ui();
                RefreshSetup();
                return;
            }

            if (_setupStep == 0)
            {
                if (!MetaProgress.ModeUnlocked(_modeDraft)) return;
                RunConfig.Mode = _modeDraft;
                _setupStep = 1;
            }
            else if (_setupStep == 1)
            {
                if (!MetaProgress.MapUnlocked(_mapDraft, _mapStarDraft) && !GameSettings.Developer) return;
                _map = _mapDraft;
                _mapStar = _mapStarDraft;
                RunConfig.MapStar = _mapStar;
                _mapReady = true;
                _setupStep = 2;
            }
            else if (_setupStep == 2)
            {
                _hero = _heroDraft;
                _heroReady = true;
                _setupStep = 3;
            }
            else if (_setupStep == 3)
            {
                _vein = _veinDraft;
                _veinReady = true;
                _setupStep = 4;
            }
            else
            {
                _unique = _uniqueDraft;
                _uniqueReady = true;
                if (lockLast) _leftLocked = true;
            }

            _setupReached = Mathf.Max(_setupReached, _setupStep);
            GameFeel.Ui();
            RefreshSetup();
        }

        void JumpSetupStep(int step)
        {
            if (step < 0 || step > 4) return;
            var open = _leftLocked || step <= _setupReached || SetupStepDone(step);
            if (!open) return;
            if (!_leftLocked && step == _setupStep) return;
            _leftLocked = false;
            _setupStep = step;
            GameFeel.Ui();
            RefreshSetup();
        }

        bool ConsumeSetupDouble(int key)
        {
            var now = Time.unscaledTime;
            var dbl = key == _setupClickKey && now - _setupClickAt <= 0.32f;
            _setupClickAt = now;
            _setupClickKey = key;
            return dbl;
        }

        void SanitizeLobbyDrafts()
        {
            if (!MetaProgress.ModeUnlocked(_modeDraft)) _modeDraft = GameModeId.Standard;
            MetaProgress.ClampLobbyMap(ref _mapDraft, ref _mapStarDraft);
            if (!MetaProgress.HeroUnlocked(_heroDraft)) _heroDraft = HeroId.Geralt;
            if (!MetaProgress.VeinUnlocked(_veinDraft)) _veinDraft = VeinId.Fire;
            if (!MetaProgress.UniqueUnlocked(_uniqueDraft)) _uniqueDraft = UniqueId.Hymn;
            if (_mapReady) MetaProgress.ClampLobbyMap(ref _map, ref _mapStar);
            if (_heroReady && !MetaProgress.HeroUnlocked(_hero)) { _hero = HeroId.Geralt; }
            if (_veinReady && !MetaProgress.VeinUnlocked(_vein)) { _vein = VeinId.Fire; }
            if (_uniqueReady && !MetaProgress.UniqueUnlocked(_unique)) { _unique = UniqueId.Hymn; }
            RunConfig.MapStar = _mapReady ? _mapStar : _mapStarDraft;
        }

        void HighlightMode(GameModeId id)
        {
            if (_leftLocked || !MetaProgress.ModeUnlocked(id)) return;
            var dbl = ConsumeSetupDouble(10 + (int)id);
            _modeDraft = id;
            GameFeel.Ui();
            RefreshSetup();
            if (dbl) AdvanceSetup(false);
        }

        void HighlightMapSlot(int index)
        {
            var maps = BalanceTables.Maps;
            if (index < 0 || index >= maps.Length) return;
            var spec = maps[index];
            var id = spec.MapId;
            var star = MapCatalog.ClampStar(id, spec.star);
            if (_leftLocked || !MetaProgress.MapUnlocked(id, star)) return;
            var dbl = ConsumeSetupDouble(20 + index);
            _mapDraft = id;
            _mapStarDraft = star;
            GameFeel.Ui();
            RefreshSetup();
            if (dbl) AdvanceSetup(false);
        }

        void HighlightMap(MapId id)
        {
            HighlightMapSlot(IndexOfMap(id, _mapStarDraft));
        }

        static int IndexOfMap(MapId id, int star)
        {
            var maps = BalanceTables.Maps;
            star = MapCatalog.ClampStar(id, star);
            for (var i = 0; i < maps.Length; i++)
                if (maps[i].MapId == id && maps[i].star == star) return i;
            for (var i = 0; i < maps.Length; i++)
                if (maps[i].MapId == id) return i;
            return 0;
        }

        void HighlightHero(HeroId id)
        {
            if (_leftLocked || !MetaProgress.HeroUnlocked(id)) return;
            var dbl = ConsumeSetupDouble(30 + (int)id);
            _heroDraft = id;
            GameFeel.Ui();
            RefreshSetup();
            if (dbl) AdvanceSetup(false);
        }

        void HighlightVein(VeinId id)
        {
            if (_leftLocked || !MetaProgress.VeinUnlocked(id)) return;
            var dbl = ConsumeSetupDouble(40 + (int)id);
            _veinDraft = id;
            GameFeel.Ui();
            RefreshSetup();
            if (dbl) AdvanceSetup(false);
        }

        void HighlightUnique(UniqueId id)
        {
            if (_leftLocked || !MetaProgress.UniqueUnlocked(id)) return;
            var dbl = ConsumeSetupDouble(50 + (int)id);
            _uniqueDraft = id;
            GameFeel.Ui();
            RefreshSetup();
            if (dbl) AdvanceSetup(false);
        }

        void TryStartRun()
        {
            if (!SetupReady()) return;
            RunConfig.MapStar = _mapStar;
            GameFeel.Ui();
            Hide();
            GameInstaller.StartRun(_map, _hero, _vein, _unique);
        }

        bool SetupReady() =>
            _leftLocked && _mapReady && _heroReady && _veinReady && _uniqueReady;

        void ToggleMod(RelicMod id)
        {
            if (!MetaProgress.ModUnlocked(id)) return;
            if (!RunConfig.HasMod(id) && ModConflict.WouldBlockMod(id)) return;
            if (RunConfig.ModA == id) RunConfig.ModA = RelicMod.None;
            else if (RunConfig.ModB == id) RunConfig.ModB = RelicMod.None;
            else if (RunConfig.ModA == RelicMod.None) RunConfig.ModA = id;
            else RunConfig.ModB = id;
            GameFeel.Ui();
            RefreshSetup();
        }

        void ToggleCurse(CurseId id)
        {
            if (!MetaProgress.CurseUnlocked(id)) return;
            if (!RunConfig.HasCurse(id) && ModConflict.WouldBlockCurse(id)) return;
            if (RunConfig.CurseA == id) RunConfig.CurseA = CurseId.None;
            else if (RunConfig.CurseB == id) RunConfig.CurseB = CurseId.None;
            else if (RunConfig.CurseC == id) RunConfig.CurseC = CurseId.None;
            else if (RunConfig.CurseA == CurseId.None) RunConfig.CurseA = id;
            else if (RunConfig.CurseB == CurseId.None) RunConfig.CurseB = id;
            else RunConfig.CurseC = id;
            GameFeel.Ui();
            RefreshSetup();
        }

        void RefreshSetup()
        {
            MetaProgress.SanitizeRunPicks();
            SanitizeLobbyDrafts();
            for (var i = 0; i < 4; i++)
            {
                var id = (GameModeId)i;
                var open = MetaProgress.ModeUnlocked(id);
                var on = open && _modeDraft == id;
                PaintPick(_modeEdges[i], _modeInners[i], on);
                if (_modeLabels[i] != null)
                    _modeLabels[i].text = open
                        ? $"{RunConfig.ModeTitle(id)}\n{RunConfig.ModeBlurb(id)}"
                        : $"{RunConfig.ModeTitle(id)}\n未解锁 · 元研究·生存";
                var hover = _modeEdges[i] != null ? _modeEdges[i].GetComponent<UiHover>() : null;
                BindTip(hover, RunConfig.ModeTitle(id), open
                    ? RunConfig.ModeBlurb(id)
                    : "未解锁。玩家模式请到元研究 → 生存分支花费残烬解锁。开发者模式可直接选择。");
                hover?.SetPalette(
                    on ? UiStyle.Accent : (open ? UiStyle.AccentDim : new Color(0.22f, 0.24f, 0.26f)),
                    on ? UiStyle.Owned : (open ? UiStyle.Panel : UiStyle.Locked),
                    UiStyle.Accent, UiStyle.PanelHi, !open || _leftLocked);
                var btn = _modeEdges[i] != null ? _modeEdges[i].GetComponent<Button>() : null;
                if (btn != null) btn.interactable = open && !_leftLocked;
            }

            var maps = BalanceTables.Maps;
            for (var i = 0; i < 13; i++)
            {
                if (_setupMapEdges[i] == null) continue;
                if (i >= maps.Length)
                {
                    _setupMapEdges[i].gameObject.SetActive(false);
                    continue;
                }

                var spec = maps[i];
                var id = spec.MapId;
                var star = spec.star;
                var open = MetaProgress.MapUnlocked(id, star);
                var on = open && _mapDraft == id && _mapStarDraft == star;
                PaintPick(_setupMapEdges[i], _setupMapInners[i], on);
                if (_setupMapLabels[i] != null)
                    _setupMapLabels[i].text = open
                        ? $"{spec.title}\n残烬 {spec.ember}  生命×{spec.hp:0.0}"
                        : $"{spec.title}\n未解锁 · 通关上一星级";
                var hover = _setupMapEdges[i].GetComponent<UiHover>();
                BindTip(hover, spec.title, MapCatalog.Hover(spec, open));
                hover?.SetPalette(
                    on ? UiStyle.Accent : (open ? UiStyle.AccentDim : new Color(0.22f, 0.24f, 0.26f)),
                    on ? UiStyle.Owned : (open ? UiStyle.Panel : UiStyle.Locked),
                    UiStyle.Accent, UiStyle.PanelHi, !open || _leftLocked);
                var btn = _setupMapEdges[i].GetComponent<Button>();
                if (btn != null) btn.interactable = open && !_leftLocked;
            }

            for (var i = 0; i < 5; i++)
            {
                var id = (HeroId)i;
                var open = MetaProgress.HeroUnlocked(id);
                var on = open && _heroDraft == id;
                PaintPick(_setupHeroEdges[i], _setupHeroInners[i], on);
                if (_setupHeroLabels[i] != null)
                    _setupHeroLabels[i].text = open
                        ? $"{HeroCatalog.Title(id)}\n{HeroCatalog.Blurb(id)}"
                        : $"{HeroCatalog.Title(id)}\n未解锁 · 元研究·生存";
                var hover = _setupHeroEdges[i] != null ? _setupHeroEdges[i].GetComponent<UiHover>() : null;
                BindTip(hover, HeroCatalog.Title(id), HeroCatalog.Hover(id, open));
                hover?.SetPalette(
                    on ? UiStyle.Accent : (open ? UiStyle.AccentDim : new Color(0.22f, 0.24f, 0.26f)),
                    on ? UiStyle.Owned : (open ? UiStyle.Panel : UiStyle.Locked),
                    UiStyle.Accent, UiStyle.PanelHi, !open || _leftLocked);
                var btn = _setupHeroEdges[i] != null ? _setupHeroEdges[i].GetComponent<Button>() : null;
                if (btn != null) btn.interactable = open && !_leftLocked;
            }

            for (var i = 0; i < 9; i++)
            {
                var id = (VeinId)i;
                var open = MetaProgress.VeinUnlocked(id);
                var on = open && _veinDraft == id;
                PaintPick(_setupVeinEdges[i], _setupVeinInners[i], on);
                if (_setupVeinLabels[i] != null)
                    _setupVeinLabels[i].text = open
                        ? $"{VeinCatalog.Title(id)}\n{VeinCatalog.Blurb(id)}"
                        : $"{VeinCatalog.Title(id)}\n未解锁 · 元研究·血脉";
                var hover = _setupVeinEdges[i] != null ? _setupVeinEdges[i].GetComponent<UiHover>() : null;
                BindTip(hover, VeinCatalog.Title(id), VeinCatalog.Hover(id, open));
                hover?.SetPalette(
                    on ? UiStyle.Accent : (open ? UiStyle.AccentDim : new Color(0.22f, 0.24f, 0.26f)),
                    on ? UiStyle.Owned : (open ? UiStyle.Panel : UiStyle.Locked),
                    UiStyle.Accent, UiStyle.PanelHi, !open || _leftLocked);
                var btn = _setupVeinEdges[i] != null ? _setupVeinEdges[i].GetComponent<Button>() : null;
                if (btn != null) btn.interactable = open && !_leftLocked;
            }

            for (var i = 0; i < 5; i++)
            {
                var id = (UniqueId)i;
                var open = MetaProgress.UniqueUnlocked(id);
                var on = open && _uniqueDraft == id;
                PaintPick(_setupUniqueEdges[i], _setupUniqueInners[i], on);
                if (_setupUniqueLabels[i] != null)
                    _setupUniqueLabels[i].text = open
                        ? $"{UniqueCatalog.Title(id)}\n{UniqueCatalog.Blurb(id)}"
                        : $"{UniqueCatalog.Title(id)}\n未解锁 · 元研究·军械";
                var hover = _setupUniqueEdges[i] != null ? _setupUniqueEdges[i].GetComponent<UiHover>() : null;
                BindTip(hover, UniqueCatalog.Title(id), UniqueCatalog.Hover(id, open));
                hover?.SetPalette(
                    on ? UiStyle.Accent : (open ? UiStyle.AccentDim : new Color(0.22f, 0.24f, 0.26f)),
                    on ? UiStyle.Owned : (open ? UiStyle.Panel : UiStyle.Locked),
                    UiStyle.Accent, UiStyle.PanelHi, !open || _leftLocked);
                var btn = _setupUniqueEdges[i] != null ? _setupUniqueEdges[i].GetComponent<Button>() : null;
                if (btn != null) btn.interactable = open && !_leftLocked;
            }

            RelicMod[] mods =
            {
                RelicMod.VeinResonance, RelicMod.GaleField, RelicMod.WeaponTrial,
                RelicMod.RotSpread, RelicMod.DashWay, RelicMod.FleshOffering, RelicMod.ShieldWall
            };
            for (var i = 0; i < mods.Length; i++)
            {
                var id = mods[i];
                var open = MetaProgress.ModUnlocked(id);
                var on = open && RunConfig.HasMod(id);
                PaintPick(_modEdges[i], _modInners[i], on);
                if (_modLabels[i] != null)
                    _modLabels[i].text = open
                        ? $"{RunConfig.ModTitle(id)}\n{RunConfig.ModBlurb(id)}"
                        : $"{RunConfig.ModTitle(id)}\n未解锁 · 元研究";
                var hover = _modEdges[i] != null ? _modEdges[i].GetComponent<UiHover>() : null;
                BindTip(hover, RunConfig.ModTitle(id), open
                    ? RunConfig.ModBlurb(id)
                    : "未解锁。玩家模式请到元研究 → 军械 / 血脉分支解锁。开发者模式可直接勾选。");
                hover?.SetPalette(
                    on ? UiStyle.Accent : (open ? UiStyle.AccentDim : new Color(0.22f, 0.24f, 0.26f)),
                    on ? UiStyle.Owned : (open ? UiStyle.Panel : UiStyle.Locked),
                    UiStyle.Accent, UiStyle.PanelHi, !open);
            }

            CurseId[] curses =
            {
                CurseId.Armor, CurseId.Swift, CurseId.Bleed, CurseId.Miasma, CurseId.EliteFury,
                CurseId.SparseLoot, CurseId.DeathEcho, CurseId.FrostCalamity, CurseId.PlagueSpread, CurseId.ChaosMutate
            };
            for (var i = 0; i < curses.Length; i++)
            {
                var id = curses[i];
                var open = MetaProgress.CurseUnlocked(id);
                var on = open && RunConfig.HasCurse(id);
                PaintPick(_curseEdges[i], _curseInners[i], on);
                if (_curseLabels[i] != null)
                    _curseLabels[i].text = open
                        ? $"{RunConfig.CurseTitle(id)}\n{RunConfig.CurseBlurb(id)}"
                        : $"{RunConfig.CurseTitle(id)}\n未解锁 · 元研究·生存";
                var hover = _curseEdges[i] != null ? _curseEdges[i].GetComponent<UiHover>() : null;
                BindTip(hover, RunConfig.CurseTitle(id), open
                    ? RunConfig.CurseBlurb(id)
                    : "未解锁。玩家模式请到元研究 → 生存分支解锁。开发者模式可直接勾选。");
                hover?.SetPalette(
                    on ? UiStyle.Warn : (open ? UiStyle.AccentDim : new Color(0.22f, 0.24f, 0.26f)),
                    on ? new Color(0.22f, 0.12f, 0.08f, 0.96f) : (open ? UiStyle.Panel : UiStyle.Locked),
                    UiStyle.Warn, UiStyle.PanelHi, !open);
            }

            if (_setupSummary != null) _setupSummary.text = BuildSetupSummary();
            if (_panelMode != null) _panelMode.SetActive(_setupStep == 0);
            if (_panelMap != null) _panelMap.SetActive(_setupStep == 1);
            if (_leftHero != null) _leftHero.SetActive(_setupStep == 2);
            if (_leftVein != null) _leftVein.SetActive(_setupStep == 3);
            if (_leftUnique != null) _leftUnique.SetActive(_setupStep == 4);
            if (_btnPrev != null) _btnPrev.SetActive(_setupStep > 0 || _leftLocked);
            PaintSetupTrail();
            if (_setupBackLabel != null) _setupBackLabel.text = "上一项";
            if (_setupNextLabel != null)
            {
                if (_leftLocked) _setupNextLabel.text = "取消";
                else if (_setupStep >= 4) _setupNextLabel.text = "完成选择";
                else _setupNextLabel.text = "确认";
            }

            var ready = SetupReady();
            if (_playBtnSetup != null) _playBtnSetup.interactable = ready;
            if (_playInner != null)
                _playInner.color = ready ? UiStyle.PanelHi : new Color(0.05f, 0.08f, 0.1f, 0.9f);
            if (_playHover != null)
                _playHover.TipBody = ready ? "" : "选择还没有完成无法开始游戏";
        }

        void PaintSetupTrail()
        {
            string[] names = { "模式", "地图", "角色", "血脉", "专武" };
            for (var i = 0; i < 5; i++)
            {
                var done = SetupStepDone(i);
                var current = !_leftLocked && _setupStep == i;
                var reached = i <= _setupReached || _leftLocked;
                if (_setupTrail[i] != null)
                {
                    if (current) _setupTrail[i].text = "▸" + names[i];
                    else if (done) _setupTrail[i].text = "✓" + names[i];
                    else _setupTrail[i].text = names[i];
                    _setupTrail[i].color = current ? Color.white : (done ? UiStyle.Accent : (reached ? UiStyle.Warn : UiStyle.Muted));
                    _setupTrail[i].fontSize = current ? 16 : 14;
                }

                if (_setupTrailEdge[i] != null)
                    _setupTrailEdge[i].color = current ? UiStyle.Accent : (done ? UiStyle.AccentDim : (reached ? UiStyle.Warn : new Color(0.18f, 0.22f, 0.24f)));
                if (_setupTrailInner[i] != null)
                    _setupTrailInner[i].color = current ? UiStyle.PanelHi : (done ? UiStyle.Owned : (reached ? UiStyle.Panel : UiStyle.Locked));
                if (_setupTrailBtn[i] != null)
                    _setupTrailBtn[i].interactable = reached;
                if (_setupTrailEdge[i] != null)
                {
                    var hover = _setupTrailEdge[i].GetComponent<UiHover>();
                    hover?.SetPalette(
                        current ? UiStyle.Accent : (done ? UiStyle.AccentDim : (reached ? UiStyle.Warn : new Color(0.18f, 0.22f, 0.24f))),
                        current ? UiStyle.PanelHi : (done ? UiStyle.Owned : (reached ? UiStyle.Panel : UiStyle.Locked)),
                        UiStyle.Accent, UiStyle.PanelHi, !reached);
                    BindTip(hover, names[i],
                        current ? "正在选择这一项" : done ? "已选，点击可切换" : reached ? "正在选择，点击可回到这一项" : "尚未到达");
                }
            }

            for (var i = 0; i < 4; i++)
            {
                if (_setupTrailArrow[i] == null) continue;
                _setupTrailArrow[i].color = i < _setupReached || SetupStepDone(i) ? UiStyle.Accent : UiStyle.Muted;
            }
        }

        bool SetupStepDone(int i)
        {
            if (i == 0) return _setupReached > 0 || _leftLocked;
            if (i == 1) return _mapReady;
            if (i == 2) return _heroReady;
            if (i == 3) return _veinReady;
            return _uniqueReady && _leftLocked;
        }

        string BuildSetupSummary()
        {
            var lines = new System.Collections.Generic.List<string>();
            if (_setupStep > 0 || _leftLocked)
                lines.Add($"模式\n{RunConfig.ModeTitle(RunConfig.Mode)}\n{RunConfig.ModeBlurb(RunConfig.Mode)}");
            if (_mapReady)
                lines.Add($"地图\n{MapCatalog.Title(_map, _mapStar)}\n{MapCatalog.Blurb(_map, _mapStar)}");
            if (_heroReady)
                lines.Add($"角色\n{HeroCatalog.Title(_hero)}\n{HeroCatalog.Blurb(_hero)}");
            if (_veinReady)
                lines.Add($"血脉\n{VeinCatalog.Title(_vein)}\n{VeinCatalog.Blurb(_vein)}");
            if (_uniqueReady)
                lines.Add($"专武\n{UniqueCatalog.Title(_unique)}\n{UniqueCatalog.Blurb(_unique)}");
            if (RunConfig.ModA != RelicMod.None || RunConfig.ModB != RelicMod.None)
                lines.Add($"模组\n{ModSummary()}");
            if (RunConfig.CurseA != CurseId.None || RunConfig.CurseB != CurseId.None || RunConfig.CurseC != CurseId.None)
                lines.Add($"诅咒\n{CurseSummary()}");
            if (lines.Count > 0)
                lines.Add($"残烬倍率  ×{RunConfig.EmberMul:0.00}");
            return lines.Count == 0 ? "暂无已选项" : string.Join("\n\n", lines);
        }

        static string ModSummary()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (RunConfig.ModA != RelicMod.None) parts.Add(RunConfig.ModTitle(RunConfig.ModA));
            if (RunConfig.ModB != RelicMod.None) parts.Add(RunConfig.ModTitle(RunConfig.ModB));
            return parts.Count == 0 ? "无" : string.Join("  ·  ", parts);
        }

        static string CurseSummary()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (RunConfig.CurseA != CurseId.None) parts.Add(RunConfig.CurseTitle(RunConfig.CurseA));
            if (RunConfig.CurseB != CurseId.None) parts.Add(RunConfig.CurseTitle(RunConfig.CurseB));
            if (RunConfig.CurseC != CurseId.None) parts.Add(RunConfig.CurseTitle(RunConfig.CurseC));
            return parts.Count == 0 ? "无" : string.Join("  ·  ", parts);
        }

        void BeginDeploy()
        {
            GameSettings.Load();
            var save = RunSave.Load();
            if (save != null)
            {
                _hero = (HeroId)Mathf.Clamp(save.hero, 0, 4);
                _map = (MapId)Mathf.Clamp(save.map, 0, 4);
                _mapStar = Mathf.Clamp(save.mapStar, 1, 5);
                _vein = (VeinId)Mathf.Clamp(save.vein, 0, 8);
                _unique = (UniqueId)Mathf.Clamp(save.unique, 0, 4);
                _heroReady = _mapReady = _veinReady = _uniqueReady = true;
            }
            else if (GameSettings.HasPlayed)
            {
                _hero = GameSettings.LastHero;
                _map = GameSettings.LastMap;
                _mapStar = GameSettings.LastStar;
                _vein = GameSettings.LastVein;
                _unique = GameSettings.LastUnique;
                _heroReady = _mapReady = _veinReady = _uniqueReady = true;
            }
            else
            {
                _heroReady = false;
                _mapReady = false;
                _veinReady = false;
                _uniqueReady = false;
            }

            Open(Page.Mode);
        }

        void ContinueLast()
        {
            if (!RunSave.Exists) return;
            GameFeel.Ui();
            Hide();
            GameInstaller.ContinueRun();
        }

        void OpenUniquePick()
        {
            _uniqueDraft = _uniqueReady ? _unique : GameSettings.LastUnique;
            GameFeel.Ui();
            Open(Page.PickUnique);
        }

        void PickUnique(UniqueId id)
        {
            _uniqueDraft = id;
            GameFeel.Ui();
            RefreshUniquePick();
        }

        void ConfirmUnique()
        {
            _unique = _uniqueDraft;
            _uniqueReady = true;
            GameFeel.Ui();
            Open(Page.Mode);
        }

        void OpenVeinPick()
        {
            _veinDraft = _veinReady ? _vein : GameSettings.LastVein;
            GameFeel.Ui();
            Open(Page.PickVein);
        }

        void PickVein(VeinId id)
        {
            _veinDraft = id;
            GameFeel.Ui();
            RefreshVeinPick();
        }

        void ConfirmVein()
        {
            _vein = _veinDraft;
            _veinReady = true;
            GameFeel.Ui();
            Open(Page.Mode);
        }

        void OpenHeroPick()
        {
            _heroDraft = _heroReady ? _hero : GameSettings.LastHero;
            GameFeel.Ui();
            Open(Page.PickHero);
        }

        void OpenMapPick()
        {
            _mapDraft = _mapReady ? _map : GameSettings.LastMap;
            GameFeel.Ui();
            Open(Page.PickMap);
        }

        void PickHero(HeroId id)
        {
            _heroDraft = id;
            GameFeel.Ui();
            RefreshHeroPick();
        }

        void PickMap(MapId id)
        {
            _mapDraft = id;
            GameFeel.Ui();
            RefreshMapPick();
        }

        void ConfirmHero()
        {
            _hero = _heroDraft;
            _heroReady = true;
            GameFeel.Ui();
            Open(Page.Mode);
        }

        void ConfirmMap()
        {
            _map = _mapDraft;
            _mapStar = _mapStarDraft;
            _mapReady = true;
            GameFeel.Ui();
            Open(Page.Mode);
        }

        void ConfirmDeploy()
        {
            if (!_heroReady || !_mapReady || !_veinReady || !_uniqueReady) return;
            RunConfig.MapStar = _mapStar;
            GameFeel.Ui();
            Hide();
            GameInstaller.StartRun(_map, _hero, _vein, _unique);
        }

        void RefreshDeploy()
        {
            FillSlot(_heroSlotText, _heroSlotEdge, _heroSlotInner, _heroReady,
                "?\n未知操作员\n点击接入",
                HeroCatalog.CardText(_hero));
            FillSlot(_mapSlotText, _mapSlotEdge, _mapSlotInner, _mapReady,
                "?\n未知战场\n点击标定",
                MapCatalog.CardText(_map));
            FillSlot(_veinSlotText, _veinSlotEdge, _veinSlotInner, _veinReady,
                "?\n未知血脉\n点击觉醒",
                VeinCatalog.CardText(_vein));
            FillSlot(_uniqueSlotText, _uniqueSlotEdge, _uniqueSlotInner, _uniqueReady,
                "?\n未知专武\n点击武装",
                UniqueCatalog.CardText(_unique));

            var ready = _heroReady && _mapReady && _veinReady && _uniqueReady;
            if (_deployHint != null)
                _deployHint.text = ready
                    ? $"协议就绪  //  {HeroCatalog.Title(_hero)}  ·  {MapCatalog.Title(_map)}  ·  {VeinCatalog.Title(_vein)}  ·  {UniqueCatalog.Title(_unique)}"
                    : "指定操作员、战场、血脉与专武后，方可启动";
            if (_startBtn != null) _startBtn.interactable = ready;
            if (_startInner != null)
                _startInner.color = ready ? UiStyle.PanelHi : new Color(0.05f, 0.08f, 0.1f, 0.9f);
        }

        void RefreshUniquePick()
        {
            for (var i = 0; i < 3; i++)
                PaintPick(_uniqueEdges[i], _uniqueInners[i], (int)_uniqueDraft == i);
        }

        void RefreshVeinPick()
        {
            for (var i = 0; i < 3; i++)
                PaintPick(_veinEdges[i], _veinInners[i], (int)_veinDraft == i);
        }

        void RefreshHeroPick()
        {
            for (var i = 0; i < 3; i++)
                PaintPick(_heroEdges[i], _heroInners[i], (int)_heroDraft == i);
        }

        void RefreshMapPick()
        {
            for (var i = 0; i < 3; i++)
                PaintPick(_mapEdges[i], _mapInners[i], (int)_mapDraft == i);
        }

        static void FillSlot(Text label, Image edge, Image inner, bool ready, string unknown, string known)
        {
            if (label != null)
            {
                label.text = ready ? known : unknown;
                label.fontSize = ready ? 18 : 26;
                label.color = ready ? UiStyle.Text : UiStyle.Muted;
            }

            if (edge != null) edge.color = ready ? UiStyle.Accent : UiStyle.AccentDim;
            if (inner != null) inner.color = ready ? UiStyle.PanelHi : UiStyle.Panel;
        }

        static void PaintPick(Image edge, Image inner, bool on)
        {
            if (edge != null) edge.color = on ? UiStyle.Accent : UiStyle.AccentDim;
            if (inner != null) inner.color = on ? UiStyle.PanelHi : UiStyle.Panel;
        }

        void ResumeRun()
        {
            GameFeel.Ui();
            Hide();
        }

        void BackToTitle()
        {
            GameFeel.Ui();
            RunSave.Capture();
            _pauseMode = false;
            GameInstaller.WipeRunObjects();
            GameSession.ClearRunFlags();
            Open(Page.Title);
            GameSession.SetMenu(true);
        }

        void NudgeMusic(float d)
        {
            GameSettings.Music = Mathf.Clamp01(GameSettings.Music + d);
            GameSettings.Save();
            GameSettings.ApplyAudio();
            RefreshSettings();
            GameFeel.Ui();
        }

        void NudgeSfx(float d)
        {
            GameSettings.Sfx = Mathf.Clamp01(GameSettings.Sfx + d);
            GameSettings.Save();
            GameSettings.ApplyAudio();
            RefreshSettings();
            GameFeel.Ui();
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void MakeSlot(Transform parent, string tag, Vector2 pos, System.Action click, out Image edge, out Image inner, out Text body, float w = 420f, float h = 320f)
        {
            var go = MakeBtn(parent, "", pos, click, w, h);
            edge = go.GetComponent<Image>();
            inner = go.transform.Find("Inner").GetComponent<Image>();
            var caption = UiStyle.Label(go.transform, "Tag", tag, 16, UiStyle.Accent, TextAnchor.UpperCenter);
            var cr = caption.rectTransform;
            cr.anchorMin = new Vector2(0f, 1f);
            cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(0.5f, 1f);
            cr.anchoredPosition = new Vector2(0f, -16f);
            cr.sizeDelta = new Vector2(-24f, 28f);

            body = go.transform.Find("L").GetComponent<Text>();
            body.fontSize = 28;
            body.color = UiStyle.Muted;
            var lr = body.rectTransform;
            lr.offsetMin = new Vector2(24f, 24f);
            lr.offsetMax = new Vector2(-24f, -48f);
        }

        void MakeHeroCard(HeroId id, float x)
        {
            var text = HeroCatalog.CardText(id);
            var tip = $"{HeroCatalog.Title(id)}  ·  {HeroCatalog.Role(id)}\n{HeroCatalog.Blurb(id)}\n被动：{HeroCatalog.Passive(id)}";
            var go = MakeBtn(_pickHero, text, new Vector2(x, 40f), () => PickHero(id), 380f, 260f, tip);
            go.GetComponentInChildren<Text>().fontSize = 20;
            var i = (int)id;
            _heroEdges[i] = go.GetComponent<Image>();
            _heroInners[i] = go.transform.Find("Inner").GetComponent<Image>();
        }

        void MakeMapCard(MapId id, float x)
        {
            var go = MakeBtn(_pickMap, MapCatalog.CardText(id), new Vector2(x, 40f), () => PickMap(id), 380f, 240f, MapCatalog.Blurb(id));
            go.GetComponentInChildren<Text>().fontSize = 20;
            var i = (int)id;
            _mapEdges[i] = go.GetComponent<Image>();
            _mapInners[i] = go.transform.Find("Inner").GetComponent<Image>();
        }

        void MakeVeinCard(VeinId id, float x)
        {
            var go = MakeBtn(_pickVein, VeinCatalog.CardText(id), new Vector2(x, 40f), () => PickVein(id), 380f, 260f, VeinCatalog.Blurb(id));
            go.GetComponentInChildren<Text>().fontSize = 20;
            var i = (int)id;
            _veinEdges[i] = go.GetComponent<Image>();
            _veinInners[i] = go.transform.Find("Inner").GetComponent<Image>();
        }

        void MakeUniqueCard(UniqueId id, float x)
        {
            var go = MakeBtn(_pickUnique, UniqueCatalog.CardText(id), new Vector2(x, 40f), () => PickUnique(id), 380f, 260f,
                UniqueCatalog.Blurb(id) + "\n" + UniqueCatalog.UpgradeBlurb(id, 5));
            go.GetComponentInChildren<Text>().fontSize = 20;
            var i = (int)id;
            _uniqueEdges[i] = go.GetComponent<Image>();
            _uniqueInners[i] = go.transform.Find("Inner").GetComponent<Image>();
        }

        void MakeModeCard(GameModeId id, float x, float y)
        {
            var go = MakeBtn(_mode, $"{RunConfig.ModeTitle(id)}\n{RunConfig.ModeBlurb(id)}", new Vector2(x, y), () =>
            {
                if (!MetaProgress.ModeUnlocked(id)) return;
                RunConfig.Mode = id;
                GameFeel.Ui();
                RefreshMode();
            }, 380f, 180f);
            go.GetComponentInChildren<Text>().fontSize = 18;
            var i = (int)id;
            _modeEdges[i] = go.GetComponent<Image>();
            _modeInners[i] = go.transform.Find("Inner").GetComponent<Image>();
        }

        void RefreshMode()
        {
            MetaProgress.SanitizeRunPicks();
            for (var i = 0; i < 4; i++)
            {
                var id = (GameModeId)i;
                var open = MetaProgress.ModeUnlocked(id);
                PaintPick(_modeEdges[i], _modeInners[i], open && (int)RunConfig.Mode == i);
                var label = _modeEdges[i] != null ? _modeEdges[i].GetComponentInChildren<Text>() : null;
                if (label != null)
                    label.text = open
                        ? $"{RunConfig.ModeTitle(id)}\n{RunConfig.ModeBlurb(id)}"
                        : $"{RunConfig.ModeTitle(id)}\n未解锁 · 元研究·生存";
                var hover = _modeEdges[i] != null ? _modeEdges[i].GetComponent<UiHover>() : null;
                if (hover != null)
                {
                    hover.TipTitle = RunConfig.ModeTitle(id);
                    hover.TipBody = open
                        ? RunConfig.ModeBlurb(id)
                        : "未解锁。玩家模式请到元研究 → 生存分支花费残烬解锁。开发者模式可直接选择。";
                    hover.SetPalette(
                        open ? UiStyle.AccentDim : new Color(0.22f, 0.24f, 0.26f),
                        open ? UiStyle.Panel : UiStyle.Locked,
                        open ? UiStyle.Accent : new Color(0.22f, 0.24f, 0.26f),
                        open ? UiStyle.PanelHi : UiStyle.Locked,
                        !open);
                }
            }
            if (_modeHint != null)
                _modeHint.text = $"{RunConfig.ModeTitle(RunConfig.Mode)}  ·  {RunConfig.ModeBlurb(RunConfig.Mode)}";
        }

        void RefreshMods()
        {
            MetaProgress.SanitizeRunPicks();
            if (_modALabel != null) _modALabel.text = $"模组 A  {RunConfig.ModTitle(RunConfig.ModA)}  ·  {ModLockBlurb(RunConfig.ModA)}";
            if (_modBLabel != null) _modBLabel.text = $"模组 B  {RunConfig.ModTitle(RunConfig.ModB)}  ·  {ModLockBlurb(RunConfig.ModB)}";
            if (_curseALabel != null) _curseALabel.text = $"诅咒 A  {RunConfig.CurseTitle(RunConfig.CurseA)}  ·  {CurseLockBlurb(RunConfig.CurseA)}";
            if (_curseBLabel != null) _curseBLabel.text = $"诅咒 B  {RunConfig.CurseTitle(RunConfig.CurseB)}";
            if (_curseCLabel != null) _curseCLabel.text = $"诅咒 C  {RunConfig.CurseTitle(RunConfig.CurseC)}    残烬倍率 ×{RunConfig.EmberMul:0.00}";
            if (_modsHint != null)
                _modsHint.text = "仅显示已在元研究解锁的模组与诅咒。均可不选。";
        }

        static string ModLockBlurb(RelicMod id) => RunConfig.ModBlurb(id);

        static string CurseLockBlurb(CurseId id) => RunConfig.CurseBlurb(id);

        void CycleMod(int slot)
        {
            var cur = slot == 0 ? RunConfig.ModA : RunConfig.ModB;
            for (var n = 0; n < 8; n++)
            {
                cur = (RelicMod)(((int)cur + 1) % 8);
                if (MetaProgress.ModUnlocked(cur)) break;
            }
            if (slot == 0) RunConfig.ModA = cur;
            else RunConfig.ModB = cur;
            GameFeel.Ui();
            RefreshMods();
        }

        void CycleCurse(int slot)
        {
            var cur = slot == 0 ? RunConfig.CurseA : slot == 1 ? RunConfig.CurseB : RunConfig.CurseC;
            for (var n = 0; n < 11; n++)
            {
                cur = (CurseId)(((int)cur + 1) % 11);
                if (MetaProgress.CurseUnlocked(cur)) break;
            }
            if (slot == 0) RunConfig.CurseA = cur;
            else if (slot == 1) RunConfig.CurseB = cur;
            else RunConfig.CurseC = cur;
            GameFeel.Ui();
            RefreshMods();
        }

        void ToggleDeveloper()
        {
            GameSettings.Developer = !GameSettings.Developer;
            GameSettings.Save();
            if (!GameSettings.Developer)
            {
                MetaProgress.SanitizeRunPicks();
                if (FindFirstObjectByType<GameSession>() != null)
                {
                    MetaProgress.SanitizeInRunAssets();
                    FindFirstObjectByType<HudView>()?.ShowEvent("开发者模式关闭，已清除本局未解锁内容。");
                }
            }
            GameFeel.Ui();
            RefreshSettings();
            RefreshHub();
            if (_page == Page.Mode || _page == Page.Mods || _page == Page.Deploy) RefreshSetup();
            if (_page == Page.Meta) RefreshMeta();
            if (_page == Page.PickHero || _page == Page.PickMap || _page == Page.PickVein || _page == Page.PickUnique)
                RefreshSetup();
        }

        void SetMetaBranch(int branch)
        {
            _metaBranch = Mathf.Clamp(branch, 0, 2);
            _metaPick = 0;
            GameFeel.Ui();
            RefreshMeta();
        }

        void SelectMetaNode(int index)
        {
            var nodes = MetaProgress.BranchNodes(_metaBranch);
            if (index < 0 || index >= nodes.Length) return;
            _metaPick = index;
            PaintMetaNodes();
            FillMetaDetail();
        }

        void ConfirmMetaReset()
        {
            if (MetaProgress.ResetTree()) GameFeel.Ui();
            RefreshMeta();
            RefreshHub();
        }

        void ConfirmMetaBuy()
        {
            var nodes = MetaProgress.BranchNodes(_metaBranch);
            if (_metaPick < 0 || _metaPick >= nodes.Length) return;
            if (MetaProgress.BuyNode(nodes[_metaPick])) GameFeel.Ui();
            RefreshMeta();
            RefreshHub();
        }

        void RefreshMeta()
        {
            var d = MetaProgress.Data;
            if (_metaBody != null)
            {
                _metaBody.text = $"残烬  {d.embers}    {MetaProgress.BranchTitle(_metaBranch)}    点选节点看详情，确认后才扣除残烬";
            }

            for (var i = 0; i < 3; i++)
            {
                if (_metaTabInner[i] != null)
                    _metaTabInner[i].color = i == _metaBranch ? UiStyle.Owned : UiStyle.Panel;
            }

            var nodes = MetaProgress.BranchNodes(_metaBranch);
            if (_metaPick >= nodes.Length) _metaPick = 0;
            for (var i = 0; i < 10; i++)
            {
                var on = i < nodes.Length;
                if (_metaNodeGo[i] != null) _metaNodeGo[i].SetActive(on);
                if (!on) continue;
                var id = nodes[i];
                if (_metaNodeText[i] != null) _metaNodeText[i].text = MetaProgress.NodeTitle(id);
                var status = MetaProgress.NodeStatus(id);
                if (_metaNodeChip[i] != null)
                {
                    _metaNodeChip[i].text = status;
                    _metaNodeChip[i].color = ChipColor(status);
                }

                if (_metaNodeHover[i] != null)
                {
                    _metaNodeHover[i].OnHover = null;
                    BindTip(_metaNodeHover[i], MetaProgress.NodeTitle(id),
                        $"{status}{MetaProgress.NodeCapLine(id)}\n花费  {MetaProgress.NodeCost(id)} 残烬\n{MetaProgress.NodePrereq(id)}\n{MetaProgress.NodeDetail(id)}");
                }
            }

            PaintMetaNodes();
            FillMetaDetail();
        }

        void PaintMetaNodes()
        {
            var nodes = MetaProgress.BranchNodes(_metaBranch);
            for (var i = 0; i < nodes.Length && i < 10; i++)
            {
                var id = nodes[i];
                var maxed = MetaProgress.NodeMaxed(id);
                var locked = !MetaProgress.PrereqMet(id);
                var ready = MetaProgress.NodeReady(id);
                var selected = i == _metaPick;
                Color edge;
                Color inner;
                if (maxed)
                {
                    edge = UiStyle.Accent;
                    inner = UiStyle.Owned;
                }
                else if (locked)
                {
                    edge = new Color(0.22f, 0.24f, 0.26f);
                    inner = UiStyle.Locked;
                }
                else if (ready)
                {
                    edge = UiStyle.Warn;
                    inner = selected ? UiStyle.PanelHi : UiStyle.Panel;
                }
                else
                {
                    edge = UiStyle.AccentDim;
                    inner = UiStyle.Panel;
                }

                if (selected && !locked) edge = Color.Lerp(edge, Color.white, 0.35f);
                if (_metaNodeEdge[i] != null) _metaNodeEdge[i].color = edge;
                if (_metaNodeInner[i] != null) _metaNodeInner[i].color = inner;
                if (_metaNodeHover[i] != null)
                    _metaNodeHover[i].SetPalette(edge, inner, selected ? Color.white : UiStyle.Accent, UiStyle.PanelHi, locked);
            }
        }

        void FillMetaDetail()
        {
            var nodes = MetaProgress.BranchNodes(_metaBranch);
            if (nodes.Length == 0) return;
            var id = nodes[Mathf.Clamp(_metaPick, 0, nodes.Length - 1)];
            var status = MetaProgress.NodeStatus(id);
            if (_metaDetailTitle != null)
                _metaDetailTitle.text = MetaProgress.NodeTitle(id);
            if (_metaDetailBody != null)
            {
                _metaDetailBody.text =
                    $"状态  {status}{MetaProgress.NodeCapLine(id)}\n" +
                    $"花费  {MetaProgress.NodeCost(id)} 残烬    库存  {MetaProgress.Data.embers}\n" +
                    $"{MetaProgress.NodePrereq(id)}\n\n" +
                    MetaProgress.NodeDetail(id);
            }

            var ready = MetaProgress.NodeReady(id);
            if (_metaBuyLabel != null)
                _metaBuyLabel.text = MetaProgress.NodeMaxed(id) ? "已解锁" : ready ? "确认研究" : status;
            if (_metaBuyInner != null)
                _metaBuyInner.color = ready ? UiStyle.Owned : UiStyle.Locked;
        }

        static Color ChipColor(string status)
        {
            if (status == "已解锁") return UiStyle.Accent;
            if (status == "未解锁" || status == "可升级") return UiStyle.Warn;
            if (status == "前置锁定") return UiStyle.Muted;
            return UiStyle.Danger;
        }

        void SetCodexTab(int tab)
        {
            _codexTab = Mathf.Clamp(tab, 0, 3);
            _codexPage = 0;
            GameFeel.Ui();
            RefreshCodex();
        }

        void ShiftCodex(int dir)
        {
            var list = CodexList();
            var pages = Mathf.Max(1, (list.Length + 7) / 8);
            _codexPage = (_codexPage + dir + pages) % pages;
            GameFeel.Ui();
            RefreshCodex();
        }

        void PreviewCodex(int index)
        {
            var list = CodexList();
            var i = _codexPage * 8 + index;
            if (i < 0 || i >= list.Length) return;
            var card = list[i];
            if (_codexDetail != null)
                _codexDetail.text = $"{card.Mark}  ·  {card.Title}\n{card.Detail}";
        }

        CodexCard[] CodexList()
        {
            switch (_codexTab)
            {
                case 1: return CodexVault.Loadout();
                case 2: return CodexVault.Achievements();
                case 3: return CodexVault.Archive();
                default: return CodexVault.Enemies();
            }
        }

        void RefreshCodex()
        {
            for (var i = 0; i < 4; i++)
            {
                if (_codexTabInner[i] != null)
                    _codexTabInner[i].color = i == _codexTab ? UiStyle.Owned : UiStyle.Panel;
            }

            var list = CodexList();
            var pages = Mathf.Max(1, (list.Length + 7) / 8);
            if (_codexPage >= pages) _codexPage = 0;
            var start = _codexPage * 8;
            for (var i = 0; i < 8; i++)
            {
                var idx = start + i;
                var on = idx < list.Length;
                if (_codexGo[i] != null) _codexGo[i].SetActive(on);
                if (!on) continue;
                var card = list[idx];
                if (_codexTitle[i] != null)
                    _codexTitle[i].text = $"{card.Title}\n{card.Preview}";
                if (_codexMark[i] != null)
                {
                    _codexMark[i].text = card.Mark;
                    _codexMark[i].color = card.Unlocked ? UiStyle.Accent : UiStyle.Muted;
                }

                var edge = card.Unlocked ? card.Tint : new Color(0.22f, 0.24f, 0.26f);
                var inner = card.Unlocked ? UiStyle.Owned : UiStyle.Locked;
                if (_codexEdge[i] != null) _codexEdge[i].color = edge;
                if (_codexInner[i] != null) _codexInner[i].color = inner;
                if (_codexHover[i] != null)
                {
                    _codexHover[i].TipTitle = card.Title;
                    _codexHover[i].TipBody = card.Detail;
                    _codexHover[i].OnHover = null;
                    _codexHover[i].SetPalette(edge, inner, Color.white, UiStyle.PanelHi, !card.Unlocked);
                }
            }

            if (list.Length > 0) PreviewCodex(0);
        }

        static void MakeListHeader(Transform parent, string text)
        {
            var go = new GameObject("Head", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = 34f;
            var label = UiStyle.Label(go.transform, "L", text, 16, UiStyle.Accent, TextAnchor.MiddleLeft);
            var lr = label.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(8f, 0f);
            lr.offsetMax = new Vector2(-8f, 0f);
        }

        static GameObject MakeListRow(Transform parent, string text, float h, System.Action click)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(Button));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = h;
            le.minHeight = h;
            le.flexibleWidth = 1f;
            UiStyle.Paint(go, UiStyle.AccentDim, true);
            var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(go.transform, false);
            var ir = (RectTransform)inner.transform;
            ir.anchorMin = Vector2.zero;
            ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(2f, 2f);
            ir.offsetMax = new Vector2(-2f, -2f);
            UiStyle.Paint(inner, UiStyle.Panel, true);
            var label = UiStyle.Label(go.transform, "L", text, 16, UiStyle.Text, TextAnchor.MiddleLeft);
            var lr = label.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(12f, 8f);
            lr.offsetMax = new Vector2(-12f, -8f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = inner.GetComponent<Image>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => click());
            var hover = go.AddComponent<UiHover>();
            hover.Edge = go.GetComponent<Image>();
            hover.Inner = inner.GetComponent<Image>();
            hover.SetPalette(UiStyle.AccentDim, UiStyle.Panel, UiStyle.Accent, UiStyle.PanelHi, false);
            return go;
        }

        static Transform MakeScrollPanel(Transform parent, string title, Vector2 pos, Vector2 size, out Transform content)
        {
            var frame = UiStyle.Frame(parent, title, size, pos, UiStyle.Panel);
            var head = UiStyle.Label(frame.transform, "H", title, 20, UiStyle.Accent, TextAnchor.MiddleCenter);
            var hr = head.rectTransform;
            hr.anchorMin = new Vector2(0f, 1f);
            hr.anchorMax = new Vector2(1f, 1f);
            hr.pivot = new Vector2(0.5f, 1f);
            hr.anchoredPosition = new Vector2(0f, -8f);
            hr.sizeDelta = new Vector2(-16f, 32f);

            var viewport = new GameObject("View", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(frame.transform, false);
            var vr = viewport.GetComponent<RectTransform>();
            vr.anchorMin = Vector2.zero;
            vr.anchorMax = Vector2.one;
            vr.offsetMin = new Vector2(8f, 8f);
            vr.offsetMax = new Vector2(-8f, -44f);
            var vimg = viewport.GetComponent<Image>();
            vimg.sprite = UiStyle.Pixel;
            vimg.color = new Color(0f, 0f, 0f, 0.2f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewport.transform, false);
            var cr = contentGo.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0f, 1f);
            cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(0.5f, 1f);
            cr.anchoredPosition = Vector2.zero;
            cr.sizeDelta = Vector2.zero;
            var vg = contentGo.GetComponent<VerticalLayoutGroup>();
            vg.childAlignment = TextAnchor.UpperCenter;
            vg.childControlHeight = true;
            vg.childControlWidth = true;
            vg.childForceExpandHeight = false;
            vg.childForceExpandWidth = true;
            vg.spacing = 8f;
            vg.padding = new RectOffset(6, 6, 6, 6);
            var fit = contentGo.GetComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = frame.AddComponent<ScrollRect>();
            scroll.viewport = vr;
            scroll.content = cr;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 48f;
            content = contentGo.transform;
            return frame.transform;
        }

        static Transform MakePage(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Stretch(go.transform);
            var rail = new GameObject("Rail", typeof(RectTransform), typeof(Image));
            rail.transform.SetParent(go.transform, false);
            var rr = (RectTransform)rail.transform;
            rr.anchorMin = new Vector2(0f, 0f);
            rr.anchorMax = new Vector2(0f, 1f);
            rr.pivot = new Vector2(0f, 0.5f);
            rr.sizeDelta = new Vector2(5f, -24f);
            rr.anchoredPosition = new Vector2(10f, 0f);
            UiStyle.Paint(rail, new Color(0.28f, 0.92f, 1f, 0.22f), false);
            go.SetActive(false);
            return go.transform;
        }

        static Text MakeChip(Transform parent)
        {
            var chip = UiStyle.Label(parent, "Chip", "", 14, UiStyle.Accent, TextAnchor.UpperRight);
            var rect = chip.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-14f, -10f);
            rect.sizeDelta = new Vector2(190f, 26f);
            return chip;
        }

        static GameObject MakeBtn(Transform parent, string text, Vector2 pos, System.Action click, float w = 420f, float h = 64f, string tip = null)
        {
            var go = UiStyle.Frame(parent, text, new Vector2(w, h), pos, UiStyle.Panel);
            var inner = go.transform.Find("Inner").GetComponent<Image>();
            var button = go.AddComponent<Button>();
            button.targetGraphic = inner;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => click());

            var label = UiStyle.Label(go.transform, "L", text, h > 80f ? 20 : 24, UiStyle.Text, TextAnchor.MiddleCenter);
            var lr = label.rectTransform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(16f, 10f);
            lr.offsetMax = new Vector2(-16f, -10f);

            var onTitle = parent != null && parent.name == "Title";
            var hover = go.AddComponent<UiHover>();
            hover.Edge = go.GetComponent<Image>();
            hover.Inner = inner;
            hover.TipTitle = onTitle ? "" : (text.Contains("\n") ? text.Split('\n')[0] : text);
            hover.TipBody = onTitle ? "" : (string.IsNullOrEmpty(tip) ? (text.Contains("\n") ? text : "") : tip);
            hover.SetPalette(UiStyle.AccentDim, UiStyle.Panel, UiStyle.Accent, UiStyle.PanelHi, false);
            return go;
        }

        static Text MakeLabel(Transform parent, string text, Vector2 pos, int size)
        {
            var label = UiStyle.Label(parent, text, text, size, UiStyle.Text, TextAnchor.MiddleCenter);
            var rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(980f, 90f);
            return label;
        }

        static GameObject ImageGo(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = SpriteFactory.Solid(Color.white, 8);
            image.color = color;
            image.raycastTarget = true;
            return go;
        }

        static void BindTip(UiHover hover, string title, string body)
        {
            if (hover == null) return;
            hover.TipTitle = title ?? "";
            hover.TipBody = body ?? "";
        }

        static void Stretch(Transform t)
        {
            var rect = (RectTransform)t;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void HideScrollHead(Transform panel)
        {
            var head = panel.Find("H");
            if (head != null) head.gameObject.SetActive(false);
            var view = panel.Find("View") as RectTransform;
            if (view != null) view.offsetMax = new Vector2(-8f, -8f);
        }

        static void StretchPad(Transform t, float top, float side, float bottom = 10f)
        {
            var rect = (RectTransform)t;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(side, bottom);
            rect.offsetMax = new Vector2(-side, -top);
        }

        static void StretchSplit(Transform t, bool top)
        {
            var rect = (RectTransform)t;
            rect.anchorMin = new Vector2(0f, top ? 0.5f : 0f);
            rect.anchorMax = new Vector2(1f, top ? 1f : 0.5f);
            rect.offsetMin = new Vector2(8f, top ? 6f : 10f);
            rect.offsetMax = new Vector2(-8f, top ? -10f : -6f);
        }
    }

    public sealed class TutorialManager : MonoBehaviour
    {
        static readonly string[] Pages =
        {
            "走位与冲刺\nWASD 移动，Shift / 空格冲刺。攻击全自动，你只负责拉开距离、绕开障碍。",
            "升级六选一\n吃经验球升级。每次最多 6 张卡，按 1-6 或点击。R 刷新，每局 3 次。专武在 3 / 6 / 12 / 18 / 25 级强化。",
            "武器栏与组合\n1 把专武 + 7 把普通武器。两边都满 5 级可合成组合技能。栏满后可替换，每局限 8 次。",
            "星级地图\n开战页选择地图星级。星级越高怪物越强、残烬越多。通关上一星才能解锁下一档。",
            "作战档案\nTab 打开档案，Q/E 切换 属性 / 组合 / 战况 / 修饰。F1 战况，F2 修饰。失败也会拿到残烬。"
        };

        int _page;
        Text _body;

        public static void Show()
        {
            var existing = FindFirstObjectByType<TutorialManager>();
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                existing._page = 0;
                existing.Paint();
                return;
            }

            var host = new GameObject("Tutorial", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(TutorialManager));
            DontDestroyOnLoad(host);
            var canvas = host.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var scaler = host.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            host.GetComponent<TutorialManager>().Build(host.transform);
        }

        void Build(Transform root)
        {
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(root, false);
            var dr = (RectTransform)dim.transform;
            dr.anchorMin = Vector2.zero;
            dr.anchorMax = Vector2.one;
            dr.offsetMin = Vector2.zero;
            dr.offsetMax = Vector2.zero;
            dim.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.05f, 0.82f);

            var panel = UiStyle.Frame(root, "Panel", new Vector2(820f, 420f), Vector2.zero, UiStyle.Panel);
            var title = UiStyle.Label(panel.transform, "T", "新手引导", 28, UiStyle.Accent, TextAnchor.UpperCenter);
            var tr = title.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -24f);
            tr.sizeDelta = new Vector2(-40f, 40f);

            _body = UiStyle.Label(panel.transform, "Body", "", 20, UiStyle.Text, TextAnchor.UpperLeft);
            var br = _body.rectTransform;
            br.anchorMin = Vector2.zero;
            br.anchorMax = Vector2.one;
            br.offsetMin = new Vector2(36f, 80f);
            br.offsetMax = new Vector2(-36f, -80f);

            MakeBtn(panel.transform, "上一项", new Vector2(-200f, -160f), () => Step(-1));
            MakeBtn(panel.transform, "下一项", new Vector2(200f, -160f), () => Step(1));
            MakeBtn(panel.transform, "关闭", new Vector2(0f, -160f), () => gameObject.SetActive(false));
            Paint();
        }

        void Step(int dir)
        {
            _page = Mathf.Clamp(_page + dir, 0, Pages.Length - 1);
            Paint();
            GameFeel.Ui();
        }

        void Paint()
        {
            if (_body != null)
                _body.text = $"{_page + 1}/{Pages.Length}\n\n{Pages[_page]}";
        }

        static void MakeBtn(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction click)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(180f, 48f);
            rect.anchoredPosition = pos;
            var img = go.GetComponent<Image>();
            img.sprite = UiStyle.Pixel;
            img.color = UiStyle.PanelHi;
            go.GetComponent<Button>().onClick.AddListener(click);
            var t = new GameObject("L", typeof(RectTransform), typeof(Text));
            t.transform.SetParent(go.transform, false);
            var lr = (RectTransform)t.transform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = Vector2.zero;
            lr.offsetMax = Vector2.zero;
            var text = t.GetComponent<Text>();
            text.font = GameFonts.UI;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UiStyle.Text;
            text.fontSize = 18;
            text.text = label;
            text.raycastTarget = false;
        }
    }
}
