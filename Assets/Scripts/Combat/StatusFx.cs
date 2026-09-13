using UnityEngine;

namespace Veinfire
{
    public sealed class StatusFx : MonoBehaviour
    {
        Transform _burn;
        Transform _wet;
        Transform _stone;
        Transform[] _pips;
        float _spin;

        public static StatusFx Ensure(Transform host)
        {
            var fx = host.GetComponent<StatusFx>();
            if (fx == null) fx = host.gameObject.AddComponent<StatusFx>();
            fx.Build();
            return fx;
        }

        void Build()
        {
            if (_burn != null) return;
            _burn = MakeOrb("Burn", new Color(1f, 0.4f, 0.08f, 1f), 0.85f, 1.45f);
            _wet = MakeOrb("Wet", new Color(0.25f, 0.75f, 1f, 1f), 1.35f, 0.7f);
            _stone = new GameObject("Stone").transform;
            _stone.SetParent(transform, false);
            _pips = new Transform[3];
            for (var i = 0; i < 3; i++)
            {
                var a = i / 3f * Mathf.PI * 2f;
                _pips[i] = MakePip(new Vector3(Mathf.Cos(a) * 0.7f, 0.35f, Mathf.Sin(a) * 0.7f));
            }

            ShowBurn(false);
            ShowWet(false);
            SetPetrify(0);
        }

        public void ShowBurn(bool on)
        {
            if (_burn != null) _burn.gameObject.SetActive(on);
        }

        public void ShowWet(bool on)
        {
            if (_wet != null) _wet.gameObject.SetActive(on);
        }

        public void SetPetrify(int stacks)
        {
            if (_pips == null) return;
            for (var i = 0; i < _pips.Length; i++)
                if (_pips[i] != null) _pips[i].gameObject.SetActive(i < stacks);
        }

        void LateUpdate()
        {
            if (GameSession.IsPaused) return;
            _spin += 160f * Time.deltaTime;
            if (_burn != null && _burn.gameObject.activeSelf)
            {
                var pulse = 0.5f + 0.12f * Mathf.Sin(Time.time * 10f);
                _burn.localScale = Vector3.one * pulse;
                _burn.localRotation = Quaternion.Euler(0f, _spin, 18f);
            }

            if (_wet != null && _wet.gameObject.activeSelf)
            {
                var drip = 1.05f + 0.08f * Mathf.Sin(Time.time * 4f);
                _wet.localScale = Vector3.one * drip;
            }
        }

        Transform MakeOrb(string name, Color color, float scale, float y)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up * y;
            go.transform.localScale = Vector3.one * scale;
            StripPhysics(go);
            PaintFx(go, color);
            return go.transform;
        }

        Transform MakePip(Vector3 local)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Pip";
            go.transform.SetParent(_stone, false);
            go.transform.localPosition = local;
            go.transform.localScale = new Vector3(0.38f, 0.55f, 0.28f);
            StripPhysics(go);
            PaintFx(go, new Color(0.92f, 0.82f, 0.42f));
            return go.transform;
        }

        static void PaintFx(GameObject go, Color color)
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (shader == null) return;
            var mat = new Material(shader);
            if (mat.HasProperty("_Color")) mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.renderQueue = 4000;
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void StripPhysics(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var body = go.GetComponent<Rigidbody>();
            if (body != null) Destroy(body);
        }
    }
}
