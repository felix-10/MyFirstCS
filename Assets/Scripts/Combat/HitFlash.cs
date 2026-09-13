using UnityEngine;

namespace Veinfire
{
    public sealed class HitFlash : MonoBehaviour
    {
        Renderer[] _renderers;
        Color[] _colors;
        float _until;

        void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _colors = new Color[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    var mat = _renderers[i].sharedMaterial;
                    if (mat != null && mat.HasProperty("_BaseColor"))
                        _colors[i] = mat.GetColor("_BaseColor");
                    else if (mat != null && mat.HasProperty("_Color"))
                        _colors[i] = mat.color;
                    else
                        _colors[i] = Color.white;
                }
            }
        }

        public void Play()
        {
            _until = Time.unscaledTime + 0.11f;
        }

        void LateUpdate()
        {
            var flash = Time.unscaledTime < _until;
            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                var mat = _renderers[i].material;
                if (mat.HasProperty("_Color"))
                    mat.color = flash ? Color.white : _colors[i];
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", flash ? Color.white : _colors[i]);
            }
        }
    }
}
