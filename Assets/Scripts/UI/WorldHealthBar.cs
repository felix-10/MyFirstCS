using UnityEngine;
using UnityEngine.Rendering;

namespace Veinfire
{
    public sealed class WorldHealthBar : MonoBehaviour
    {
        Transform _follow;
        Transform _fill;
        float _height;
        float _width = 1f;
        bool _ownerGone;
        bool _large;

        public static WorldHealthBar Attach(Transform follow, Color color, float height, bool large = false)
        {
            var root = new GameObject(large ? "BossHpBar" : "HpBar");
            var bar = root.AddComponent<WorldHealthBar>();
            bar._follow = follow;
            bar._height = height;
            bar._large = large;

            var bg = CreatePiece(root.transform, "Bg", Color.black, 80);
            bg.localScale = new Vector3(large ? 2.4f : 1.15f, large ? 0.18f : 0.14f, 1f);
            bar._fill = CreatePiece(root.transform, "Fill", color, 81);
            bar._fill.localScale = new Vector3(large ? 2.32f : 1.1f, large ? 0.14f : 0.1f, 1f);
            bar._width = large ? 2.32f : 1.1f;
            return bar;
        }

        static Transform CreatePiece(Transform parent, string name, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Solid(color, 16);
            renderer.sortingOrder = order;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                var material = new Material(shader);
                if (material.HasProperty("_Color")) material.color = Color.white;
                material.SetInt("_ZTest", (int)CompareFunction.Always);
                material.renderQueue = 4000;
                renderer.material = material;
            }

            return go.transform;
        }

        public void Set(float current, float max)
        {
            var ratio = max <= 0f ? 0f : Mathf.Clamp01(current / max);
            if (_fill == null) return;
            var w = Mathf.Max(0.04f, _width * ratio);
            _fill.localScale = new Vector3(w, _large ? 0.14f : 0.1f, 1f);
            _fill.localPosition = new Vector3((w - _width) * 0.5f, 0f, 0f);
        }

        void LateUpdate()
        {
            if (_follow == null)
            {
                if (!_ownerGone)
                {
                    _ownerGone = true;
                    Destroy(gameObject);
                }

                return;
            }

            var scale = Mathf.Max(_follow.lossyScale.x, _follow.lossyScale.y);
            var height = _large ? 1.6f + scale * 1.35f : 1.2f + scale * 0.85f;
            transform.position = _follow.position + Vector3.up * height;
            transform.localScale = Vector3.one * (_large ? 1.6f : 1f);
            var camera = Camera.main;
            if (camera != null)
                transform.rotation = Quaternion.LookRotation(transform.position - camera.transform.position);
        }
    }
}
