using UnityEngine;
using UnityEngine.UI;

namespace Veinfire
{
    public sealed class UiTip : MonoBehaviour
    {
        static UiTip _i;
        RectTransform _rt;
        Text _title;
        Text _body;
        CanvasGroup _group;
        Canvas _canvas;

        public static void Bind(Canvas canvas)
        {
            if (_i != null && _i._canvas == canvas) return;
            if (_i != null)
            {
                UnityEngine.Object.Destroy(_i.gameObject);
                _i = null;
            }
            var go = UiStyle.Frame(canvas.transform, "Tip", new Vector2(420f, 180f), Vector2.zero, new Color(0.03f, 0.07f, 0.09f, 0.97f), false);
            go.transform.SetAsLastSibling();
            _i = go.AddComponent<UiTip>();
            _i._canvas = canvas;
            _i._rt = (RectTransform)go.transform;
            _i._group = go.AddComponent<CanvasGroup>();
            _i._group.blocksRaycasts = false;
            _i._group.interactable = false;
            _i._title = UiStyle.Label(go.transform, "T", "", 20, UiStyle.Accent, TextAnchor.UpperLeft);
            var tr = _i._title.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -14f);
            tr.sizeDelta = new Vector2(-28f, 32f);
            _i._body = UiStyle.Label(go.transform, "B", "", 16, UiStyle.Text, TextAnchor.UpperLeft);
            var br = _i._body.rectTransform;
            br.anchorMin = Vector2.zero;
            br.anchorMax = Vector2.one;
            br.offsetMin = new Vector2(16f, 14f);
            br.offsetMax = new Vector2(-16f, -48f);
            Hide();
        }

        public static void Show(string title, string body)
        {
            if (_i == null || string.IsNullOrEmpty(body)) return;
            _i._title.text = title ?? "";
            _i._body.text = body;
            var lines = Mathf.Clamp(2 + body.Length / 18, 4, 10);
            _i._rt.sizeDelta = new Vector2(440f, 70f + lines * 18f);
            _i._group.alpha = 1f;
            _i.gameObject.SetActive(true);
            _i.transform.SetAsLastSibling();
        }

        public static void Hide()
        {
            if (_i == null) return;
            _i._group.alpha = 0f;
            _i.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (_group == null || _group.alpha < 0.01f) return;
            var cam = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay ? _canvas.worldCamera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform,
                Input.mousePosition,
                cam,
                out var local);
            var size = _rt.sizeDelta;
            local += new Vector2(28f + size.x * 0.5f, -24f - size.y * 0.5f);
            var limit = ((RectTransform)_canvas.transform).rect;
            local.x = Mathf.Clamp(local.x, -limit.width * 0.5f + size.x * 0.5f + 12f, limit.width * 0.5f - size.x * 0.5f - 12f);
            local.y = Mathf.Clamp(local.y, -limit.height * 0.5f + size.y * 0.5f + 12f, limit.height * 0.5f - size.y * 0.5f - 12f);
            _rt.anchoredPosition = local;
        }
    }
}
