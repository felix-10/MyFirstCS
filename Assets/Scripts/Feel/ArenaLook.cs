using UnityEngine;

namespace Veinfire
{
    public static class ArenaLook
    {
        public static void Apply(MapId map, Transform player, Camera camera)
        {
            Color fog;
            Color ambient;
            Color back;
            float density;
            switch (map)
            {
                case MapId.BoneCloister:
                    fog = new Color(0.08f, 0.14f, 0.07f);
                    ambient = new Color(0.12f, 0.2f, 0.1f);
                    back = new Color(0.04f, 0.07f, 0.03f);
                    density = 0.034f;
                    break;
                case MapId.AshMarsh:
                    fog = new Color(0.18f, 0.16f, 0.1f);
                    ambient = new Color(0.28f, 0.24f, 0.16f);
                    back = new Color(0.12f, 0.1f, 0.06f);
                    density = 0.012f;
                    break;
                case MapId.CaveHollow:
                    fog = new Color(0.06f, 0.05f, 0.08f);
                    ambient = new Color(0.1f, 0.1f, 0.16f);
                    back = new Color(0.03f, 0.03f, 0.05f);
                    density = 0.045f;
                    break;
                case MapId.InfernalRuin:
                    fog = new Color(0.18f, 0.06f, 0.04f);
                    ambient = new Color(0.28f, 0.1f, 0.06f);
                    back = new Color(0.08f, 0.03f, 0.02f);
                    density = 0.028f;
                    break;
                default:
                    fog = new Color(0.08f, 0.07f, 0.09f);
                    ambient = new Color(0.16f, 0.14f, 0.18f);
                    back = new Color(0.05f, 0.045f, 0.06f);
                    density = 0.022f;
                    break;
            }

            RenderSettings.fog = true;
            RenderSettings.fogColor = fog;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = density;
            RenderSettings.ambientLight = ambient;
            if (camera != null)
                camera.backgroundColor = back;

            PaintGround(map);

            if (player != null && player.GetComponent<Light>() == null)
            {
                var light = player.gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = map == MapId.BoneCloister
                    ? new Color(0.45f, 1f, 0.4f)
                    : map == MapId.AshMarsh
                        ? new Color(1f, 0.88f, 0.45f)
                        : new Color(0.85f, 0.55f, 0.35f);
                light.intensity = 1.8f;
                light.range = 9f;
            }

            if (GameObject.Find("FillLight") == null)
            {
                var fill = new GameObject("FillLight");
                var light = fill.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(0.45f, 0.55f, 0.8f);
                light.intensity = 0.35f;
                fill.transform.rotation = Quaternion.Euler(20f, 140f, 0f);
            }

            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (var i = 0; i < lights.Length; i++)
            {
                var dir = lights[i];
                if (dir.type != LightType.Directional || dir.gameObject.name == "FillLight") continue;
                dir.color = map == MapId.AshMarsh
                    ? new Color(1f, 0.92f, 0.7f)
                    : map == MapId.BoneCloister
                        ? new Color(0.55f, 0.85f, 0.5f)
                        : new Color(0.85f, 0.75f, 0.7f);
                dir.intensity = 1.25f;
                dir.shadows = LightShadows.Soft;
                break;
            }
        }

        static void PaintGround(MapId map)
        {
            var ground = GameObject.Find("Ground");
            if (ground == null) return;
            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            var shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse");
            if (shader == null) return;
            var mat = new Material(shader);
            mat.mainTexture = MakeFloor(map);
            mat.color = Color.white;
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", map == MapId.BoneCloister ? 0.28f : 0.12f);
            renderer.sharedMaterial = mat;
        }

        static Texture2D MakeFloor(MapId map)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var nx = x / (float)size;
                    var ny = y / (float)size;
                    Color col;
                    if (map == MapId.BoneCloister)
                    {
                        var wet = Mathf.PerlinNoise(nx * 5f, ny * 5f);
                        col = Color.Lerp(new Color(0.12f, 0.18f, 0.08f), new Color(0.22f, 0.32f, 0.12f), wet);
                        if (wet > 0.62f) col = new Color(0.18f, 0.55f, 0.16f);
                    }
                    else if (map == MapId.AshMarsh)
                    {
                        var gold = Mathf.PerlinNoise(nx * 3f, ny * 3f);
                        col = Color.Lerp(new Color(0.42f, 0.36f, 0.18f), new Color(0.62f, 0.52f, 0.28f), gold);
                    }
                    else
                    {
                        var tile = (Mathf.FloorToInt(nx * 10f) + Mathf.FloorToInt(ny * 10f)) % 2 == 0;
                        col = tile ? new Color(0.16f, 0.15f, 0.17f) : new Color(0.12f, 0.11f, 0.13f);
                        var crack = Mathf.Pow(Mathf.PerlinNoise(nx * 9f, ny * 9f), 4f);
                        col += new Color(0.25f, 0.08f, 0.05f) * crack;
                    }

                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
