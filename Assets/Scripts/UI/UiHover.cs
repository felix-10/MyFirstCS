using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Veinfire
{
    public sealed class UiHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image Edge;
        public Image Inner;
        public string TipTitle;
        public string TipBody;
        public Action OnHover;
        public Color EdgeIdle = UiStyle.AccentDim;
        public Color EdgeHot = UiStyle.Accent;
        public Color InnerIdle = UiStyle.Panel;
        public Color InnerHot = UiStyle.PanelHi;
        public bool Locked;

        RectTransform _rt;
        Vector3 _base = Vector3.one;
        bool _over;

        void Awake()
        {
            _rt = transform as RectTransform;
            if (_rt != null) _base = _rt.localScale;
        }

        public void SetPalette(Color edgeIdle, Color innerIdle, Color edgeHot, Color innerHot, bool locked)
        {
            EdgeIdle = edgeIdle;
            InnerIdle = innerIdle;
            EdgeHot = edgeHot;
            InnerHot = innerHot;
            Locked = locked;
            if (!_over) ApplyIdle();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _over = true;
            if (Edge != null) Edge.color = Locked ? EdgeIdle : EdgeHot;
            if (Inner != null) Inner.color = Locked ? InnerIdle : InnerHot;
            if (!string.IsNullOrEmpty(TipBody)) UiTip.Show(TipTitle, TipBody);
            OnHover?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _over = false;
            ApplyIdle();
            UiTip.Hide();
        }

        void ApplyIdle()
        {
            if (Edge != null) Edge.color = EdgeIdle;
            if (Inner != null) Inner.color = InnerIdle;
        }

        void Update()
        {
            if (_rt == null) return;
            var t = Locked ? 1f : (_over ? 1.045f : 1f);
            _rt.localScale = Vector3.Lerp(_rt.localScale, _base * t, Time.unscaledDeltaTime * 14f);
        }

        void OnDisable()
        {
            _over = false;
            if (_rt != null) _rt.localScale = _base;
        }
    }
}
