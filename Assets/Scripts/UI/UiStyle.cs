using UnityEngine;
using UnityEngine.UI;

namespace Veinfire
{
    public static class UiStyle
    {
        public static readonly Color Void = new Color(0.03f, 0.06f, 0.08f, 0.97f);
        public static readonly Color Panel = new Color(0.05f, 0.1f, 0.13f, 0.94f);
        public static readonly Color PanelHi = new Color(0.08f, 0.16f, 0.2f, 0.98f);
        public static readonly Color Accent = new Color(0.28f, 0.92f, 1f, 1f);
        public static readonly Color AccentDim = new Color(0.12f, 0.42f, 0.5f, 1f);
        public static readonly Color Danger = new Color(1f, 0.32f, 0.45f, 1f);
        public static readonly Color Text = new Color(0.88f, 0.97f, 1f, 1f);
        public static readonly Color Muted = new Color(0.52f, 0.72f, 0.8f, 1f);
        public static readonly Color Hp = new Color(0.25f, 0.9f, 0.95f, 1f);
        public static readonly Color Xp = new Color(0.45f, 0.55f, 1f, 1f);
        public static readonly Color Boss = new Color(1f, 0.35f, 0.55f, 1f);
        public static readonly Color Locked = new Color(0.06f, 0.07f, 0.08f, 0.96f);
        public static readonly Color Owned = new Color(0.05f, 0.2f, 0.18f, 0.98f);
        public static readonly Color Warn = new Color(1f, 0.8f, 0.36f, 1f);

        public static Sprite Pixel
        {
            get
            {
                if (_pixel == null) _pixel = SpriteFactory.Solid(Color.white, 8);
                return _pixel;
            }
        }

        static Sprite _pixel;

        public static Image Paint(GameObject go, Color color, bool raycast)
        {
            var image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            image.sprite = Pixel;
            image.color = color;
            image.raycastTarget = raycast;
            image.type = Image.Type.Simple;
            return image;
        }

        public static Text Label(Transform parent, string name, string content, int size, Color color, TextAnchor align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = GameFonts.UI;
            text.fontSize = size;
            text.color = color;
            text.alignment = align;
            text.text = content;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static GameObject Frame(Transform parent, string name, Vector2 size, Vector2 pos, Color fill, bool raycast = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            Paint(go, AccentDim, raycast);

            var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            inner.transform.SetParent(go.transform, false);
            var ir = (RectTransform)inner.transform;
            ir.anchorMin = Vector2.zero;
            ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(2f, 2f);
            ir.offsetMax = new Vector2(-2f, -2f);
            Paint(inner, fill, raycast);

            var shine = new GameObject("Edge", typeof(RectTransform), typeof(Image));
            shine.transform.SetParent(go.transform, false);
            var sr = (RectTransform)shine.transform;
            sr.anchorMin = new Vector2(0f, 1f);
            sr.anchorMax = new Vector2(1f, 1f);
            sr.pivot = new Vector2(0.5f, 1f);
            sr.sizeDelta = new Vector2(0f, 2f);
            sr.anchoredPosition = Vector2.zero;
            Paint(shine, Accent, false);
            return go;
        }

        public static void Stretch(Transform t)
        {
            var rect = (RectTransform)t;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Corners(Transform parent)
        {
            Corner(parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, 2f), new Vector2(2f, 18f));
            Corner(parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(18f, 2f), new Vector2(2f, 18f));
            Corner(parent, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18f, 2f), new Vector2(2f, 18f));
            Corner(parent, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(18f, 2f), new Vector2(2f, 18f));
        }

        static void Corner(Transform parent, Vector2 anchor, Vector2 pivot, Vector2 h, Vector2 v)
        {
            var a = new GameObject("cH", typeof(RectTransform), typeof(Image));
            a.transform.SetParent(parent, false);
            var ar = (RectTransform)a.transform;
            ar.anchorMin = ar.anchorMax = ar.pivot = anchor;
            ar.sizeDelta = h;
            ar.anchoredPosition = Vector2.zero;
            Paint(a, Accent, false);

            var b = new GameObject("cV", typeof(RectTransform), typeof(Image));
            b.transform.SetParent(parent, false);
            var br = (RectTransform)b.transform;
            br.anchorMin = br.anchorMax = br.pivot = pivot;
            br.sizeDelta = v;
            br.anchoredPosition = Vector2.zero;
            Paint(b, Accent, false);
        }
    }
}
