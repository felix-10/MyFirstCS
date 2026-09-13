using System;
using UnityEngine;

namespace Veinfire
{
    public enum MapId
    {
        VeinWaste = 0,
        BoneCloister = 1,
        AshMarsh = 2,
        CaveHollow = 3,
        InfernalRuin = 4
    }

    public static class MapCatalog
    {
        public const string RootName = "MapRoot";

        public static MapId ParseId(string id)
        {
            switch (id)
            {
                case "BoneCloister": return MapId.BoneCloister;
                case "AshMarsh": return MapId.AshMarsh;
                case "CaveHollow": return MapId.CaveHollow;
                case "InfernalRuin": return MapId.InfernalRuin;
                default: return MapId.VeinWaste;
            }
        }

        public static MapSpec Current => BalanceTables.Spec(GameInstaller.CurrentMap, RunConfig.MapStar);

        public static int ClampStar(MapId id, int star)
        {
            if (id == MapId.CaveHollow) return 4;
            if (id == MapId.InfernalRuin) return 5;
            return Mathf.Clamp(star, 1, 3);
        }

        public static string Title(MapId id) => Title(id, RunConfig.MapStar);

        public static string Title(MapId id, int star)
        {
            var spec = BalanceTables.Spec(id, ClampStar(id, star));
            if (!string.IsNullOrEmpty(spec.title)) return spec.title;
            switch (id)
            {
                case MapId.BoneCloister: return $"生命绝地 ★{star}";
                case MapId.AshMarsh: return $"神之平原 ★{star}";
                case MapId.CaveHollow: return "裂隙洞窟 ★4";
                case MapId.InfernalRuin: return "炼狱遗迹 ★5";
                default: return $"末日城市 ★{star}";
            }
        }

        public static string Blurb(MapId id) => Blurb(id, RunConfig.MapStar);

        public static string Blurb(MapId id, int star)
        {
            var spec = BalanceTables.Spec(id, ClampStar(id, star));
            if (!string.IsNullOrEmpty(spec.blurb)) return spec.blurb;
            switch (id)
            {
                case MapId.BoneCloister: return "部分地面带有腐蚀，触碰或经过会持续掉血。";
                case MapId.AshMarsh: return "障碍很少，空间开阔，但怪物攻击力更高。";
                case MapId.CaveHollow: return "狭窄回廊，精英更早出现。";
                case MapId.InfernalRuin: return "多重地形伤害，Boss 会清 DOT。";
                default: return "废墟密布。怪物血量与护甲更高，更耐打。";
            }
        }

        public static string CardText(MapId id)
        {
            return $"{Title(id)}\n{Blurb(id)}";
        }

        public static string UnlockHint(MapId id, int star)
        {
            star = ClampStar(id, star);
            if (id == MapId.CaveHollow) return "玩家模式：通关任意地图 ★3 后解锁。开发者模式可直接选择。";
            if (id == MapId.InfernalRuin) return "玩家模式：通关裂隙洞窟 ★4 后解锁。开发者模式可直接选择。";
            if (star <= 1) return "初始开放。";
            return $"玩家模式：通关本图 ★{star - 1} 后解锁。开发者模式可直接选择。";
        }

        public static string Hover(MapSpec spec, bool open)
        {
            if (spec == null) return "";
            var body =
                spec.blurb + "\n" +
                $"生命×{spec.hp:0.0}    护甲+{spec.armor:0}    伤害×{spec.damage:0.00}    移速×{spec.move:0.00}\n" +
                $"地形腐蚀 {spec.hazard:0}/秒    基础残烬 {spec.ember}    风险权重 {spec.risk:0%}";
            if (spec.eliteEarly > 0.01f) body += $"\n精英提前 {spec.eliteEarly:0}s";
            body += "\n" + (open ? "已解锁，可开战。" : UnlockHint(spec.MapId, spec.star));
            return body;
        }

        public static float EnemyHpMul(MapId id) => BalanceTables.Spec(id, StarOf(id)).hp;
        public static float EnemyArmor(MapId id) => BalanceTables.Spec(id, StarOf(id)).armor;
        public static float EnemyDamageMul(MapId id) => Mathf.Max(1f, BalanceTables.Spec(id, StarOf(id)).damage);
        public static float EnemyMoveMul => Mathf.Max(1f, Current.move);
        public static bool HighStar => RunConfig.MapStar >= 4;

