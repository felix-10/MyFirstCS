using UnityEngine;
using UnityEngine.UI;

namespace Veinfire
{
    public enum HitKind
    {
        Normal,
        Crit,
        Burn,
        Wet,
        Petrify,
        Heal,
        Corrode,
        Shock,
        Ice,
        Plague,
        Holy,
        Chaos
    }

    public sealed class DamagePopup : MonoBehaviour
    {
        static Canvas _canvas;
        Text _text;
        float _life;
        float _maxLife;
        Vector3 _world;
        Vector3 _drift;

        public static void Spawn(Vector3 position, float amount, HitKind kind = HitKind.Normal)
        {
            var n = Mathf.Max(1, Mathf.RoundToInt(amount));
            string mark;
            Color color;
            var size = 34;
            switch (kind)
            {
                case HitKind.Burn:
                    mark = $"灼 {n}";
                    color = new Color(1f, 0.45f, 0.12f);
                    size = 30;
                    break;
                case HitKind.Wet:
                    mark = $"湿 {n}";
                    color = new Color(0.4f, 0.8f, 1f);
                    size = 30;
                    break;
                case HitKind.Corrode:
                    mark = $"蚀 {n}";
                    color = new Color(0.72f, 0.35f, 1f);
                    size = 30;
                    break;
                case HitKind.Shock:
                    mark = $"雷 {n}";
                    color = new Color(0.75f, 0.65f, 1f);
                    size = 30;
                    break;
                case HitKind.Ice:
                    mark = $"冰 {n}";
                    color = new Color(0.65f, 0.95f, 1f);
                    size = 30;
                    break;
                case HitKind.Plague:
                    mark = $"疫 {n}";
                    color = new Color(0.35f, 0.9f, 0.4f);
                    size = 30;
                    break;
                case HitKind.Holy:
                    mark = $"圣 {n}";
                    color = new Color(1f, 0.88f, 0.35f);
                    size = 30;
                    break;
                case HitKind.Chaos:
                    mark = $"混 {n}";
                    color = new Color(0.85f, 0.2f, 0.28f);
                    size = 30;
                    break;
                case HitKind.Petrify:
                    mark = $"岩 {n}";
                    color = new Color(0.9f, 0.82f, 0.5f);
                    size = 40;
                    break;
                case HitKind.Crit:
                    mark = $"暴 {n}";
                    color = new Color(0.4f, 0.98f, 1f);
                    size = 42;
                    break;
                default:
                    mark = n.ToString();
                    color = new Color(1f, 0.93f, 0.55f);
                    break;
            }

            Make(position, mark, color, size, 0.85f);
        }

        public static void SpawnTag(Vector3 position, string tag, HitKind kind)
        {
            var color = kind == HitKind.Burn
                ? new Color(1f, 0.5f, 0.15f)
                : kind == HitKind.Wet
                    ? new Color(0.45f, 0.85f, 1f)
                    : kind == HitKind.Heal
                        ? new Color(0.35f, 1f, 0.5f)
                        : new Color(0.86f, 0.78f, 0.5f);
            Make(position + Vector3.up * 0.4f, tag, color, 26, 1f);
        }

        static void Make(Vector3 world, string mark, Color color, int size, float life)
        {
            EnsureCanvas();
            var go = new GameObject("Dmg", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(DamagePopup));
            go.transform.SetParent(_canvas.transform, false);
            var popup = go.GetComponent<DamagePopup>();
            popup._text = go.GetComponent<Text>();
            popup._text.font = GameFonts.UI;
            popup._text.fontSize = size;
            popup._text.alignment = TextAnchor.MiddleCenter;
            popup._text.horizontalOverflow = HorizontalWrapMode.Overflow;
            popup._text.verticalOverflow = VerticalWrapMode.Overflow;
            popup._text.raycastTarget = false;
            popup._text.fontStyle = FontStyle.Bold;
            popup._text.text = mark;
            popup._text.color = color;
            popup._world = world + Vector3.up * 1.35f;
            popup._drift = new Vector3(Random.Range(-28f, 28f), 70f, 0f);
            popup._life = life;
            popup._maxLife = life;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(220f, 64f);
            popup.Place();
        }

        static void EnsureCanvas()
        {
            if (_canvas != null) return;
            var host = GameObject.Find("VeinfireAudio");
            if (host == null)
            {
                AudioHub.Ensure();
                host = GameObject.Find("VeinfireAudio");
            }

            var go = new GameObject("DmgCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            if (host != null)
                go.transform.SetParent(host.transform, false);
            else
                Object.DontDestroyOnLoad(go);
            _canvas = go.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 320;
            var group = go.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        void Update()
        {
            _life -= Time.unscaledDeltaTime;
            _world += Vector3.up * (1.6f * Time.unscaledDeltaTime);
            _drift.y += 40f * Time.unscaledDeltaTime;
            Place();
            if (_text != null)
            {
                var c = _text.color;
                c.a = Mathf.Clamp01(_life / Mathf.Max(0.05f, _maxLife));
                _text.color = c;
            }

            if (_life <= 0f) Destroy(gameObject);
        }

        void Place()
        {
            var cam = Camera.main;
            var rt = transform as RectTransform;
            if (cam == null || rt == null) return;
            var sp = cam.WorldToScreenPoint(_world);
            if (sp.z < 0.2f)
            {
                if (_text != null) _text.enabled = false;
                return;
            }

            if (_text != null) _text.enabled = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                sp + _drift,
                null,
                out var local);
            rt.anchoredPosition = local;
        }
    }
}
