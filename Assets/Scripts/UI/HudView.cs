using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Veinfire
{
    public sealed class HudView : MonoBehaviour
    {
        Text _hpText;
        Text _xpText;
        Text _timer;
        Text _status;
        Text _banner;
        Image _hpFill;
        Image _xpFill;
        CharacterSheet _sheet;
        LevelDirector _levels;
        bool _bound;
        Image _bossFill;
        Text _bossText;
        GameObject _bossRoot;
        Image _dashFill;
        Image _dashDim;
        Text _dashMark;
        PlayerDash _dash;
        Transform _choiceRoot;
        Button _restartButton;
        Button _menuButton;
        Button _rerollButton;
        readonly List<Button> _buttons = new List<Button>();
        readonly List<UpgradeOption> _offered = new List<UpgradeOption>();
        readonly List<Button> _replaceButtons = new List<Button>();
        readonly List<WeaponId> _replaceTargets = new List<WeaponId>();
        Transform _replaceRoot;
        Text _replaceTitle;
        Button _replaceCancel;
        GameObject _confirmRoot;
        Text _confirmText;
        Button _confirmYes;
        Button _confirmNo;
        UpgradeOption _replaceOption;
        WeaponId _replaceDrop;
        string _choiceBanner;
        float _eventUntil;
        bool _lockBanner;

        public static HudView Create(Transform canvas)
        {
            var canvasComp = canvas.GetComponent<Canvas>();
            if (canvasComp != null) canvasComp.sortingOrder = 200;
            var hud = canvas.gameObject.AddComponent<HudView>();
            hud.Build(canvas);
            return hud;
        }

        void Build(Transform canvas)
        {
            UiStyle.Corners(canvas);
            MakeBar(canvas, "HpBar", new Vector2(24f, -24f), new Vector2(420f, 28f), new Color(0.04f, 0.08f, 0.1f, 0.9f), UiStyle.Hp, out _hpFill);
            _hpText = MakeLabel(canvas, "HP", new Vector2(36f, -24f), TextAnchor.UpperLeft, 16);
            _hpText.color = UiStyle.Text;
            _hpText.rectTransform.sizeDelta = new Vector2(400f, 28f);

            MakeBar(canvas, "XpBar", new Vector2(0f, 36f), new Vector2(640f, 18f), new Color(0.04f, 0.08f, 0.12f, 0.9f), UiStyle.Xp, out _xpFill);
            var xpBar = canvas.Find("XpBar") as RectTransform;
            if (xpBar != null)
            {
                xpBar.anchorMin = new Vector2(0.5f, 0f);
                xpBar.anchorMax = new Vector2(0.5f, 0f);
                xpBar.pivot = new Vector2(0.5f, 0f);
                xpBar.anchoredPosition = new Vector2(0f, 28f);
            }

            _xpText = MakeLabel(canvas, "XP", new Vector2(0f, 54f), TextAnchor.LowerCenter, 16);
            _xpText.color = UiStyle.Muted;
            _xpText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            _xpText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _xpText.rectTransform.pivot = new Vector2(0.5f, 0f);
            _xpText.rectTransform.anchoredPosition = new Vector2(0f, 54f);
            _xpText.rectTransform.sizeDelta = new Vector2(640f, 28f);

            _status = MakeLabel(canvas, "Status", new Vector2(24, -64), TextAnchor.UpperLeft, 16);
            _status.color = UiStyle.Muted;
            _status.rectTransform.sizeDelta = new Vector2(820f, 70f);
            var statusHover = _status.gameObject.AddComponent<UiHover>();
            statusHover.TipTitle = "本局状态";
            statusHover.TipBody = "Tab 打开作战档案。属性 / 组合 / 战况 / 修饰可在面板内切换。";
            var hudCanvas = canvas.GetComponent<Canvas>() ?? canvas.GetComponentInParent<Canvas>();
            if (hudCanvas != null) UiTip.Bind(hudCanvas);
            BuildDashIcon(canvas);
            _timer = MakeLabel(canvas, "Time", new Vector2(-24, -20), TextAnchor.UpperRight, 22);
            _timer.color = UiStyle.Accent;
            _banner = MakeLabel(canvas, "Banner", Vector2.zero, TextAnchor.MiddleCenter, 28);
            _banner.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _banner.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _banner.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _banner.rectTransform.anchoredPosition = new Vector2(0f, 140f);
            _banner.rectTransform.sizeDelta = new Vector2(900f, 120f);
            _banner.gameObject.SetActive(false);

            _choiceRoot = new GameObject("Choices", typeof(RectTransform), typeof(Image)).transform;
            _choiceRoot.SetParent(canvas, false);
            var root = (RectTransform)_choiceRoot;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            var dim = _choiceRoot.GetComponent<Image>();
            dim.sprite = UiStyle.Pixel;
            dim.color = new Color(0.02f, 0.05f, 0.07f, 0.82f);
            dim.raycastTarget = true;
            _choiceRoot.gameObject.SetActive(false);

            for (var i = 0; i < 6; i++)
                _buttons.Add(MakeChoiceButton(root, i));
            _rerollButton = MakeRestartButton(_choiceRoot, "刷新  R", new Vector2(0f, -280f), 240f);
            _rerollButton.onClick.AddListener(RerollChoices);
            _rerollButton.gameObject.SetActive(false);
            BuildReplacePanel(canvas);

            _restartButton = MakeRestartButton(canvas, "重新开始", new Vector2(0f, 20f), 280f);
            _restartButton.gameObject.SetActive(false);
            _menuButton = MakeRestartButton(canvas, "返回标题", new Vector2(0f, -56f), 280f);
            _menuButton.gameObject.SetActive(false);
            BuildBossBar(canvas);
            _sheet = CharacterSheet.Create(canvas);
        }

        void BuildDashIcon(Transform canvas)
        {
            var frame = UiStyle.Frame(canvas, "DashCharge", new Vector2(88f, 88f), Vector2.zero, UiStyle.Panel);
            var rect = (RectTransform)frame.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(70f, 92f);

            _dashDim = frame.transform.Find("Inner").GetComponent<Image>();
            _dashDim.color = new Color(0.05f, 0.12f, 0.16f, 0.95f);

            var fillGo = new GameObject("Charge", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(frame.transform, false);
            var fillRect = (RectTransform)fillGo.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(6f, 6f);
            fillRect.offsetMax = new Vector2(-6f, -6f);
            _dashFill = fillGo.GetComponent<Image>();
            _dashFill.sprite = UiStyle.Pixel;
            _dashFill.color = UiStyle.Accent;
            _dashFill.type = Image.Type.Filled;
            _dashFill.fillMethod = Image.FillMethod.Vertical;
            _dashFill.fillOrigin = (int)Image.OriginVertical.Bottom;
            _dashFill.fillAmount = 1f;
            _dashFill.raycastTarget = false;

            _dashMark = UiStyle.Label(frame.transform, "Mark", "冲", 28, UiStyle.Text, TextAnchor.MiddleCenter);
            var mr = _dashMark.rectTransform;
            mr.anchorMin = Vector2.zero;
            mr.anchorMax = Vector2.one;
            mr.offsetMin = new Vector2(4f, 18f);
            mr.offsetMax = new Vector2(-4f, -4f);

            var hint = UiStyle.Label(frame.transform, "Hint", "SHIFT", 12, UiStyle.Muted, TextAnchor.LowerCenter);
            var hr = hint.rectTransform;
            hr.anchorMin = Vector2.zero;
            hr.anchorMax = Vector2.one;
            hr.offsetMin = new Vector2(4f, 6f);
            hr.offsetMax = new Vector2(-4f, -4f);
        }

        void TickDash()
        {
            if (_dashFill == null) return;
            if (_dash == null && _levels != null) _dash = _levels.Dash;
            var charge = _dash != null ? _dash.Charge01 : 1f;
            _dashFill.fillAmount = charge;
            var ready = charge >= 0.999f;
            _dashFill.color = ready ? UiStyle.Accent : new Color(0.18f, 0.55f, 0.65f, 0.95f);
            if (_dashDim != null)
                _dashDim.color = ready ? new Color(0.08f, 0.18f, 0.22f, 0.95f) : new Color(0.03f, 0.07f, 0.09f, 0.95f);
            if (_dashMark != null)
                _dashMark.color = ready ? UiStyle.Text : UiStyle.Muted;
        }

        void BuildBossBar(Transform canvas)
        {
            MakeBar(canvas, "BossBar", new Vector2(0f, -18f), new Vector2(760f, 22f), new Color(0.06f, 0.04f, 0.08f, 0.92f), UiStyle.Boss, out _bossFill);
            var bar = canvas.Find("BossBar") as RectTransform;
            if (bar != null)
            {
                bar.anchorMin = new Vector2(0.5f, 1f);
                bar.anchorMax = new Vector2(0.5f, 1f);
                bar.pivot = new Vector2(0.5f, 1f);
                bar.anchoredPosition = new Vector2(0f, -18f);
                _bossRoot = bar.gameObject;
            }

            _bossText = MakeLabel(canvas, "BossHp", new Vector2(0f, -18f), TextAnchor.UpperCenter, 18);
            _bossText.color = UiStyle.Text;
            _bossText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            _bossText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            _bossText.rectTransform.pivot = new Vector2(0.5f, 1f);
            _bossText.rectTransform.anchoredPosition = new Vector2(0f, -18f);
            _bossText.rectTransform.sizeDelta = new Vector2(760f, 30f);
            if (_bossRoot != null) _bossRoot.SetActive(false);
            _bossText.gameObject.SetActive(false);
        }

        void Update()
        {
            TickDash();
            TickBoss();
            TickChoiceKeys();
            if (_lockBanner) return;
            if (_eventUntil > 0f && Time.unscaledTime > _eventUntil && !_choiceRoot.gameObject.activeSelf)
            {
                _eventUntil = 0f;
                _banner.gameObject.SetActive(false);
            }
        }

        void TickChoiceKeys()
        {
            if (_replaceRoot != null && _replaceRoot.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (_confirmRoot != null && _confirmRoot.activeSelf) HideConfirm();
                    else CloseReplace(false);
                }
                return;
            }
            if (_choiceRoot == null || !_choiceRoot.gameObject.activeSelf) return;
            if (_sheet != null && _sheet.gameObject.activeSelf) return;
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) PickIndex(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) PickIndex(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) PickIndex(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) PickIndex(3);
            else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) PickIndex(4);
            else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) PickIndex(5);
            else if (Input.GetKeyDown(KeyCode.R)) RerollChoices();
            else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                for (var i = 0; i < _buttons.Count; i++)
                {
                    if (_buttons[i] == null || !_buttons[i].gameObject.activeSelf) continue;
                    var rect = (RectTransform)_buttons[i].transform;
                    if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
                    {
                        PickIndex(i);
                        break;
                    }
                }
            }
        }

        public void Bind(PlayerHealth health, LevelDirector levels)
        {
            if (_bound)
            {
                _levels = levels;
                _dash = levels != null ? levels.Dash : null;
                return;
            }

            _bound = true;
            _levels = levels;
            _dash = levels != null ? levels.Dash : null;
            health.Changed += SetHp;
            SetHp(health.Current, health.Max);
            levels.XpChanged += SetXp;
            SetXp(levels.Level, levels.Xp, levels.XpToNext);
            levels.LevelUpOffered += ShowChoices;
        }

        public void ShowSheet(bool on, int tab = -1)
        {
            _sheet?.Show(on, _levels);
            if (on && tab >= 0) _sheet?.OpenTab(tab);
            if (on && _sheet != null)
                _sheet.transform.SetAsLastSibling();
        }

        void TickBoss()
        {
            EnemyHealth boss = null;
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null && enemies[i].Kind == EnemyKind.Boss && enemies[i].gameObject.activeInHierarchy)
                {
                    boss = enemies[i];
                    break;
                }
            }

            var show = boss != null;
            if (_bossRoot != null) _bossRoot.SetActive(show);
            if (_bossText != null) _bossText.gameObject.SetActive(show);
            if (!show) return;
            var max = Mathf.Max(1f, boss.MaxHp);
            if (_bossFill != null) _bossFill.fillAmount = Mathf.Clamp01(boss.Current / max);
            if (_bossText != null)
                _bossText.text = $"BOSS    {Mathf.CeilToInt(boss.Current)} / {Mathf.CeilToInt(max)}";
        }

        void SetHp(float hp, float max)
        {
            if (_hpFill != null) _hpFill.fillAmount = max <= 0f ? 0f : Mathf.Clamp01(hp / max);
            if (_hpText != null) _hpText.text = $"生命  {Mathf.FloorToInt(hp)} / {Mathf.CeilToInt(max)}";
        }

        void SetXp(int level, int xp, int need)
        {
            if (_xpFill != null) _xpFill.fillAmount = need <= 0 ? 0f : Mathf.Clamp01(xp / (float)need);
            if (_xpText != null) _xpText.text = $"等级 {level}    经验 {xp} / {need}";
        }

        public void SetTimer(float seconds)
        {
            var m = Mathf.FloorToInt(seconds / 60f);
            var s = Mathf.FloorToInt(seconds % 60f);
            var mode = RunConfig.ModeTitle(RunConfig.Mode);
            if (!RunRules.TimeVictory)
            {
                _timer.text = $"{m:00}:{s:00}\n{mode}";
                return;
            }

            var remain = Mathf.Max(0f, RunRules.RunSeconds - seconds);
            var rm = Mathf.FloorToInt(remain / 60f);
            var rs = Mathf.FloorToInt(remain % 60f);
            var goalM = Mathf.FloorToInt(RunRules.RunSeconds / 60f);
            _timer.text = $"{m:00}:{s:00}\n{mode}  目标 {goalM:00}:00  (剩余 {rm:00}:{rs:00})";
        }

        public void SetStatus(string text)
        {
            var hover = _status != null ? _status.GetComponent<UiHover>() : null;
            if (hover != null) hover.TipBody = string.IsNullOrEmpty(text) ? "Tab 打开作战档案。" : text;
            if (_status == null) return;
            if (!string.IsNullOrEmpty(text))
            {
                var lines = text.Split('\n');
                if (lines.Length > 2)
                {
                    var bits = lines[2].Split(new[] { "  " }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (bits.Length > 8)
                    {
                        var keep = new System.Text.StringBuilder();
                        for (var i = 0; i < 7; i++)
                        {
                            if (i > 0) keep.Append("  ");
                            keep.Append(bits[i]);
                        }
                        keep.Append("  +").Append(bits.Length - 7);
                        lines[2] = keep.ToString();
                        text = string.Join("\n", lines);
                    }
                }
            }
            _status.text = text;
        }

        public void ShowEvent(string text)
        {
            if (_lockBanner) return;
            _banner.gameObject.SetActive(true);
            _banner.text = text;
            _eventUntil = Time.unscaledTime + 2.5f;
        }

        public void ShowGameOver(float seconds, int kills)
        {
            ShowEnd($"你倒下了\n存活 {Format(seconds)}    击杀 {kills}{EmberLine()}\n按 R 重开，或返回标题");
        }

        public void ShowWin(float seconds, int kills)
        {
            ShowEnd($"撑过了夜火\n用时 {Format(seconds)}    击杀 {kills}{EmberLine()}\n按 R 再来一局，或返回标题");
        }

        static string EmberLine()
        {
            return $"\n残烬 +{RunConfig.LastEmbers}    库存 {MetaProgress.Data.embers}";
        }

        void ShowEnd(string text)
        {
            _lockBanner = true;
            _choiceRoot.gameObject.SetActive(false);
            _banner.gameObject.SetActive(true);
            _banner.text = text;
            if (_restartButton != null)
            {
                _restartButton.gameObject.SetActive(true);
                _restartButton.onClick.RemoveAllListeners();
                _restartButton.onClick.AddListener(GameSession.Restart);
            }

            if (_menuButton != null)
            {
                _menuButton.gameObject.SetActive(true);
                _menuButton.onClick.RemoveAllListeners();
                _menuButton.onClick.AddListener(() =>
                {
                    GameInstaller.WipeRunObjects();
                    GameSession.ClearRunFlags();
                    MainMenu.ShowTitle();
                });
            }
        }

        static string Format(float seconds)
        {
            var m = Mathf.FloorToInt(seconds / 60f);
            var s = Mathf.FloorToInt(seconds % 60f);
            return $"{m:00}:{s:00}";
        }

        void ShowChoices(int level, List<UpgradeOption> options)
        {
            CloseReplace(true);
            _lockBanner = false;
            _sheet?.Show(false, _levels);
            if (_restartButton != null) _restartButton.gameObject.SetActive(false);
            if (_menuButton != null) _menuButton.gameObject.SetActive(false);
            _banner.gameObject.SetActive(true);
            var unique = LevelDirector.IsUniqueLevel(level);
            var left = _levels != null ? _levels.Rerolls : 0;
            _banner.text = unique
                ? $"SYSTEM  //  等级 {level}    专武强化    R 刷新剩余次数：{left}    Tab 档案"
                : $"SYSTEM  //  等级 {level}    六选一    R 刷新剩余次数：{left}    1-6 / Tab 档案";
            _banner.color = UiStyle.Accent;
            _choiceBanner = _banner.text;
            _banner.rectTransform.anchoredPosition = new Vector2(0f, 280f);
            _choiceRoot.gameObject.SetActive(true);
            _choiceRoot.SetAsLastSibling();
            _banner.rectTransform.SetAsLastSibling();
            _offered.Clear();
            if (options != null) _offered.AddRange(options);
            for (var i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                var active = i < _offered.Count;
                button.gameObject.SetActive(active);
                if (!active) continue;
                var index = i;
                var option = _offered[i];
                PaintChoice(button, option, i);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => PickIndex(index));
            }

            if (_rerollButton != null)
            {
                _rerollButton.gameObject.SetActive(true);
                _rerollButton.interactable = left > 0;
                var label = _rerollButton.GetComponentInChildren<Text>();
                if (label != null) label.text = left > 0 ? $"刷新  R  ×{left}" : "刷新耗尽";
            }
        }

        void RerollChoices()
        {
            if (_levels == null || _levels.Rerolls <= 0) return;
            _levels.Reroll();
        }

        void PickIndex(int index)
        {
            if (index < 0 || index >= _offered.Count) return;
            var option = _offered[index];
            if (option != null && option.ReplaceHint)
            {
                OpenReplace(option);
                return;
            }

            CommitChoice(option);
        }

        void CommitChoice(UpgradeOption option)
        {
            CloseReplace(true);
            _choiceRoot.gameObject.SetActive(false);
            _banner.gameObject.SetActive(false);
            if (_rerollButton != null) _rerollButton.gameObject.SetActive(false);
            _offered.Clear();
            if (_levels != null) _levels.Choose(option);
            else FindFirstObjectByType<LevelDirector>()?.Choose(option);
        }

        static void MakeBar(Transform parent, string name, Vector2 anchored, Vector2 size, Color back, Color fill, out Image fillImage)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchored;
            rect.sizeDelta = size;
            var sprite = UiStyle.Pixel;
            var bg = go.GetComponent<Image>();
            bg.sprite = sprite;
            bg.color = back;
            bg.raycastTarget = false;

            var edge = new GameObject("Edge", typeof(RectTransform), typeof(Image));
            edge.transform.SetParent(go.transform, false);
            var er = (RectTransform)edge.transform;
            er.anchorMin = new Vector2(0f, 0f);
            er.anchorMax = new Vector2(0f, 1f);
            er.pivot = new Vector2(0f, 0.5f);
            er.sizeDelta = new Vector2(2f, 0f);
            er.anchoredPosition = Vector2.zero;
            var ei = edge.GetComponent<Image>();
            ei.sprite = sprite;
            ei.color = UiStyle.Accent;
            ei.raycastTarget = false;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(go.transform, false);
            var fillRect = (RectTransform)fillGo.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillImage = fillGo.GetComponent<Image>();
            fillImage.sprite = sprite;
            fillImage.color = fill;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 1f;
            fillImage.raycastTarget = false;
        }

        static Button MakeRestartButton(Transform canvas, string text, Vector2 pos, float width)
        {
            var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 64f);
            rect.anchoredPosition = pos;
            go.GetComponent<Image>().sprite = UiStyle.Pixel;
            go.GetComponent<Image>().color = UiStyle.PanelHi;
            go.GetComponent<Image>().raycastTarget = true;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = (RectTransform)labelGo.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<Text>();
            label.font = GameFonts.UI;
            label.fontSize = 24;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = UiStyle.Text;
            label.text = text;
            label.raycastTarget = false;
            return go.GetComponent<Button>();
        }

        static Text MakeLabel(Transform parent, string name, Vector2 anchored, TextAnchor align, int size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = align == TextAnchor.UpperRight ? new Vector2(1f, 1f) : align == TextAnchor.MiddleCenter ? new Vector2(0.5f, 0.5f) : align == TextAnchor.LowerCenter ? new Vector2(0.5f, 0f) : new Vector2(0f, 1f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = rect.anchorMin;
            rect.anchoredPosition = anchored;
            rect.sizeDelta = new Vector2(560f, 40f);
            var text = go.GetComponent<Text>();
            text.font = GameFonts.UI;
            text.fontSize = size;
            text.alignment = align;
            text.color = UiStyle.Text;
            text.raycastTarget = false;
            return text;
        }

        static void PaintChoice(Button button, UpgradeOption option, int index)
        {
            var label = button.transform.Find("Label")?.GetComponent<Text>();
            var badge = button.transform.Find("Badge")?.GetComponent<Text>();
            var inner = button.transform.Find("Inner")?.GetComponent<Image>();
            var frame = button.GetComponent<Image>();
            var edge = button.transform.Find("Edge")?.GetComponent<Image>();
            var owned = option.OwnedHint;
            var fusion = option.FusionHint;
            if (label != null)
            {
                label.text = $"{index + 1}    {option.Title}\n{option.Description}";
                label.color = UiStyle.Text;
            }
            if (badge != null)
            {
                if (option.ReplaceHint) { badge.text = "替换"; badge.color = new Color(1f, 0.55f, 0.4f); }
                else if (owned && fusion) { badge.text = "已装备  ·  组合"; badge.color = new Color(1f, 0.86f, 0.45f); }
                else if (fusion) { badge.text = "组合"; badge.color = new Color(1f, 0.82f, 0.38f); }
                else if (owned) { badge.text = "已装备"; badge.color = new Color(0.45f, 0.92f, 0.95f); }
                else { badge.text = option.Category; badge.color = UiStyle.Muted; }
            }
            if (option.ReplaceHint)
            {
                if (frame != null) frame.color = new Color(0.72f, 0.28f, 0.22f);
                if (inner != null) inner.color = new Color(0.16f, 0.06f, 0.06f, 0.98f);
                if (edge != null) edge.color = new Color(1f, 0.45f, 0.35f);
            }
            else if (owned && fusion)
            {
                if (frame != null) frame.color = new Color(0.72f, 0.58f, 0.2f);
                if (inner != null) inner.color = new Color(0.05f, 0.15f, 0.17f, 0.98f);
                if (edge != null) edge.color = new Color(1f, 0.82f, 0.38f);
            }
            else if (fusion)
            {
                if (frame != null) frame.color = new Color(0.7f, 0.55f, 0.16f);
                if (inner != null) inner.color = new Color(0.2f, 0.15f, 0.05f, 0.98f);
                if (edge != null) edge.color = new Color(1f, 0.82f, 0.38f);
            }
            else if (owned)
            {
                if (frame != null) frame.color = new Color(0.18f, 0.62f, 0.68f);
                if (inner != null) inner.color = new Color(0.04f, 0.15f, 0.17f, 0.98f);
                if (edge != null) edge.color = new Color(0.4f, 0.92f, 0.95f);
            }
            else
            {
                if (frame != null) frame.color = UiStyle.AccentDim;
                if (inner != null) inner.color = option.IsWeapon ? new Color(0.07f, 0.11f, 0.14f, 0.98f) : UiStyle.Panel;
                if (edge != null) edge.color = UiStyle.Accent;
            }

            var hover = button.GetComponent<UiHover>() ?? button.gameObject.AddComponent<UiHover>();
            hover.Edge = frame != null ? frame : edge;
            hover.Inner = inner;
            hover.TipTitle = option.Title;
            hover.TipBody = option.Description;
        }

        static Button MakeChoiceButton(RectTransform parent, int index)
        {
            var col = index % 3;
            var row = index / 3;
            var frame = UiStyle.Frame(parent, $"Choice{index}", new Vector2(340f, 228f), new Vector2((col - 1) * 358f, 86f - row * 246f), UiStyle.Panel);
            var button = frame.AddComponent<Button>();
            var inner = frame.transform.Find("Inner").GetComponent<Image>();
            button.targetGraphic = inner;
            button.transition = Selectable.Transition.ColorTint;
            button.interactable = true;
            var badge = UiStyle.Label(frame.transform, "Badge", "", 14, UiStyle.Muted, TextAnchor.UpperLeft);
            var br = badge.rectTransform;
            br.anchorMin = new Vector2(0f, 1f);
            br.anchorMax = new Vector2(1f, 1f);
            br.pivot = new Vector2(0f, 1f);
            br.anchoredPosition = new Vector2(16f, -10f);
            br.sizeDelta = new Vector2(-32f, 24f);
            var label = UiStyle.Label(frame.transform, "Label", "", 16, UiStyle.Text, TextAnchor.UpperLeft);
            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(16f, 14f);
            labelRect.offsetMax = new Vector2(-16f, -36f);
            return button;
        }

        void BuildReplacePanel(Transform canvas)
        {
            _replaceRoot = new GameObject("Replace", typeof(RectTransform), typeof(Image)).transform;
            _replaceRoot.SetParent(canvas, false);
            var root = (RectTransform)_replaceRoot;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            var dim = _replaceRoot.GetComponent<Image>();
            dim.sprite = UiStyle.Pixel;
            dim.color = new Color(0.02f, 0.04f, 0.06f, 0.88f);
            dim.raycastTarget = true;
            _replaceRoot.gameObject.SetActive(false);

            _replaceTitle = UiStyle.Label(_replaceRoot, "Title", "", 22, UiStyle.Accent, TextAnchor.UpperCenter);
            var tr = _replaceTitle.rectTransform;
            tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -70f);
            tr.sizeDelta = new Vector2(980f, 80f);

            for (var i = 0; i < 7; i++)
            {
                var col = i % 4;
                var row = i / 4;
                var frame = UiStyle.Frame(_replaceRoot, "Drop" + i, new Vector2(260f, 110f), new Vector2((col - 1.5f) * 280f, 40f - row * 126f), UiStyle.Panel);
                var button = frame.AddComponent<Button>();
                button.targetGraphic = frame.transform.Find("Inner").GetComponent<Image>();
                var idx = i;
                button.onClick.AddListener(() => SelectDrop(idx));
                UiStyle.Label(frame.transform, "Label", "", 16, UiStyle.Text, TextAnchor.MiddleCenter);
                _replaceButtons.Add(button);
            }

            _replaceCancel = MakeRestartButton(_replaceRoot, "取消替换  ESC", new Vector2(0f, -280f), 280f);
            _replaceCancel.onClick.AddListener(() => CloseReplace(false));

            _confirmRoot = new GameObject("Confirm", typeof(RectTransform), typeof(Image));
            _confirmRoot.transform.SetParent(_replaceRoot, false);
            var cr = (RectTransform)_confirmRoot.transform;
            cr.anchorMin = cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(640f, 280f);
            cr.anchoredPosition = Vector2.zero;
            var panel = _confirmRoot.GetComponent<Image>();
            panel.sprite = UiStyle.Pixel;
            panel.color = new Color(0.05f, 0.09f, 0.11f, 0.98f);
            UiStyle.Corners(_confirmRoot.transform);

            _confirmText = UiStyle.Label(_confirmRoot.transform, "Body", "", 18, UiStyle.Text, TextAnchor.UpperCenter);
            var ctext = _confirmText.rectTransform;
            ctext.anchorMin = new Vector2(0f, 0f);
            ctext.anchorMax = new Vector2(1f, 1f);
            ctext.offsetMin = new Vector2(28f, 90f);
            ctext.offsetMax = new Vector2(-28f, -24f);

            _confirmYes = MakeRestartButton(_confirmRoot.transform, "确认替换", new Vector2(-120f, -90f), 220f);
            _confirmYes.onClick.AddListener(ConfirmReplace);
            _confirmNo = MakeRestartButton(_confirmRoot.transform, "返回", new Vector2(120f, -90f), 180f);
            _confirmNo.onClick.AddListener(HideConfirm);
            _confirmRoot.SetActive(false);
        }

        void OpenReplace(UpgradeOption option)
        {
            _replaceOption = option;
            HideConfirm();
            var loadout = _levels != null ? _levels.Loadout : null;
            if (loadout == null)
            {
                CloseReplace(false);
                return;
            }

            loadout.CollectCommons(_replaceTargets);
            var left = RunConfig.ReplacesLeft;
            if (_replaceTitle != null)
                _replaceTitle.text =
                    $"用「{WeaponCatalog.Title(option.SourceWeapon)}」覆盖哪一件？\n新武器从 Lv.1 开始。本局剩余替换 {left}/{RunConfig.MaxReplaces}。专武不可替换。";
            for (var i = 0; i < _replaceButtons.Count; i++)
            {
                var on = i < _replaceTargets.Count;
                _replaceButtons[i].gameObject.SetActive(on);
                if (!on) continue;
                var id = _replaceTargets[i];
                var label = _replaceButtons[i].GetComponentInChildren<Text>();
                var fused = FusionCatalog.WeaponBound(id);
                if (label != null)
                    label.text = fused
                        ? $"{WeaponCatalog.Title(id)}  Lv.{loadout.LevelOf(id)}\n将解除已生效的组合技能"
                        : $"{WeaponCatalog.Title(id)}  Lv.{loadout.LevelOf(id)}";
            }

            _replaceRoot.gameObject.SetActive(true);
            _replaceRoot.SetAsLastSibling();
            if (_banner != null)
            {
                _banner.text = $"SYSTEM  //  选择要覆盖的普通武器    剩余替换 {RunConfig.ReplacesLeft}/{RunConfig.MaxReplaces}    ESC 取消";
                _banner.gameObject.SetActive(true);
                _banner.rectTransform.SetAsLastSibling();
            }
        }

        void SelectDrop(int index)
        {
            if (index < 0 || index >= _replaceTargets.Count) return;
            _replaceDrop = _replaceTargets[index];
            var loadout = _levels != null ? _levels.Loadout : null;
            var incoming = _replaceOption != null ? _replaceOption.SourceWeapon : WeaponId.Orbit;
            var extra = FusionCatalog.WeaponBound(_replaceDrop) ? "\n该武器已绑定组合技能，替换后组合会解除。" : "";
            var last = RunConfig.ReplacesLeft <= 1 ? "\n这是本局最后一次替换。" : $"\n确认后剩余 {Mathf.Max(0, RunConfig.ReplacesLeft - 1)}/{RunConfig.MaxReplaces} 次。";
            if (_confirmText != null)
                _confirmText.text =
                    $"确定将「{WeaponCatalog.Title(_replaceDrop)} Lv.{(loadout != null ? loadout.LevelOf(_replaceDrop) : 1)}」\n替换为「{WeaponCatalog.Title(incoming)} Lv.1」？\n覆盖后无法撤销。{extra}{last}";
            if (_confirmRoot != null) _confirmRoot.SetActive(true);
        }

        void HideConfirm()
        {
            if (_confirmRoot != null) _confirmRoot.SetActive(false);
        }

        void CloseReplace(bool silent)
        {
            HideConfirm();
            _replaceOption = null;
            if (_replaceRoot != null) _replaceRoot.gameObject.SetActive(false);
            if (!silent && _choiceRoot != null && _offered.Count > 0)
            {
                _choiceRoot.gameObject.SetActive(true);
                if (_banner != null)
                {
                    _banner.gameObject.SetActive(true);
                    if (!string.IsNullOrEmpty(_choiceBanner)) _banner.text = _choiceBanner;
                }
            }
        }

        void ConfirmReplace()
        {
            if (_replaceOption == null || _levels == null || _levels.Loadout == null) return;
            var incoming = _replaceOption.SourceWeapon;
            if (!_levels.Loadout.Replace(_replaceDrop, incoming, _levels.Passives)) return;
            var done = _replaceOption;
            CloseReplace(true);
            CommitChoice(done);
        }
    }
}
