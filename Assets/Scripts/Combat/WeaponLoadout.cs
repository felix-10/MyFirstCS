using UnityEngine;

namespace Veinfire
{
    public enum WeaponId
    {
        Needle, Divine, Blade, Spiral, Star,
        Orbit, Nova, Frost, Lash, Grenade,
        ShadowBlade, FlameRing, SpikeFan, Gravity, PoisonFog,
        IonJet, Spear, StarBurst, DarkOrbit, HolyAura,
        IceDisc, Totem, Homing, Shockwave, Spore,
        ChainBall, Refract, ChaosShot, MoonKnife, Magma,
        Thorns, ArrowRain, Drain, Aurora, Hammer
    }

    public sealed class WeaponLoadout : MonoBehaviour
    {
        public AutoAimWeapon Needle;
        public readonly System.Collections.Generic.HashSet<WeaponId> Owned = new System.Collections.Generic.HashSet<WeaponId>();
        public const int MaxWeapons = 8;
        readonly System.Collections.Generic.Dictionary<WeaponId, GenericArm> _arms = new System.Collections.Generic.Dictionary<WeaponId, GenericArm>();

        public int Count
        {
            get
            {
                var n = Owned.Count - RunConfig.FusionSlots;
                return Mathf.Max(1, n);
            }
        }

        public int SupportCount
        {
            get
            {
                var n = 0;
                foreach (var id in Owned)
                    if (!UniqueCatalog.IsUniqueWeapon(id)) n++;
                return n;
            }
        }

        public bool CanUnlock => Count < MaxWeapons;
        public bool Owns(WeaponId id) => Owned.Contains(id);

        public void EquipStarter(UniqueId id)
        {
            WeaponId[] uniques = { WeaponId.Needle, WeaponId.Divine, WeaponId.Blade, WeaponId.Spiral, WeaponId.Star };
            for (var i = 0; i < uniques.Length; i++) Owned.Remove(uniques[i]);
            Owned.Add(UniqueCatalog.WeaponOf(id));
        }

        public void RestoreLevels(int needle, int orbit, int nova, int frost, int lash, int divine = 0, int blade = 0)
        {
            if (Needle != null && Owns(WeaponId.Needle)) Needle.Level = Mathf.Clamp(needle, 1, 5);
            RestoreWeapon(WeaponId.Orbit, orbit);
            RestoreWeapon(WeaponId.Nova, nova);
            RestoreWeapon(WeaponId.Frost, frost);
            RestoreWeapon(WeaponId.Lash, lash);
            RestoreWeapon(WeaponId.Divine, divine);
            RestoreWeapon(WeaponId.Blade, blade);
        }

        public void RestoreExtra(WeaponId id, int level) => RestoreWeapon(id, level);

        void RestoreWeapon(WeaponId id, int level)
        {
            if (level <= 0) return;
            if (!Owns(id))
            {
                if (UniqueCatalog.IsUniqueWeapon(id)) return;
                Unlock(id);
            }
            SetLevel(id, Mathf.Clamp(level, 1, 5));
        }

        void SetLevel(WeaponId id, int level)
        {
            switch (id)
            {
                case WeaponId.Orbit:
                    var orbit = GetComponent<OrbitWeapon>();
                    if (orbit != null) { orbit.Level = level; orbit.Rebuild(); }
                    break;
                case WeaponId.Nova:
                    var nova = GetComponent<NovaWeapon>();
                    if (nova != null) nova.Level = level;
                    break;
                case WeaponId.Frost:
                    var frost = GetComponent<FrostAura>();
                    if (frost != null) frost.Level = level;
                    break;
                case WeaponId.Lash:
                    var lash = GetComponent<ChainLash>();
                    if (lash != null) lash.Level = level;
                    break;
                case WeaponId.Divine:
                    var bolt = GetComponent<LightningWeapon>();
                    if (bolt != null) bolt.Level = level;
                    break;
                case WeaponId.Blade:
                    var blade = GetComponent<TemorisSword>();
                    if (blade != null) blade.Level = level;
                    break;
                case WeaponId.Needle:
                    if (Needle != null) Needle.Level = level;
                    break;
                case WeaponId.Spiral:
                    var sp = GetComponent<SpiralHuntWeapon>();
                    if (sp != null) sp.Level = level;
                    break;
                case WeaponId.Star:
                    var st = GetComponent<StarfallInstrument>();
                    if (st != null) st.Level = level;
                    break;
                default:
                    if (_arms.TryGetValue(id, out var arm) && arm != null) arm.Level = level;
                    break;
            }
        }

        public void BindNeedle(AutoAimWeapon needle, Projectile prefab)
        {
            Needle = needle;
            if (Needle.ProjectilePrefab == null)
                Needle.ProjectilePrefab = prefab;
        }