        static int StarOf(MapId id) => id == GameInstaller.CurrentMap ? MapCatalog.ClampStar(id, RunConfig.MapStar) : MapCatalog.ClampStar(id, 2);

        public static void Clear()
        {
            var root = GameObject.Find(RootName);
            if (root != null) UnityEngine.Object.Destroy(root);
        }

        public static void Build(MapId id)
        {
            Clear();
            var root = new GameObject(RootName);
            switch (id)
            {
                case MapId.BoneCloister:
                    BuildDeadland(root.transform, RunConfig.MapStar);
                    break;
                case MapId.AshMarsh:
                    BuildPlain(root.transform, RunConfig.MapStar);
                    break;
                case MapId.CaveHollow:
                    BuildCave(root.transform);
                    break;
                case MapId.InfernalRuin:
                    BuildInfernal(root.transform);
                    break;
                default:
                    BuildCity(root.transform, RunConfig.MapStar);
                    break;
            }
        }

        static void BuildCity(Transform root, int star)
        {
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(z) <= 1) continue;
                    if ((x + z) % 2 == 0) continue;
                    var h = 2.4f + (Mathf.Abs(x) + Mathf.Abs(z)) * 0.15f;
                    Block(root, new Vector3(x * 7.2f, h * 0.5f, z * 7.2f), new Vector3(3.2f, h, 3.2f), new Color(0.22f, 0.2f, 0.24f));
                }
            }

            for (var i = -2; i <= 2; i++)
            {
                if (i == 0) continue;
                Block(root, new Vector3(i * 8f, 1.4f, 24f), new Vector3(5.2f, 2.8f, 1.1f), new Color(0.18f, 0.16f, 0.2f));
                Block(root, new Vector3(i * 8f, 1.4f, -24f), new Vector3(5.2f, 2.8f, 1.1f), new Color(0.18f, 0.16f, 0.2f));
                Block(root, new Vector3(24f, 1.4f, i * 8f), new Vector3(1.1f, 2.8f, 5.2f), new Color(0.18f, 0.16f, 0.2f));
                Block(root, new Vector3(-24f, 1.4f, i * 8f), new Vector3(1.1f, 2.8f, 5.2f), new Color(0.18f, 0.16f, 0.2f));
            }

            if (star >= 3)
            {
                Block(root, new Vector3(4f, 1.6f, 4f), new Vector3(6.4f, 3.2f, 1.2f), new Color(0.2f, 0.18f, 0.22f));
                Block(root, new Vector3(-6f, 1.6f, -5f), new Vector3(1.2f, 3.2f, 7.2f), new Color(0.2f, 0.18f, 0.22f));
            }
        }

        static void BuildDeadland(Transform root, int star)
        {
            var rng = new System.Random(21);
            for (var i = 0; i < 10; i++)
            {
                var a = (float)rng.NextDouble() * Mathf.PI * 2f;
                var r = 9f + (float)rng.NextDouble() * 18f;
                var pos = new Vector3(Mathf.Cos(a) * r, 0.9f, Mathf.Sin(a) * r);
                if (pos.sqrMagnitude < 20f) continue;
                var s = 1.1f + (float)rng.NextDouble() * 1.4f;
                Block(root, pos, new Vector3(s, 1.8f, s), new Color(0.16f, 0.12f, 0.1f));
            }

            var pads = star >= 3 ? 14 : 8;
            for (var i = 0; i < pads; i++)
            {
                var a = i / (float)pads * Mathf.PI * 2f + 0.4f;
                var r = 11f + (i % 2) * 7f;
                Hazard(root, new Vector3(Mathf.Cos(a) * r, 0.12f, Mathf.Sin(a) * r), new Vector3(5.2f, 0.22f, 5.2f));
            }
        }

        static void BuildPlain(Transform root, int star)
        {
            Block(root, new Vector3(14f, 0.7f, 11f), new Vector3(1.4f, 1.4f, 1.4f), new Color(0.72f, 0.62f, 0.38f));
            Block(root, new Vector3(-16f, 0.55f, -8f), new Vector3(1.1f, 1.1f, 1.8f), new Color(0.68f, 0.58f, 0.34f));
            Block(root, new Vector3(6f, 0.5f, -18f), new Vector3(1.6f, 1f, 1.2f), new Color(0.7f, 0.6f, 0.36f));
            if (star >= 3)
            {
                Hazard(root, new Vector3(8f, 0.12f, -6f), new Vector3(4.4f, 0.22f, 4.4f));
                Hazard(root, new Vector3(-10f, 0.12f, 9f), new Vector3(5.2f, 0.22f, 3.6f));
            }
        }

        static void BuildCave(Transform root)
        {
            for (var i = -4; i <= 4; i++)
            {
                if (i == 0) continue;
                Block(root, new Vector3(i * 5.2f, 2.2f, i % 2 == 0 ? 8f : -8f), new Vector3(1.6f, 4.4f, 1.6f), new Color(0.14f, 0.12f, 0.16f));
                Block(root, new Vector3(i % 2 == 0 ? 10f : -10f, 2.4f, i * 5.2f), new Vector3(1.8f, 4.8f, 1.8f), new Color(0.12f, 0.11f, 0.14f));
            }

            for (var i = -2; i <= 2; i++)
            {
                Block(root, new Vector3(i * 7f, 1.8f, 18f), new Vector3(4.4f, 3.6f, 1.2f), new Color(0.16f, 0.14f, 0.18f));
                Block(root, new Vector3(i * 7f, 1.8f, -18f), new Vector3(4.4f, 3.6f, 1.2f), new Color(0.16f, 0.14f, 0.18f));
            }

            Hazard(root, new Vector3(0f, 0.12f, 12f), new Vector3(3.2f, 0.22f, 3.2f));
        }

        static void BuildInfernal(Transform root)
        {
            BuildCity(root, 3);
            for (var i = 0; i < 10; i++)
            {
                var a = i / 10f * Mathf.PI * 2f;
                Hazard(root, new Vector3(Mathf.Cos(a) * 14f, 0.12f, Mathf.Sin(a) * 14f), new Vector3(4.2f, 0.22f, 4.2f));
            }
        }

        static void Block(Transform root, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "MapSolid";
            go.transform.SetParent(root, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            PrimitiveFactory.Paint(go, color);
            var col = go.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = false;

            var body = go.GetComponent<Rigidbody>();
            if (body == null) body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.detectCollisions = true;
        }

        static void Hazard(Transform root, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "MapHazard";
            go.transform.SetParent(root, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            PrimitiveFactory.Paint(go, new Color(0.35f, 0.95f, 0.28f, 0.7f));
            var col = go.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = true;
            var body = go.GetComponent<Rigidbody>();
            if (body == null) body = go.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            go.AddComponent<MapHazard>().DamagePerSecond = Mathf.Max(8f, Current.hazard);
        }
    }

    [Serializable]
    public sealed class MapSpec
    {
        public string id;
        public int star = 1;
        public string title;
        public string blurb;
        public float hp = 1f;
        public float armor;
        public float damage = 1f;
        public float move = 1f;
        public float hazard;
        public int ember = 60;
        public float risk;
        public float eliteEarly;
        public MapId MapId => MapCatalog.ParseId(id);
    }

    [Serializable]
    public sealed class MapFile
    {
        public MapSpec[] maps;
    }

    [Serializable]
    public sealed class BalanceFile
    {
        public int version = 26;
        public float[] emberCurseClamp = { 1f, 2.2f };
        public float hazardResistCap = 0.7f;
        public int weaponSlots = 8;
        public int rerolls = 3;
        public int replaces = 8;
        public int[] uniqueLevels = { 3, 6, 12, 18, 25 };
        public float dotLatch = 0.25f;
        public int metaResetCost = 120;
        public float farmWindowMin = 20f;
        public float farmDecay = 0.2f;
        public float farmFloor = 0.4f;
        public float farmClearMin = 120f;
        public float failEmberFloor = 0.3f;
        public MetaCaps metaCaps = new MetaCaps();
        public EndlessCaps endlessCaps = new EndlessCaps();
    }

    [Serializable]
    public sealed class MetaCaps
    {
        public float haste = 0.06f;
        public float veinDot = 0.16f;
        public float hp = 20f;
        public float crit = 0.06f;
        public float armor = 2f;
    }

    [Serializable]
    public sealed class EndlessCaps
    {
        public float hp = 4f;
        public float move = 2f;
        public float damage = 3.5f;
    }

    public static class BalanceTables
    {
        static BalanceFile _bal;
        static MapSpec[] _maps = System.Array.Empty<MapSpec>();

        public static BalanceFile Bal
        {
            get
            {
                Load();
                return _bal;
            }
        }

        public static MapSpec[] Maps
        {
            get
            {
                Load();
                return _maps;
            }
        }

        public static float DotLatch => Mathf.Max(0.12f, Bal.dotLatch);
        public static int Rerolls => Mathf.Max(1, Bal.rerolls);
        public static int[] UniqueLevels => Bal.uniqueLevels ?? new[] { 3, 6, 12, 18, 25 };

        public static void Load()
        {
            if (_bal != null && _maps != null && _maps.Length > 0) return;
            _bal = Read("BalanceConfig.json", new BalanceFile());
            if (_bal.metaCaps == null) _bal.metaCaps = new MetaCaps();
            if (_bal.endlessCaps == null) _bal.endlessCaps = new EndlessCaps();
            if (_bal.uniqueLevels == null || _bal.uniqueLevels.Length == 0)
                _bal.uniqueLevels = new[] { 3, 6, 12, 18, 25 };
            var file = Read("MapBalanceConfig.json", new MapFile());
            _maps = file.maps != null && file.maps.Length > 0 ? file.maps : FallbackMaps();
        }

        static T Read<T>(string name, T fallback)
        {
            try
            {
                var path = System.IO.Path.Combine(Application.streamingAssetsPath, name);
                if (System.IO.File.Exists(path))
                    return JsonUtility.FromJson<T>(System.IO.File.ReadAllText(path)) ?? fallback;
            }
            catch
            {
            }

            return fallback;
        }

        static MapSpec[] FallbackMaps()
        {
            return new[]
            {
                new MapSpec { id = "VeinWaste", star = 1, title = "末日城市 ★1", blurb = "楼块网格。", hp = 1.1f, armor = 3f, ember = 60 },
                new MapSpec { id = "VeinWaste", star = 2, title = "末日城市 ★2", blurb = "废墟密布。", hp = 1.4f, armor = 8f, ember = 90 },
                new MapSpec { id = "VeinWaste", star = 3, title = "末日城市 ★3", blurb = "道路封堵。", hp = 1.8f, armor = 12f, damage = 1.15f, ember = 130 },
                new MapSpec { id = "BoneCloister", star = 1, title = "生命绝地 ★1", blurb = "腐蚀垫。", hazard = 12f, ember = 60 },
                new MapSpec { id = "BoneCloister", star = 2, title = "生命绝地 ★2", blurb = "更多腐蚀。", hp = 1.3f, hazard = 16f, ember = 90 },
                new MapSpec { id = "BoneCloister", star = 3, title = "生命绝地 ★3", blurb = "大面积腐蚀。", hp = 1.7f, armor = 6f, hazard = 21f, ember = 130 },
                new MapSpec { id = "AshMarsh", star = 1, title = "神之平原 ★1", blurb = "开阔平原。", damage = 1.1f, ember = 60 },
                new MapSpec { id = "AshMarsh", star = 2, title = "神之平原 ★2", blurb = "怪物更疼。", damage = 1.4f, ember = 90 },
                new MapSpec { id = "AshMarsh", star = 3, title = "神之平原 ★3", blurb = "腐蚀斑块。", hp = 1.2f, armor = 2f, damage = 1.8f, move = 1.1f, hazard = 10f, ember = 130 },
                new MapSpec { id = "CaveHollow", star = 4, title = "裂隙洞窟 ★4", blurb = "狭窄回廊。", hp = 2.2f, armor = 14f, damage = 1.6f, ember = 190, risk = 0.55f },
                new MapSpec { id = "InfernalRuin", star = 5, title = "炼狱遗迹 ★5", blurb = "多重地形伤害。", hp = 2.8f, armor = 18f, damage = 2f, ember = 260, risk = 0.75f }
            };
        }

        public static MapSpec Spec(MapId id, int star)
        {
            Load();
            MapSpec best = null;
            for (var i = 0; i < _maps.Length; i++)
            {
                var row = _maps[i];
                if (row.MapId != id) continue;
                if (row.star == star) return row;
                if (best == null || Mathf.Abs(row.star - star) < Mathf.Abs(best.star - star))
                    best = row;
            }

            return best ?? new MapSpec { id = id.ToString(), star = star, ember = 60 };
        }
    }
}
