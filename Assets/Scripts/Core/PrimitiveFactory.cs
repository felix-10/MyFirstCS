using UnityEngine;

namespace Veinfire
{
    public static class PrimitiveFactory
    {
        public static GameObject Capsule(string name, Color color, bool trigger)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.GetComponent<CapsuleCollider>().isTrigger = trigger;
            var body = go.AddComponent<Rigidbody>();
            World.PrepareCharacterBody(body);
            Paint(go, color);
            return go;
        }

        public static GameObject Sphere(string name, Color color, float scale, bool trigger)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = Vector3.one * scale;
            go.GetComponent<SphereCollider>().isTrigger = trigger;
            var body = go.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            Paint(go, color);
            return go;
        }

        public static void StripRigidbody(GameObject go)
        {
            var body = go.GetComponent<Rigidbody>();
            if (body != null) Object.Destroy(body);
        }

        public static void Paint(GameObject go, Color color)
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Diffuse");
            if (shader == null) return;
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.35f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.15f);
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.85f);
            }

            renderer.sharedMaterial = material;
        }

        public static void AddTrail(GameObject go, Color color, float time = 0.12f, float width = 0.18f)
        {
            var child = go.transform.Find("Trail");
            var trailGo = child != null ? child.gameObject : new GameObject("Trail");
            if (child == null) trailGo.transform.SetParent(go.transform, false);
            var trail = trailGo.GetComponent<TrailRenderer>();
            if (trail == null) trail = trailGo.AddComponent<TrailRenderer>();
            if (trail == null) return;
            trail.time = time;
            trail.minVertexDistance = 0.08f;
            trail.widthMultiplier = width;
            trail.emitting = true;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (shader != null)
            {
                var mat = new Material(shader);
                if (mat.HasProperty("_Color")) mat.color = color;
                trail.material = mat;
            }

            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
        }
    }
}