        public bool Unlock(WeaponId id)
        {
            if (Owns(id) || UniqueCatalog.IsUniqueWeapon(id)) return false;
            if (!CanUnlock) return false;
            return Attach(id);
        }

        public bool Replace(WeaponId drop, WeaponId add, System.Collections.Generic.List<TraitRecord> passives)
        {
            if (!Owns(drop) || UniqueCatalog.IsUniqueWeapon(drop)) return false;
            if (Owns(add) || UniqueCatalog.IsUniqueWeapon(add)) return false;
            if (!WeaponCatalog.Unlocked(add)) return false;
            if (RunConfig.ReplacesLeft <= 0) return false;
            Drop(drop, passives);
            if (!Attach(add)) return false;
            RunConfig.ReplacesLeft = Mathf.Max(0, RunConfig.ReplacesLeft - 1);
            return true;
        }

        public void Drop(WeaponId id, System.Collections.Generic.List<TraitRecord> passives)
        {
            if (!Owns(id) || UniqueCatalog.IsUniqueWeapon(id)) return;
            FusionCatalog.UnbindWeapon(id, this, passives);
            Owned.Remove(id);
            switch (id)
            {
                case WeaponId.Orbit:
                    var orbit = GetComponent<OrbitWeapon>();
                    if (orbit != null) DestroyImmediate(orbit);
                    break;
                case WeaponId.Nova:
                    var nova = GetComponent<NovaWeapon>();
                    if (nova != null) DestroyImmediate(nova);
                    break;
                case WeaponId.Frost:
                    var frost = GetComponent<FrostAura>();
                    if (frost != null) DestroyImmediate(frost);
                    break;
                case WeaponId.Lash:
                    var lash = GetComponent<ChainLash>();
                    if (lash != null) DestroyImmediate(lash);
                    break;
                default:
                    if (_arms.TryGetValue(id, out var arm))
                    {
                        _arms.Remove(id);
                        if (arm != null) DestroyImmediate(arm);
                    }
                    break;
            }
        }

        bool Attach(WeaponId id)
        {
            if (!WeaponCatalog.Unlocked(id)) return false;
            switch (id)
            {
                case WeaponId.Orbit:
                    var orbit = GetComponent<OrbitWeapon>() ?? gameObject.AddComponent<OrbitWeapon>();
                    orbit.Level = 1;
                    orbit.Fangs = 2;
                    orbit.Rebuild();
                    break;
                case WeaponId.Nova:
                    var nova = GetComponent<NovaWeapon>() ?? gameObject.AddComponent<NovaWeapon>();
                    nova.Level = 1;
                    break;
                case WeaponId.Frost:
                    var frost = GetComponent<FrostAura>() ?? gameObject.AddComponent<FrostAura>();
                    frost.Level = 1;
                    break;
                case WeaponId.Lash:
                    var lash = GetComponent<ChainLash>() ?? gameObject.AddComponent<ChainLash>();
                    lash.Level = 1;
                    break;
                default:
                    if (_arms.TryGetValue(id, out var old) && old != null) DestroyImmediate(old);
                    var arm = gameObject.AddComponent<GenericArm>();
                    arm.Bind(id);
                    _arms[id] = arm;
                    break;
            }

            Owned.Add(id);
            return true;
        }

        public void CollectCommons(System.Collections.Generic.List<WeaponId> dest)
        {
            dest.Clear();
            foreach (var id in Owned)
                if (!UniqueCatalog.IsUniqueWeapon(id)) dest.Add(id);
        }

        public void Upgrade(WeaponId id)
        {
            SetLevel(id, Mathf.Min(5, LevelOf(id) + 1));
        }

        public int LevelOf(WeaponId id)
        {
            switch (id)
            {
                case WeaponId.Needle: return Needle != null ? Needle.Level : 0;
                case WeaponId.Orbit: return GetComponent<OrbitWeapon>()?.Level ?? 0;
                case WeaponId.Nova: return GetComponent<NovaWeapon>()?.Level ?? 0;
                case WeaponId.Frost: return GetComponent<FrostAura>()?.Level ?? 0;
                case WeaponId.Lash: return GetComponent<ChainLash>()?.Level ?? 0;
                case WeaponId.Divine: return GetComponent<LightningWeapon>()?.Level ?? 0;
                case WeaponId.Blade: return GetComponent<TemorisSword>()?.Level ?? 0;
                case WeaponId.Spiral: return GetComponent<SpiralHuntWeapon>()?.Level ?? 0;
                case WeaponId.Star: return GetComponent<StarfallInstrument>()?.Level ?? 0;
                default:
                    return _arms.TryGetValue(id, out var arm) && arm != null ? arm.Level : 0;
            }
        }

        public string Summary()
        {
            var parts = new System.Collections.Generic.List<string>();
            foreach (var id in Owned)
                parts.Add($"{WeaponCatalog.Title(id)}{LevelOf(id)}");
            return string.Join("  ", parts);
        }
    }
}
