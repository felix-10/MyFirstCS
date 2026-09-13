using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Veinfire
{
    public sealed class CharacterSheet : MonoBehaviour
    {
        enum Tab
        {
            Profile,
            Fusion,
            Report,
            Mods
        }

        LevelDirector _levels;
        Tab _tab;
        Text _header;
        Text _profile;
        Text _report;
        Text _mods;
        Transform _weaponGrid;
        Transform _passiveGrid;
        Transform _fusionContent;
        readonly List<GameObject> _icons = new List<GameObject>();
        readonly List<GameObject> _fusionRows = new List<GameObject>();
        readonly List<Image> _tabFill = new List<Image>();
        readonly List<Text> _tabLabel = new List<Text>();
        readonly List<GameObject> _pages = new List<GameObject>();

        public static CharacterSheet Create(Transform canvas)
        {
            var go = new GameObject("CharacterSheet", typeof(RectTransform), typeof(CanvasGroup), typeof(CharacterSheet));
            go.transform.SetParent(canvas, false);
            var group = go.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            var sheet = go.GetComponent<CharacterSheet>();
            sheet.Build();
            Canvas.ForceUpdateCanvases();
            go.SetActive(false);
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
            return sheet;
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            if (Input.GetKeyDown(KeyCode.Q)) SetTab((Tab)(((int)_tab + 3) % 4));
            if (Input.GetKeyDown(KeyCode.E)) SetTab((Tab)(((int)_tab + 1) % 4));
        }

        void Build()
        {
            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var dim = NewImage(transform, "Dim", new Color(0.01f, 0.03f, 0.04f, 0.82f));
            Stretch(dim);

            var frame = NewImage(transform, "Frame", UiStyle.AccentDim);
            var frameRect = (RectTransform)frame;
            frameRect.anchorMin = frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(1244f, 744f);
            UiStyle.Corners(frame);

            var panel = NewImage(frame, "Panel", new Color(0.035f, 0.07f, 0.09f, 0.98f));
            Stretch(panel);
            var panelRect = (RectTransform)panel;
            panelRect.offsetMin = new Vector2(2f, 2f);
            panelRect.offsetMax = new Vector2(-2f, -2f);

            _header = MakeLabel(panel, "Header", "OPERATOR ARCHIVE", new Vector2(28f, -18f), 22, TextAnchor.UpperLeft, new Vector2(0f, 1f));
            _header.color = UiStyle.Accent;
            _header.rectTransform.sizeDelta = new Vector2(640f, 32f);

            MakeLabel(panel, "Hint", "Q / E 切换分页    TAB 关闭", new Vector2(-28f, -20f), 16, TextAnchor.UpperRight, new Vector2(1f, 1f)).color = UiStyle.Muted;

            var line = NewImage(panel, "Rule", new Color(0.28f, 0.92f, 1f, 0.22f));
            var lr = (RectTransform)line;
            lr.anchorMin = lr.anchorMax = new Vector2(0.5f, 1f);
            lr.pivot = new Vector2(0.5f, 1f);
            lr.anchoredPosition = new Vector2(0f, -56f);
            lr.sizeDelta = new Vector2(1180f, 1f);

            string[] names = { "属性", "组合", "战况", "修饰" };
            for (var i = 0; i < names.Length; i++)
            {
                var idx = i;
                var btnGo = new GameObject("Tab" + i, typeof(RectTransform), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(panel, false);
                var br = (RectTransform)btnGo.transform;
                br.anchorMin = br.anchorMax = new Vector2(0f, 1f);
                br.pivot = new Vector2(0f, 1f);
                br.anchoredPosition = new Vector2(28f + i * 168f, -68f);
                br.sizeDelta = new Vector2(156f, 40f);
                var fill = btnGo.GetComponent<Image>();
                fill.sprite = UiStyle.Pixel;
                fill.color = UiStyle.Panel;
                _tabFill.Add(fill);
                var btn = btnGo.GetComponent<Button>();
                btn.targetGraphic = fill;
                btn.onClick.AddListener(() => SetTab((Tab)idx));
                var label = MakeLabel(btnGo.transform, "L", names[i], Vector2.zero, 18, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f));
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = Vector2.zero;
                label.rectTransform.offsetMax = Vector2.zero;
                label.raycastTarget = false;
                _tabLabel.Add(label);
            }

            var profile = Page(panel, "Profile");
            _profile = MakeLabel(profile, "Stats", "", new Vector2(24f, -16f), 17, TextAnchor.UpperLeft, new Vector2(0f, 1f));
            _profile.rectTransform.sizeDelta = new Vector2(420f, 560f);
            _profile.horizontalOverflow = HorizontalWrapMode.Wrap;
            _profile.verticalOverflow = VerticalWrapMode.Truncate;
            MakeLabel(profile, "WTitle", "武装", new Vector2(-24f, -16f), 16, TextAnchor.UpperRight, new Vector2(1f, 1f)).color = UiStyle.Muted;
            _weaponGrid = MakeGrid(profile, "Weapons", new Vector2(-24f, -44f), new Vector2(700f, 168f), 6);
            MakeLabel(profile, "PTitle", "被动", new Vector2(-24f, -228f), 16, TextAnchor.UpperRight, new Vector2(1f, 1f)).color = UiStyle.Muted;
            _passiveGrid = MakeGrid(profile, "Passives", new Vector2(-24f, -256f), new Vector2(700f, 320f), 6);

            var fusion = Page(panel, "Fusion");
            _fusionContent = MakeList(fusion, "FusionList", new Vector2(0f, -8f), new Vector2(1170f, 560f));

            var report = Page(panel, "Report");
            _report = BodyText(report, "ReportBody");

            var mods = Page(panel, "Mods");
            _mods = BodyText(mods, "ModsBody");

            SetTab(Tab.Profile);
        }

        Transform Page(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(16f, 16f);
            rect.offsetMax = new Vector2(-16f, -118f);
            _pages.Add(go);
            return go.transform;
        }

        static Text BodyText(Transform parent, string name)
        {
            var label = MakeLabel(parent, name, "", new Vector2(20f, -12f), 18, TextAnchor.UpperLeft, new Vector2(0f, 1f));
            label.rectTransform.sizeDelta = new Vector2(1120f, 540f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        void SetTab(Tab tab)
        {
            _tab = tab;
            for (var i = 0; i < _pages.Count; i++)
                _pages[i].SetActive(i == (int)tab);
            for (var i = 0; i < _tabFill.Count; i++)
            {
                var on = i == (int)tab;
                _tabFill[i].color = on ? new Color(0.08f, 0.28f, 0.32f, 1f) : new Color(0.05f, 0.1f, 0.12f, 0.92f);
                if (i < _tabLabel.Count)
                    _tabLabel[i].color = on ? UiStyle.Accent : UiStyle.Muted;
            }
            if (_levels != null) Refresh(_levels);
        }

        public void OpenTab(int tab)
        {
            SetTab((Tab)Mathf.Clamp(tab, 0, 3));
        }

        public void Show(bool on, LevelDirector levels)
        {
            gameObject.SetActive(on);
            if (!on) return;
            transform.SetAsLastSibling();
            _levels = levels;
            Refresh(levels);
        }

        public void Refresh(LevelDirector levels)
        {
            _levels = levels;
            if (levels == null) return;
            FillProfile(levels);
            FillFusion(levels.Loadout);
            FillReport();
            FillMods();
        }

        void FillProfile(LevelDirector levels)
        {
            var stats = levels.Stats;
            var health = levels.Health;
            var dash = levels.Dash;
            var loadout = levels.Loadout;
            if (stats == null || health == null || _profile == null) return;

            _header.text = $"{HeroCatalog.Title(GameInstaller.CurrentHero)}   ·   {VeinCatalog.Title(GameInstaller.CurrentVein)}   ·   {UniqueCatalog.Title(GameInstaller.CurrentUnique)}";
            _profile.text =
                $"编制  {HeroCatalog.Role(GameInstaller.CurrentHero)}\n" +
                $"天赋  {HeroCatalog.Passive(GameInstaller.CurrentHero)}\n\n" +
                $"等级  {levels.Level}\n" +
                $"生命  {Mathf.FloorToInt(health.Current)} / {Mathf.CeilToInt(health.Max)}\n" +
                $"移速  {stats.MoveSpeed:0.0}\n" +
                $"伤害  {stats.ScaledDamage:0.0}    力量  ×{stats.Might:0.00}\n" +
                $"暴击  {stats.CritChance * 100f:0}%    暴伤  ×{stats.CritDamage:0.00}\n" +
                $"间隔  {stats.ScaledInterval(stats.FireInterval):0.00}s\n" +
                $"弹数  {stats.ShotCount}    穿透  {stats.Pierce}\n" +
                $"范围  ×{stats.Area:0.00}    弹速  {stats.ProjectileSpeed:0.0}\n" +
                $"护甲  {stats.Armor:0.0}    吸血  {stats.LifeSteal:0.0}\n" +
                $"回复  {stats.HealthRegen:0.0}/秒\n" +
                $"瞬吸  {(stats.SiphonXp ? "开启" : "关闭")}\n" +
                $"拾取  {stats.PickupRadius:0.0}    击退  {stats.Knockback:0.0}\n" +
                $"冲刺  {(dash != null ? dash.Cooldown : 0f):0.00}s\n" +
                $"冷却  ×{stats.CooldownMul:0.00}\n\n" +
                $"栏位  {(loadout != null ? loadout.Count : 0)} / {WeaponLoadout.MaxWeapons}";

            ClearIcons();
            if (loadout != null)
            {
                foreach (var id in loadout.Owned)
                {
                    var unique = UniqueCatalog.IsUniqueWeapon(id);
                    AddIcon(_weaponGrid, unique ? "专" : MarkOf(id), WeaponCatalog.Title(id), loadout.LevelOf(id), FamilyColor(id), true);
                }
            }

            var passives = levels.Passives;
            for (var i = 0; i < passives.Count; i++)
            {
                var trait = passives[i];
                AddIcon(_passiveGrid, trait.Mark, trait.Title, trait.Stacks, trait.Color, true);
            }
            if (passives.Count == 0)
                AddIcon(_passiveGrid, "—", "尚无被动", 0, new Color(0.32f, 0.32f, 0.32f), true);
        }

        void FillFusion(WeaponLoadout loadout)
        {
            for (var i = 0; i < _fusionRows.Count; i++)
                if (_fusionRows[i] != null) Destroy(_fusionRows[i]);
            _fusionRows.Clear();
            if (_fusionContent == null) return;

            for (var i = 0; i < FusionCatalog.All.Length; i++)
            {
                var r = FusionCatalog.All[i];
                var state = FusionCatalog.StateOf(r, loadout);
                _fusionRows.Add(FusionRow(_fusionContent, r, state, loadout));
            }
        }

        static GameObject FusionRow(Transform parent, FusionRecipe r, FusionCatalog.RowState state, WeaponLoadout loadout)
        {
            var go = new GameObject(r.Id, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = UiStyle.Pixel;
            image.color = TintOf(state);
            var layout = go.AddComponent<LayoutElement>();
            layout.minHeight = 92f;
            layout.preferredHeight = 92f;

            var pill = MakeLabel(go.transform, "State", FusionCatalog.StateTitle(state), new Vector2(16f, 0f), 15, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f));
            pill.rectTransform.sizeDelta = new Vector2(90f, 28f);
            pill.color = ColorOf(state);

            var title = MakeLabel(go.transform, "Title", r.Title, new Vector2(118f, 18f), 20, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f));
            title.rectTransform.sizeDelta = new Vector2(420f, 28f);
            title.color = UiStyle.Text;

            var pair = $"{OwnedMark(loadout, r.A)}{WeaponCatalog.Title(r.A)}   +   {OwnedMark(loadout, r.B)}{WeaponCatalog.Title(r.B)}";
            var sub = MakeLabel(go.transform, "Pair", pair, new Vector2(118f, -14f), 16, TextAnchor.MiddleLeft, new Vector2(0f, 0.5f));
            sub.rectTransform.sizeDelta = new Vector2(520f, 24f);
            sub.color = UiStyle.Muted;

            var fx = MakeLabel(go.transform, "Fx", r.Effect, new Vector2(-20f, 0f), 16, TextAnchor.MiddleRight, new Vector2(1f, 0.5f));
            fx.rectTransform.sizeDelta = new Vector2(460f, 72f);
            fx.horizontalOverflow = HorizontalWrapMode.Wrap;
            fx.color = new Color(0.78f, 0.9f, 0.94f, 0.95f);
            var hover = go.AddComponent<UiHover>();
            hover.TipTitle = r.Title;
            hover.TipBody = $"{pair}\n{r.Effect}\n{FusionCatalog.StateTitle(state)}" +
                            (r.Pack ? "\n玩家模式需解锁高级融合配方包。开发者模式全部可见。" : "\n玩家模式需解锁组合进化。开发者模式全部可见。");
            return go;
        }

        static string OwnedMark(WeaponLoadout loadout, WeaponId id)
        {
            if (loadout == null || !loadout.Owns(id)) return "○ ";
            return $"● Lv.{loadout.LevelOf(id)} ";
        }

        static Color TintOf(FusionCatalog.RowState s)
        {
            switch (s)
            {
                case FusionCatalog.RowState.Done: return new Color(0.16f, 0.14f, 0.05f, 0.96f);
                case FusionCatalog.RowState.Ready: return new Color(0.16f, 0.12f, 0.04f, 0.96f);
                case FusionCatalog.RowState.Progress: return new Color(0.04f, 0.14f, 0.16f, 0.96f);
                case FusionCatalog.RowState.Spent: return new Color(0.08f, 0.06f, 0.06f, 0.92f);
                case FusionCatalog.RowState.Locked: return new Color(0.06f, 0.07f, 0.08f, 0.92f);
                default: return new Color(0.05f, 0.08f, 0.1f, 0.9f);
            }
        }

        static Color ColorOf(FusionCatalog.RowState s)
        {
            switch (s)
            {
                case FusionCatalog.RowState.Done: return new Color(1f, 0.84f, 0.38f);
                case FusionCatalog.RowState.Ready: return new Color(1f, 0.72f, 0.28f);
                case FusionCatalog.RowState.Progress: return new Color(0.4f, 0.92f, 0.95f);
                case FusionCatalog.RowState.Spent: return new Color(0.7f, 0.45f, 0.4f);
                case FusionCatalog.RowState.Locked: return UiStyle.Muted;
                default: return new Color(0.55f, 0.62f, 0.68f);
            }
        }

        void FillReport()
        {
            if (_report == null) return;
            var relic = string.IsNullOrEmpty(RunConfig.RelicName) ? "无" : RunConfig.RelicName;
            var ev = string.IsNullOrEmpty(RunConfig.EventName) ? "无" : RunConfig.EventName;
            _report.text =
                $"模式        {RunConfig.ModeTitle(RunConfig.Mode)}\n" +
                $"地图        {MapCatalog.Title(GameInstaller.CurrentMap, RunConfig.MapStar)}\n" +
                $"存活        {Mathf.FloorToInt(GameSession.Clock / 60f):00}:{Mathf.FloorToInt(GameSession.Clock % 60f):00}\n" +
                $"击杀        {GameSession.Kills}        Boss  {RunConfig.BossKills}\n" +
                $"输出        {RunConfig.DamageDealt:0}\n" +
                $"承伤        {RunConfig.DamageTaken:0}\n" +
                $"连斩        {GameSession.Combo}\n" +
                $"流派        {RunConfig.BuildTag()}\n" +
                $"遗物        {relic}\n" +
                $"事件        {ev}\n" +
                $"修饰        {RunConfig.ModsLine()}";
        }

        void FillMods()
        {
            if (_mods == null) return;
            var body = "当前生效\n" + RunConfig.ModsLine() + "\n\n";
            var warn = ModConflict.WarnLine();
            if (!string.IsNullOrEmpty(warn)) body += warn + "\n";
            body += DescribeMod(RunConfig.ModA);
            if (RunConfig.ModB != RunConfig.ModA) body += DescribeMod(RunConfig.ModB);
            body += DescribeCurse(RunConfig.CurseA);
            if (RunConfig.CurseB != RunConfig.CurseA) body += DescribeCurse(RunConfig.CurseB);
            if (RunConfig.CurseC != RunConfig.CurseA && RunConfig.CurseC != RunConfig.CurseB) body += DescribeCurse(RunConfig.CurseC);
            if (RunConfig.ModA == RelicMod.None && RunConfig.ModB == RelicMod.None
                && RunConfig.CurseA == CurseId.None && RunConfig.CurseB == CurseId.None && RunConfig.CurseC == CurseId.None)
                body += "本局未启用模组或诅咒。";
            _mods.text = body;
        }

        static string DescribeMod(RelicMod id)
        {
            if (id == RelicMod.None) return "";
            return RunConfig.ModTitle(id) + "\n" + RunConfig.ModBlurb(id) + "\n\n";
        }

        static string DescribeCurse(CurseId id)
        {
            if (id == CurseId.None) return "";
            return RunConfig.CurseTitle(id) + "\n" + RunConfig.CurseBlurb(id) + "\n\n";
        }

        static string MarkOf(WeaponId id)
        {
            var t = WeaponCatalog.Title(id);
            return t.Length > 0 ? t.Substring(0, 1) : "武";
        }

        static Color FamilyColor(WeaponId id)
        {
            switch (WeaponCatalog.FamilyOf(id))
            {
                case WeaponCatalog.WeaponFamily.Melee: return new Color(0.95f, 0.52f, 0.28f);
                case WeaponCatalog.WeaponFamily.Dot: return new Color(0.42f, 0.88f, 0.5f);
                default: return new Color(0.4f, 0.78f, 1f);
            }
        }

        void AddIcon(Transform grid, string mark, string title, int level, Color color, bool owned)
        {
            if (!owned || grid == null) return;
            var go = new GameObject(title, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(grid, false);
            var image = go.GetComponent<Image>();
            image.sprite = SpriteFactory.Solid(Color.white, 16);
            image.color = new Color(color.r * 0.16f, color.g * 0.2f, color.b * 0.24f, 0.96f);

            var rim = new GameObject("Rim", typeof(RectTransform), typeof(Image));
            rim.transform.SetParent(go.transform, false);
            var rr = (RectTransform)rim.transform;
            rr.anchorMin = Vector2.zero;
            rr.anchorMax = Vector2.one;
            rr.offsetMin = new Vector2(1f, 1f);
            rr.offsetMax = new Vector2(-1f, -1f);
            var ri = rim.GetComponent<Image>();
            ri.sprite = UiStyle.Pixel;
            ri.color = new Color(color.r, color.g, color.b, 0.28f);
            ri.raycastTarget = false;

            var body = level > 0 ? $"{mark}\nLv.{level}\n{title}" : $"{mark}\n{title}";
            var label = MakeLabel(go.transform, "M", body, Vector2.zero, 13, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f));
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(4f, 4f);
            label.rectTransform.offsetMax = new Vector2(-4f, -4f);
            label.color = UiStyle.Text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            _icons.Add(go);
        }

        void ClearIcons()
        {
            for (var i = 0; i < _icons.Count; i++)
                if (_icons[i] != null) Destroy(_icons[i]);
            _icons.Clear();
        }

        static Transform MakeGrid(Transform parent, string name, Vector2 anchored, Vector2 size, int columns)
        {
            var viewGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewGo.transform.SetParent(parent, false);
            var view = (RectTransform)viewGo.transform;
            view.anchorMin = new Vector2(1f, 1f);
            view.anchorMax = new Vector2(1f, 1f);
            view.pivot = new Vector2(1f, 1f);
            view.anchoredPosition = anchored;
            view.sizeDelta = size;
            var bg = viewGo.GetComponent<Image>();
            bg.sprite = UiStyle.Pixel;
            bg.color = new Color(0.03f, 0.07f, 0.09f, 0.92f);
            bg.raycastTarget = true;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewGo.transform, false);
            var content = (RectTransform)contentGo.transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, size.y);

            var grid = contentGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(100f, 100f);
            grid.spacing = new Vector2(8f, 8f);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            var fit = contentGo.GetComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewGo.GetComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = view;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            return content;
        }

        static Transform MakeList(Transform parent, string name, Vector2 anchored, Vector2 size)
        {
            var viewGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            viewGo.transform.SetParent(parent, false);
            var view = (RectTransform)viewGo.transform;
            view.anchorMin = new Vector2(0.5f, 1f);
            view.anchorMax = new Vector2(0.5f, 1f);
            view.pivot = new Vector2(0.5f, 1f);
            view.anchoredPosition = anchored;
            view.sizeDelta = size;
            var bg = viewGo.GetComponent<Image>();
            bg.sprite = UiStyle.Pixel;
            bg.color = new Color(0.025f, 0.05f, 0.07f, 0.5f);
            bg.raycastTarget = true;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewGo.transform, false);
            var content = (RectTransform)contentGo.transform;
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.sizeDelta = new Vector2(0f, 0f);
            var v = contentGo.GetComponent<VerticalLayoutGroup>();
            v.spacing = 8f;
            v.padding = new RectOffset(8, 8, 8, 8);
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewGo.GetComponent<ScrollRect>();
            scroll.content = content;
            scroll.viewport = view;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 32f;
            return content;
        }

        static Transform NewImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = UiStyle.Pixel;
            image.color = color;
            image.raycastTarget = true;
            return go.transform;
        }

        static void Stretch(Transform t)
        {
            var rect = (RectTransform)t;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static Text MakeLabel(Transform parent, string name, string text, Vector2 anchored, int size, TextAnchor align, Vector2 anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(420f, 36f);
            var label = go.GetComponent<Text>();
            label.font = GameFonts.UI;
            label.fontSize = size;
            label.alignment = align;
            label.color = UiStyle.Text;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }
    }
}
