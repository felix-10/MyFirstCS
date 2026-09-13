using System.Collections.Generic;
using UnityEngine;

namespace Veinfire
{
    public static class CombatUtil
    {
        public static Transform NearestEnemy(Vector3 origin, float maxSqr = float.MaxValue)
        {
            return UniqueTarget(origin, Mathf.Sqrt(Mathf.Min(maxSqr, 1e8f)));
        }

        public static Transform UniqueTarget(Vector3 origin, float maxDist, EnemyHealth skip = null)
        {
            var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            Transform best = null;
            var bestScore = float.MaxValue;
            var skipId = skip != null ? skip.GetInstanceID() : 0;
            var maxSqr = maxDist * maxDist;
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                if (skipId != 0 && enemy.GetInstanceID() == skipId) continue;
                var dist = World.Planar(origin, enemy.transform.position).magnitude;
                if (dist * dist > maxSqr) continue;
                var score = dist - ThreatPull(enemy.Kind);
                if (score >= bestScore) continue;
                bestScore = score;
                best = enemy.transform;
            }

            return best;
        }

        static float ThreatPull(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Boss: return 2.4f;
                case EnemyKind.Elite: return 1.4f;
                case EnemyKind.Spitter: return 0.8f;
                case EnemyKind.Exploder: return 0.5f;
                default: return 0f;
            }
        }

        public static void DamageEnemiesInRadius(Vector3 origin, float radius, float damage, float knockback, PlayerHealth lifestealOwner = null, float lifesteal = 0f, PlayerCombatStats stats = null)
        {
            var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            var r2 = radius * radius;
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                if (World.Planar(origin, enemy.transform.position).sqrMagnitude > r2) continue;
                var crit = false;
                var hit = stats != null ? stats.RollHit(damage, out crit) : damage;
                enemy.Damage(hit, origin, knockback, crit, stats != null);
                if (lifestealOwner != null && lifesteal > 0f)
                    lifestealOwner.Heal(lifesteal);
            }
        }

        public static int GatherChain(Vector3 from, float firstRange, float hop, int max, List<EnemyHealth> into)
        {
            into.Clear();
            var used = new HashSet<int>();
            var cursor = from;
            var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (var n = 0; n < max; n++)
            {
                EnemyHealth best = null;
                var limit = n == 0 ? firstRange : hop;
                var bestSqr = limit * limit;
                for (var i = 0; i < enemies.Length; i++)
                {
                    var enemy = enemies[i];
                    if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                    if (used.Contains(enemy.GetInstanceID())) continue;
                    var d = World.Planar(cursor, enemy.transform.position).sqrMagnitude;
                    if (d > bestSqr) continue;
                    bestSqr = d;
                    best = enemy;
                }

                if (best == null) break;
                used.Add(best.GetInstanceID());
                into.Add(best);
                cursor = best.transform.position;
            }

            return into.Count;
        }
    }

    public static class ShotFx
    {
        public static void Dress(GameObject go, Color body, Vector3 scale, Color trail, float trailTime, float trailWidth)
        {
            if (go == null) return;
            go.transform.localScale = scale;
            PrimitiveFactory.Paint(go, body);
            PrimitiveFactory.AddTrail(go, trail, trailTime, trailWidth);
        }

        public static void Hymn(GameObject go, float size)
        {
            size = Mathf.Max(0.35f, size);
            Dress(go,
                new Color(1f, 0.18f, 0.05f),
                new Vector3(0.13f, 0.13f, 0.46f) * size,
                new Color(1f, 0.42f, 0.08f),
                0.07f,
                0.09f);
        }

        public static void Spiral(GameObject go, float size)
        {
            size = Mathf.Max(0.55f, size);
            Dress(go,
                new Color(0.28f, 0.92f, 1f),
                new Vector3(0.34f, 0.2f, 1.05f) * size,
                new Color(0.62f, 0.32f, 1f),
                0.34f,
                0.26f);
        }

        public static void ForWeapon(WeaponId id, GameObject go, float size = 1f)
        {
            size = Mathf.Max(0.45f, size);
            switch (id)
            {
                case WeaponId.Frost:
                case WeaponId.IceDisc:
                case WeaponId.Aurora:
                    Dress(go, new Color(0.55f, 0.9f, 1f), new Vector3(0.42f, 0.1f, 0.42f) * size,
                        new Color(0.7f, 0.95f, 1f), 0.16f, 0.2f);
                    break;
                case WeaponId.Magma:
                    Dress(go, new Color(1f, 0.38f, 0.08f), Vector3.one * (0.32f * size),
                        new Color(1f, 0.55f, 0.1f), 0.2f, 0.22f);
                    break;
                case WeaponId.FlameRing:
                    Dress(go, new Color(1f, 0.32f, 0.05f), new Vector3(0.95f, 0.08f, 0.95f) * size,
                        new Color(1f, 0.5f, 0.1f), 0.22f, 0.28f);
                    break;
                case WeaponId.PoisonFog:
                case WeaponId.Spore:
                    Dress(go, new Color(0.35f, 0.9f, 0.22f), Vector3.one * (0.3f * size),
                        new Color(0.45f, 1f, 0.3f), 0.18f, 0.2f);
                    break;
                case WeaponId.ChaosShot:
                    Dress(go, new Color(0.95f, 0.2f, 0.75f), new Vector3(0.22f, 0.36f, 0.22f) * size,
                        new Color(1f, 0.35f, 0.9f), 0.22f, 0.18f);
                    break;
                case WeaponId.HolyAura:
                case WeaponId.StarBurst:
                    Dress(go, new Color(1f, 0.92f, 0.45f), Vector3.one * (0.26f * size),
                        new Color(1f, 0.95f, 0.6f), 0.14f, 0.16f);
                    break;
                case WeaponId.ShadowBlade:
                case WeaponId.DarkOrbit:
                    Dress(go, new Color(0.45f, 0.18f, 0.85f), new Vector3(0.16f, 0.08f, 0.55f) * size,
                        new Color(0.55f, 0.25f, 1f), 0.2f, 0.14f);
                    break;
                case WeaponId.IonJet:
                    Dress(go, new Color(0.35f, 0.75f, 1f), new Vector3(0.1f, 0.1f, 0.7f) * size,
                        new Color(0.45f, 0.85f, 1f), 0.12f, 0.1f);
                    break;
                case WeaponId.Spear:
                    Dress(go, new Color(0.82f, 0.86f, 0.9f), new Vector3(0.12f, 0.12f, 0.72f) * size,
                        new Color(0.9f, 0.92f, 0.95f), 0.1f, 0.08f);
                    break;
                case WeaponId.MoonKnife:
                    Dress(go, new Color(0.75f, 0.8f, 1f), new Vector3(0.38f, 0.08f, 0.22f) * size,
                        new Color(0.8f, 0.85f, 1f), 0.16f, 0.12f);
                    break;
                case WeaponId.Homing:
                    Dress(go, new Color(0.55f, 1f, 0.45f), Vector3.one * (0.24f * size),
                        new Color(0.65f, 1f, 0.5f), 0.2f, 0.16f);
                    break;
                case WeaponId.Grenade:
                    Dress(go, new Color(1f, 0.55f, 0.15f), Vector3.one * (0.38f * size),
                        new Color(1f, 0.7f, 0.2f), 0.1f, 0.22f);
                    break;
                case WeaponId.SpikeFan:
                    Dress(go, new Color(0.72f, 0.74f, 0.7f), new Vector3(0.1f, 0.1f, 0.4f) * size,
                        new Color(0.85f, 0.85f, 0.8f), 0.08f, 0.08f);
                    break;
                case WeaponId.Refract:
                    Dress(go, new Color(0.9f, 0.95f, 1f), new Vector3(0.18f, 0.18f, 0.18f) * size,
                        new Color(0.95f, 1f, 1f), 0.18f, 0.14f);
                    break;
                default:
                    Dress(go, new Color(1f, 0.78f, 0.28f), Vector3.one * (0.22f * size),
                        new Color(1f, 0.82f, 0.35f), 0.12f, 0.14f);
                    break;
            }
        }

        public static GameObject Flash(string name, Vector3 at, Color color, Vector3 scale, Quaternion rot, float life, PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = at;
            go.transform.rotation = rot;
            go.transform.localScale = scale;
            PrimitiveFactory.Paint(go, color);
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            Object.Destroy(go, life);
            return go;
        }

        public static void Shock(Vector3 at, Color color, float diameter)
        {
            var go = Flash("Shock", at + Vector3.up * 0.1f, color, new Vector3(0.6f, 0.08f, 0.6f), Quaternion.identity, 0.28f, PrimitiveType.Cylinder);
            var grow = go.AddComponent<VfxGrow>();
            grow.Target = diameter;
            grow.Life = 0.22f;
        }

        public static void Ring(Vector3 at, Color color, float diameter, float life)
        {
            Flash("Ring", at + Vector3.up * 0.08f, color, new Vector3(diameter, 0.06f, diameter), Quaternion.identity, life, PrimitiveType.Cylinder);
        }

        public static void Lightning(Vector3 from, Vector3 to, Color color, float life = 0.1f, int jags = 7, float width = 0.14f)
        {
            var go = new GameObject("Lightning");
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = Mathf.Max(3, jags);
            line.startWidth = width;
            line.endWidth = width * 0.35f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            if (shader != null)
            {
                var mat = new Material(shader);
                if (mat.HasProperty("_Color")) mat.color = color;
                line.material = mat;
            }

            line.startColor = Color.white;
            line.endColor = color;
            var delta = to - from;
            var len = delta.magnitude;
            var axis = len > 0.01f ? delta / len : Vector3.forward;
            var side = Vector3.Cross(axis, Vector3.up);
            if (side.sqrMagnitude < 0.01f) side = Vector3.Cross(axis, Vector3.right);
            side.Normalize();
            var n = line.positionCount;
            for (var i = 0; i < n; i++)
            {
                var t = i / (n - 1f);
                var p = Vector3.Lerp(from, to, t);
                if (i > 0 && i < n - 1)
                    p += side * Random.Range(-0.38f, 0.38f) + Vector3.up * Random.Range(-0.12f, 0.28f);
                line.SetPosition(i, p);
            }

            Object.Destroy(go, life);
            if (len > 1.2f)
            {
                var mid = Vector3.Lerp(from, to, 0.45f) + side * Random.Range(-0.8f, 0.8f) + Vector3.up * 0.2f;
                LightningFork(from, to, mid, color, life * 0.8f, width * 0.55f);
            }
        }

        static void LightningFork(Vector3 from, Vector3 to, Vector3 mid, Color color, float life, float width)
        {
            var go = new GameObject("LightningFork");
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 4;
            line.startWidth = width;
            line.endWidth = width * 0.2f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                var mat = new Material(shader);
                if (mat.HasProperty("_Color")) mat.color = color;
                line.material = mat;
            }

            line.startColor = Color.white;
            line.endColor = new Color(color.r, color.g, color.b, 0.2f);
            line.SetPosition(0, Vector3.Lerp(from, to, 0.35f));
            line.SetPosition(1, mid);
            line.SetPosition(2, mid + Vector3.up * 0.15f);
            line.SetPosition(3, Vector3.Lerp(mid, to, 0.4f));
            Object.Destroy(go, life);
        }

        public static void SkyBolt(Vector3 at, Color color)
        {
            var sky = at + Vector3.up * 11f + new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.6f, 0.6f));
            var ground = at + Vector3.up * 0.15f;
            Lightning(sky, ground, color, 0.12f, 9, 0.18f);
            Lightning(sky + Vector3.right * 0.4f, ground, Color.white, 0.08f, 6, 0.07f);
            Ring(at, color, 1.5f, 0.1f);
        }

        public static void Whip(Vector3 from, Vector3 to, Color color, float life = 0.1f)
        {
            var go = new GameObject("Whip");
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 8;
            line.startWidth = 0.16f;
            line.endWidth = 0.05f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                var mat = new Material(shader);
                if (mat.HasProperty("_Color")) mat.color = color;
                line.material = mat;
            }

            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0.15f);
            var delta = to - from;
            var sag = Vector3.up * Mathf.Clamp(delta.magnitude * 0.12f, 0.2f, 0.7f);
            for (var i = 0; i < 8; i++)
            {
                var t = i / 7f;
                var p = Vector3.Lerp(from, to, t);
                p -= sag * (4f * t * (1f - t));
                line.SetPosition(i, p);
            }

            Object.Destroy(go, life);
        }

        public static void Impact(WeaponId id, Vector3 at)
        {
            switch (id)
            {
                case WeaponId.Hammer:
                    Flash("Hammer", at + Vector3.up * 0.45f, new Color(0.72f, 0.5f, 0.22f), new Vector3(1.1f, 0.35f, 1.1f), Quaternion.identity, 0.16f, PrimitiveType.Cube);
                    break;
                case WeaponId.ArrowRain:
                    Flash("Rain", at + Vector3.up * 3.2f, new Color(0.85f, 0.9f, 0.7f), new Vector3(0.12f, 6.2f, 0.12f), Quaternion.Euler(12f, 30f, 0f), 0.14f, PrimitiveType.Cube);
                    Ring(at, new Color(0.8f, 0.85f, 0.55f), 1.4f, 0.12f);
                    break;
                case WeaponId.Thorns:
                    Flash("Thorn", at + Vector3.up * 0.5f, new Color(0.25f, 0.7f, 0.18f), new Vector3(0.22f, 1.1f, 0.22f), Quaternion.identity, 0.14f, PrimitiveType.Cube);
                    break;
                case WeaponId.Shockwave:
                case WeaponId.Gravity:
                    Ring(at, new Color(0.7f, 0.55f, 1f), 3.4f, 0.22f);
                    break;
                case WeaponId.HolyAura:
                    Ring(at, new Color(1f, 0.92f, 0.45f), 2.6f, 0.2f);
                    break;
                case WeaponId.PoisonFog:
                    Ring(at, new Color(0.4f, 0.9f, 0.25f), 2.6f, 0.2f);
                    break;
            }
        }
    }

    public sealed class VfxFall : MonoBehaviour
    {
        public Vector3 Target;
        public float Duration = 0.22f;
        Vector3 _from;
        float _t;

        void OnEnable()
        {
            _from = transform.position;
            _t = 0f;
        }

        void Update()
        {
            if (GameSession.IsPaused) return;
            _t += Time.deltaTime / Mathf.Max(0.05f, Duration);
            transform.position = Vector3.Lerp(_from, Target, Mathf.Clamp01(_t));
            if (_t >= 1f) Destroy(gameObject);
        }
    }

    public sealed class VfxGrow : MonoBehaviour
    {
        public float Target = 4f;
        public float Life = 0.22f;
        float _t;

        void Update()
        {
            if (GameSession.IsPaused) return;
            _t += Time.deltaTime / Mathf.Max(0.05f, Life);
            var s = Mathf.Lerp(0.45f, Target, Mathf.Clamp01(_t));
            transform.localScale = new Vector3(s, 0.08f, s);
            if (_t >= 1f) Destroy(gameObject);
        }
    }
}
