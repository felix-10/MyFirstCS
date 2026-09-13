using UnityEngine;

namespace Veinfire
{
    public sealed class GenericArm : MonoBehaviour
    {
        public WeaponId Id;
        public int Level = 1;
        float _cd;
        PlayerCombatStats _stats;
        PlayerHealth _hp;
        Projectile _prefab;
        ArmHalo _halo;

        public void Bind(WeaponId id)
        {
            Id = id;
            Level = 1;
            _stats = GetComponent<PlayerCombatStats>();
            _hp = GetComponent<PlayerHealth>();
            var needle = GetComponent<AutoAimWeapon>();
            if (needle != null) _prefab = needle.ProjectilePrefab;
            if (id == WeaponId.DarkOrbit || id == WeaponId.Gravity)
                EnsureHalo();
        }

        void OnDestroy()
        {
            if (_halo != null) Destroy(_halo.gameObject);
        }

        void Update()
        {
            if (GameSession.IsPaused || _hp != null && _hp.IsDead) return;
            if (Id == WeaponId.DarkOrbit || Id == WeaponId.Gravity)
            {
                EnsureHalo();
                return;
            }

            _cd -= Time.deltaTime;
            if (_cd > 0f) return;
            var interval = _stats != null ? _stats.ScaledInterval(BaseInterval()) : BaseInterval();
            _cd = interval;
            Fire();
        }

        float BaseInterval()
        {
            switch (Id)
            {
                case WeaponId.Grenade: return 1.05f;
                case WeaponId.ShadowBlade: return 0.58f;
                case WeaponId.FlameRing: return 0.82f;
                case WeaponId.SpikeFan: return Mathf.Max(0.28f, 0.52f - Level * 0.03f);
                case WeaponId.PoisonFog: return 2.4f;
                case WeaponId.HolyAura: return 0.42f;
                case WeaponId.IonJet: return 0.22f;
                case WeaponId.Aurora: return 0.85f;
                case WeaponId.Spear: return 0.9f;
                case WeaponId.StarBurst: return 1.55f;
                case WeaponId.IceDisc: return 0.72f;
                case WeaponId.Totem: return 2.3f;
                case WeaponId.Homing: return 0.62f;
                case WeaponId.Shockwave: return 1.15f;
                case WeaponId.Spore: return 1.05f;
                case WeaponId.ChainBall: return 0.95f;
                case WeaponId.Refract: return 0.7f;
                case WeaponId.ChaosShot: return 0.38f;
                case WeaponId.MoonKnife: return 0.88f;
                case WeaponId.Magma: return 0.72f;
                case WeaponId.Thorns: return Mathf.Max(0.28f, 0.78f - Level * 0.08f);
                case WeaponId.ArrowRain: return 1.4f;
                case WeaponId.Drain: return 0.48f;
                case WeaponId.Hammer: return 1.32f;
                default: return 0.65f;
            }
        }

        void Fire()
        {
            var origin = transform.position + Vector3.up * 0.45f;
            var area = _stats != null ? _stats.Area : 1f;
            var dmg = (_stats != null ? _stats.ScaledDamage : 12f) * (0.55f + 0.08f * Level);
            var knock = _stats != null ? _stats.Knockback : 2f;
            var dir = AimDir(origin);
            switch (Id)
            {
                case WeaponId.Shockwave:
                    var shockR = (2.3f + Level * 0.45f) * area * (FusionCatalog.On("lash_shock") ? 1.28f : 1f);
                    Blast(origin, shockR, dmg, knock + 2.4f + Level * 0.5f);
                    ShotFx.Shock(origin, new Color(0.85f, 0.85f, 0.9f), shockR * 2f);
                    break;
                case WeaponId.HolyAura:
                    Blast(origin, (1.7f + Level * 0.28f) * area, dmg * (0.4f + Level * 0.06f), 0.3f);
                    if (_stats != null)
                    {
                        var stack = 0.12f + Level * 0.04f;
                        if (FusionCatalog.On("holy_hammer") || FusionCatalog.On("holy_aurora")) stack *= 1.35f;
                        _stats.Armor = Mathf.Min(_stats.Armor + stack, 14f);
                    }
                    ShotFx.Impact(Id, origin);
                    break;
                case WeaponId.PoisonFog:
                    PlantFog(origin, area, dmg);
                    break;
                case WeaponId.Grenade:
                    var nade = SpawnShot(origin, dir, dmg, 6.2f);
                    if (nade != null)
                    {
                        nade.Duration = 0.5f;
                        nade.ExplodeRadius = (1.7f + Level * 0.35f) * area;
                        if (FusionCatalog.On("nade_magma") || FusionCatalog.On("totem_nade"))
                        {
                            nade.ExplodeRadius *= 1.22f;
                            nade.IgniteTime = 1.6f;
                            nade.Kind = HitKind.Burn;
                        }
                        nade.HitRadius = 0.22f;
                        nade.Pierce = 0;
                        nade.ZoneTint = new Color(1f, 0.55f, 0.15f, 0.5f);
                        if (!FusionCatalog.On("nade_magma") && !FusionCatalog.On("totem_nade")) nade.Kind = HitKind.Normal;
                        nade.Begin();
                    }
                    break;
                case WeaponId.ShadowBlade:
                    for (var i = 0; i < 1 + Level / 3; i++)
                    {
                        var yaw = (i - Level / 6f) * 10f;
                        var shot = SpawnShot(origin, Quaternion.Euler(0f, yaw, 0f) * dir, dmg * 0.9f, 13f + Level);
                        if (shot == null) continue;
                        shot.Pierce = 2 + Level;
                        if (FusionCatalog.On("shadow_dark")) shot.SlowTime = 0.85f;
                        shot.ReturnShot = true;
                        shot.ReturnAfter = 6.5f + Level * 0.4f;
                        shot.Duration = 1.8f;
                        shot.Begin();
                    }
                    break;
                case WeaponId.FlameRing:
                    var ring = SpawnShot(origin, dir, dmg, 8f);
                    if (ring != null)
                    {
                        ring.ReturnShot = true;
                        ring.ReturnAfter = 4.2f + Level * 0.35f;
                        ring.IgniteTime = (1.4f + Level * 0.25f) * (FusionCatalog.On("flame_magma") ? 1.45f : 1f);
                        ring.HitRadius = (0.7f + Level * 0.08f) * (FusionCatalog.On("flame_orbit") ? 1.25f : 1f);
                        ring.Duration = 1.7f;
                        ring.Kind = HitKind.Burn;
                        ring.Begin();
                    }
                    break;
                case WeaponId.SpikeFan:
                    ShootFan(origin, dir, dmg * 0.55f, 4 + Level * 2 + (FusionCatalog.On("spike_arrow") ? 3 : 0), 52f + Level * 6f, 7f, 0.32f,
                        FusionCatalog.On("spike_spear") ? 3 : 0);
                    break;
                case WeaponId.IonJet:
                    Jet(origin, dir, (3.6f + Level * 0.55f) * (FusionCatalog.On("ion_refract") ? 1.25f : 1f),
                        (0.42f + Level * 0.16f) * (FusionCatalog.On("ion_chain") ? 1.2f : 1f), dmg * 0.55f);
                    break;
                case WeaponId.Aurora:
                    Aurora(origin, dir, 7.4f + Level * 0.7f,
                        (0.7f + Level * 0.22f) * (FusionCatalog.On("ice_aurora") || FusionCatalog.On("holy_aurora") ? 1.25f : 1f), dmg * 0.8f);
                    break;
                case WeaponId.Spear:
                    var spears = Level >= 4 ? 2 : 1;
                    for (var i = 0; i < spears; i++)
                    {
                        var yaw = spears == 1 ? 0f : (i == 0 ? -6f : 6f);
                        var spear = SpawnShot(origin, Quaternion.Euler(0f, yaw, 0f) * dir, dmg * 1.15f, 18f + Level * 1.4f);
                        if (spear == null) continue;
                        spear.Pierce = 3 + Level + (FusionCatalog.On("spike_spear") ? 2 : 0);
                        spear.Duration = 1.05f;
                        spear.HitRadius = 0.38f;
                        spear.Begin();
                    }
                    break;
                case WeaponId.StarBurst:
                    Radial(origin, dmg * 0.45f, 6 + Level * 2 + (FusionCatalog.On("star_arrow") ? 4 : 0) + (FusionCatalog.On("star_chaos") ? 2 : 0), 9f, 0.7f);
                    break;
                case WeaponId.IceDisc:
                    var disc = SpawnShot(origin, dir, dmg, 10f);
                    if (disc != null)
                    {
                        disc.SplitOnHit = true;
                        disc.SplitCount = 2 + Level;
                        disc.SlowTime = (0.7f + Level * 0.12f) * (FusionCatalog.On("ice_aurora") || FusionCatalog.On("frost_ice") ? 1.6f : 1f);
                        disc.SlowMul = 0.7f;
                        disc.Pierce = 1;
                        disc.Kind = HitKind.Ice;
                        disc.Duration = 1.2f;
                        disc.Begin();
                    }
                    break;
                case WeaponId.Totem:
                    SummonTotem(origin, dmg, 3.1f + Level * 0.7f);
                    break;
                case WeaponId.Homing:
                    var missiles = (Level >= 4 ? 2 : 1) + (FusionCatalog.On("homing_moon") ? 1 : 0);
                    for (var i = 0; i < missiles; i++)
                    {
                        var yaw = missiles == 1 ? 0f : (i == 0 ? -18f : 18f);
                        var bolt = SpawnShot(origin, Quaternion.Euler(0f, yaw, 0f) * dir, dmg * 0.85f, 9f);
                        if (bolt == null) continue;
                        bolt.HomingTurn = 160f + Level * 40f;
                        bolt.Duration = 1.6f;
                        bolt.Pierce = Level >= 5 ? 1 : 0;
                        bolt.Begin();
                    }
                    break;
                case WeaponId.Spore:
                    var spore = SpawnShot(origin, dir, dmg * 0.7f, 8.5f);
                    if (spore != null)
                    {
                        spore.ExplodeRadius = 1.5f * area;
                        spore.ZoneLife = (2.2f + Level * 0.55f) * (_stats != null ? _stats.Duration : 1f)
                                         * (FusionCatalog.On("fog_spore") ? 1.35f : 1f);
                        spore.Kind = HitKind.Plague;
                        spore.ZoneTint = new Color(0.4f, 0.85f, 0.2f, 0.5f);
                        spore.Duration = 0.7f;
                        spore.Begin();
                    }
                    break;
                case WeaponId.ChainBall:
                    ChainLightning(origin, dmg);
                    break;
                case WeaponId.Refract:
                    var beam = SpawnShot(origin, dir, dmg * 0.9f, 16f);
                    if (beam != null)
                    {
                        beam.WallBounce = 1 + Level + (FusionCatalog.On("ion_refract") ? 2 : 0);
                        beam.Pierce = 1 + Level / 2;
                        beam.Duration = 1.4f;
                        beam.HitRadius = 0.28f;
                        beam.Begin();
                    }
                    break;
                case WeaponId.ChaosShot:
                    var n = (Level >= 5 ? 2 : 1) + (FusionCatalog.On("drain_chaos") || FusionCatalog.On("star_chaos") ? 1 : 0);
                    for (var i = 0; i < n; i++)
                    {
                        var chaos = SpawnShot(origin, Quaternion.Euler(0f, Random.Range(-22f, 22f), 0f) * dir, dmg, 10f + Random.Range(-2f, 3f));
                        if (chaos == null) continue;
                        chaos.Damage *= Random.Range(0.7f, 1.3f);
                        chaos.Helix = 0.45f + Level * 0.12f;
                        chaos.HelixSpin = 280f + Random.Range(0f, 220f);
                        chaos.Kind = HitKind.Chaos;
                        chaos.Duration = 1.1f;
                        chaos.Begin();
                    }
                    break;
                case WeaponId.MoonKnife:
                    Radial(origin, dmg * 0.7f, 4 + Level + (FusionCatalog.On("homing_moon") ? 3 : 0), 12f, 1.05f);
                    break;
                case WeaponId.Magma:
                    Cone(origin, dir, 3.4f + Level * 0.5f, (26f + Level * 7f) * (FusionCatalog.On("flame_magma") ? 1.3f : 1f), dmg, knock, true);
                    break;
                case WeaponId.Thorns:
                    ThornPatch(dmg, area);
                    break;
                case WeaponId.ArrowRain:
                    Rain(dmg, area);
                    break;
                case WeaponId.Drain:
                    Drain(dmg);
                    break;
                case WeaponId.Hammer:
                    Hammers(dmg, area);
                    break;
                default:
                    ShootFan(origin, dir, dmg, 1, 0f, 12f, 1.1f, 0);
                    break;
            }
        }

        Vector3 AimDir(Vector3 origin)
        {
            var target = CombatUtil.UniqueTarget(origin, 22f);
            var dir = target != null ? World.Planar(origin, target.position) : transform.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.Normalize();
            return dir;
        }

        void Blast(Vector3 origin, float radius, float dmg, float knock)
        {
            CombatUtil.DamageEnemiesInRadius(origin, radius, dmg, knock, _hp, _stats != null ? _stats.LifeSteal : 0f, _stats);
        }

        void PlantFog(Vector3 origin, float area, float dmg)
        {
            var zone = new GameObject("PoisonFog");
            zone.transform.position = origin;
            var field = zone.AddComponent<StrikeZone>();
            field.Radius = 1.35f * area;
            field.GrowPerSec = 0.55f + Level * 0.14f;
            field.TickDamage = dmg * 0.38f;
            field.Interval = 0.4f;
            field.Life = (2.4f + Level * 0.45f) * (_stats != null ? _stats.Duration : 1f)
                         * (FusionCatalog.On("fog_spore") ? 1.35f : 1f);
            field.Follow = transform;
            field.Vein = true;
            field.Plague = true;
            field.OwnerHealth = _hp;
            field.OwnerStats = _stats;
            field.Tint = new Color(0.35f, 0.9f, 0.22f, 0.45f);
        }

        void Jet(Vector3 origin, Vector3 dir, float length, float width, float dmg)
        {
            Beam(origin, dir, length, width, dmg, HitKind.Shock, 0f, 0.05f);
            var n = 3 + Level / 2;
            for (var i = 0; i < n; i++)
            {
                var yaw = Random.Range(-12f, 12f);
                var shot = Quaternion.Euler(0f, yaw, 0f) * dir;
                var tip = origin + shot * (length * Random.Range(0.55f, 1f)) + Vector3.up * Random.Range(0.1f, 0.35f);
                ShotFx.Lightning(origin + Vector3.up * 0.2f, tip, new Color(0.7f, 0.9f, 1f), 0.07f, 5, 0.07f);
            }
        }

        void Aurora(Vector3 origin, Vector3 dir, float length, float width, float dmg)
        {
            Beam(origin, dir, length, width, dmg, HitKind.Ice, 0.55f, 0.18f);
            var rot = Quaternion.LookRotation(dir, Vector3.up);
            var sheets = 3 + Level / 2;
            for (var i = 0; i < sheets; i++)
            {
                var t = (i + 1f) / (sheets + 1f);
                var at = origin + dir * (length * t) + Vector3.up * 0.8f;
                ShotFx.Flash("Aurora", at, new Color(0.35f, 1f, 0.75f, 0.45f),
                    new Vector3(width * (0.6f + t * 0.8f), 1.6f + Level * 0.15f, 0.08f),
                    rot, 0.2f, PrimitiveType.Cube);
            }
        }

        readonly System.Collections.Generic.List<EnemyHealth> _chain = new System.Collections.Generic.List<EnemyHealth>();

        void ChainLightning(Vector3 origin, float dmg)
        {
            var hops = 2 + Level + (FusionCatalog.On("ion_chain") ? 3 : 0);
            if (CombatUtil.GatherChain(origin, 15f, 5.4f + Level * 0.4f, hops, _chain) <= 0) return;
            var from = origin + Vector3.up * 0.9f;
            var white = Color.white;
            var glow = new Color(0.72f, 0.88f, 1f);
            for (var i = 0; i < _chain.Count; i++)
            {
                var enemy = _chain[i];
                if (enemy == null) continue;
                var to = enemy.transform.position + Vector3.up * 0.95f;
                ShotFx.Lightning(from, to, i == 0 ? white : glow, 0.12f, 8, 0.17f);
                var crit = false;
                var hit = _stats != null ? _stats.RollHit(dmg * (1f - i * 0.07f), out crit) : dmg;
                enemy.Damage(hit, origin, 2.2f, crit, true, HitKind.Shock);
                if (_stats != null && _stats.LifeSteal > 0f) _hp?.Heal(_stats.LifeSteal);
                from = to;
            }

            GameFeel.Shoot(origin);
        }

        void Beam(Vector3 origin, Vector3 dir, float length, float width, float dmg, HitKind kind, float slow, float life)
        {
            var mid = origin + dir * (length * 0.5f);
            var rot = Quaternion.LookRotation(dir, Vector3.up);
            var hits = Physics.OverlapBox(mid, new Vector3(width * 0.5f, 0.8f, length * 0.5f), rot, ~0, QueryTriggerInteraction.Collide);
            var seen = new System.Collections.Generic.HashSet<int>();
            for (var i = 0; i < hits.Length; i++)
            {
                var enemy = hits[i].GetComponentInParent<EnemyHealth>();
                if (enemy == null || !seen.Add(enemy.GetInstanceID())) continue;
                var crit = false;
                var hit = _stats != null ? _stats.RollHit(dmg, out crit) : dmg;
                enemy.Damage(hit, origin, 1.2f, crit, true, kind, false, false);
                if (slow > 0f) enemy.ApplySlow(0.62f, slow);
                if (kind == HitKind.Burn) enemy.Ignite(1.2f);
            }

            var color = kind == HitKind.Ice ? new Color(0.55f, 0.92f, 1f) : new Color(0.35f, 0.78f, 1f);
            ShotFx.Flash("Beam", mid, color, new Vector3(width, 0.12f, length), rot, life, PrimitiveType.Cube);
        }

        void Cone(Vector3 origin, Vector3 dir, float range, float angle, float dmg, float knock, bool burn)
        {
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null) continue;
                var to = World.Planar(origin, enemy.transform.position);
                if (to.magnitude > range) continue;
                if (Vector3.Angle(dir, to) > angle) continue;
                var crit = false;
                var hit = _stats != null ? _stats.RollHit(dmg, out crit) : dmg;
                enemy.Damage(hit, origin, knock, crit, true, HitKind.Burn, false, false);
                if (burn) enemy.Ignite(1.5f + Level * 0.2f);
            }

            ShotFx.Flash("Magma", origin + dir * (range * 0.45f), new Color(1f, 0.4f, 0.08f),
                new Vector3(range * Mathf.Tan(angle * Mathf.Deg2Rad) * 1.4f, 0.14f, range * 0.9f),
                Quaternion.LookRotation(dir, Vector3.up), 0.14f, PrimitiveType.Cube);
        }

        void ShootFan(Vector3 origin, Vector3 dir, float dmg, int count, float spread, float speed, float life, int pierce)
        {
            var n = Mathf.Max(1, count);
            for (var i = 0; i < n; i++)
            {
                var yaw = n == 1 ? 0f : Mathf.Lerp(-spread * 0.5f, spread * 0.5f, i / (n - 1f));
                var shot = SpawnShot(origin, Quaternion.Euler(0f, yaw, 0f) * dir, dmg, speed);
                if (shot == null) continue;
                shot.Duration = life;
                shot.Pierce = pierce;
                shot.Begin();
            }
        }

        void Radial(Vector3 origin, float dmg, int count, float speed, float life)
        {
            var n = Mathf.Max(3, count);
            for (var i = 0; i < n; i++)
            {
                var dir = Quaternion.Euler(0f, i * (360f / n), 0f) * Vector3.forward;
                var shot = SpawnShot(origin, dir, dmg, speed);
                if (shot == null) continue;
                shot.Duration = life;
                shot.Begin();
            }
        }

        void ThornPatch(float dmg, float area)
        {
            var t = CombatUtil.UniqueTarget(transform.position, 10f);
            var at = t != null ? t.position : transform.position + transform.forward * 2.2f;
            at.y = transform.position.y;
            var patches = 1 + Level / 2 + (FusionCatalog.On("thorns_gravity") ? 1 : 0);
            for (var i = 0; i < patches; i++)
            {
                var p = at + Quaternion.Euler(0f, i * (360f / patches), 0f) * Vector3.forward * (i == 0 ? 0f : 1.1f);
                Blast(p, 1.15f * area, dmg, 1.4f);
                ShotFx.Impact(WeaponId.Thorns, p);
            }
        }

        void Rain(float dmg, float area)
        {
            var t = CombatUtil.UniqueTarget(transform.position, 16f);
            if (t == null) return;
            var center = t.position;
            center.y = transform.position.y;
            var n = 4 + Level * 2 + (FusionCatalog.On("star_arrow") || FusionCatalog.On("spike_arrow") ? 4 : 0);
            var delay = new GameObject("ArrowRain");
            delay.transform.position = center;
            var rain = delay.AddComponent<ArmRain>();
            rain.Damage = dmg;
            rain.Count = n;
            rain.Radius = 1.8f * area;
            rain.Owner = this;
            rain.Delay = 0.35f;
        }

        void Drain(float dmg)
        {
            var range = 8.5f + Level * 0.8f;
            var t = CombatUtil.UniqueTarget(transform.position, range);
            if (t == null) return;
            var enemy = t.GetComponent<EnemyHealth>();
            if (enemy == null) return;
            var ticks = 1 + Level / 2;
            var dealt = dmg * (0.7f + Level * 0.08f);
            enemy.Damage(dealt * ticks, transform.position, 0.4f, false, true, HitKind.Chaos);
            enemy.PullToward(transform.position, 1.1f + Level * 0.25f);
            var steal = FusionCatalog.On("drain_chaos") ? 1.6f : 1f;
            _hp?.Heal(Mathf.Min(3f + Level, dealt * 0.14f * ticks * steal), false);
            var along = t.position - transform.position;
            along.y = 0f;
            var mid = (transform.position + t.position) * 0.5f + Vector3.up * 0.5f;
            var rot = along.sqrMagnitude > 0.01f ? Quaternion.LookRotation(along, Vector3.up) : Quaternion.identity;
            ShotFx.Flash("Drain", mid, new Color(0.85f, 0.2f, 0.55f), new Vector3(0.1f + Level * 0.02f, 0.1f, along.magnitude), rot, 0.14f, PrimitiveType.Cube);
        }

        void Hammers(float dmg, float area)
        {
            var n = 1 + Level / 2 + (FusionCatalog.On("holy_hammer") ? 1 : 0);
            var used = new System.Collections.Generic.HashSet<int>();
            for (var i = 0; i < n; i++)
            {
                Transform t = null;
                if (i == 0) t = CombatUtil.UniqueTarget(transform.position, 16f);
                else
                {
                    var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
                    var best = float.MaxValue;
                    for (var e = 0; e < enemies.Length; e++)
                    {
                        if (enemies[e] == null || !used.Add(enemies[e].GetInstanceID())) continue;
                        var d = World.Planar(transform.position, enemies[e].transform.position).sqrMagnitude;
                        if (d > 280f || d >= best) continue;
                        best = d;
                        t = enemies[e].transform;
                    }
                }

                if (t == null) break;
                used.Add(t.GetInstanceID());
                var at = t.position;
                at.y = transform.position.y;
                Blast(at, 1.55f * area, dmg * (1f - i * 0.12f), 2.4f);
                var gold = new Color(1f, 0.9f, 0.4f);
                var hammer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hammer.name = "Hammer";
                hammer.transform.position = at + Vector3.up * 8f;
                hammer.transform.localScale = new Vector3(0.7f, 1.4f, 0.45f);
                PrimitiveFactory.Paint(hammer, gold);
                var col = hammer.GetComponent<Collider>();
                if (col != null) Destroy(col);
                var fall = hammer.AddComponent<VfxFall>();
                fall.Target = at + Vector3.up * 0.6f;
                fall.Duration = 0.16f + i * 0.05f;
                ShotFx.Impact(WeaponId.Hammer, at);
            }
        }

        void SummonTotem(Vector3 origin, float dmg, float life)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Totem";
            go.transform.position = origin + transform.forward * 1.4f + Vector3.up * 0.4f;
            go.transform.localScale = new Vector3(0.55f, 0.7f, 0.55f);
            PrimitiveFactory.Paint(go, new Color(0.75f, 0.45f, 0.2f));
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);
            var totem = go.AddComponent<ArmTotem>();
            totem.Arm = this;
            totem.Damage = dmg * 0.55f;
            totem.Life = life * (_stats != null ? _stats.Duration : 1f) * (FusionCatalog.On("totem_nade") || FusionCatalog.On("totem_shock") ? 1.2f : 1f);
        }

        void EnsureHalo()
        {
            if (_halo != null)
            {
                _halo.Level = Level;
                return;
            }

            var go = new GameObject("ArmHalo");
            go.transform.SetParent(transform, false);
            _halo = go.AddComponent<ArmHalo>();
            _halo.Arm = this;
            _halo.Id = Id;
            _halo.Level = Level;
            _halo.Rebuild();
        }

        public Projectile SpawnShot(Vector3 origin, Vector3 dir, float dmg, float speed)
        {
            if (_prefab == null)
            {
                var needle = GetComponent<AutoAimWeapon>();
                if (needle != null) _prefab = needle.ProjectilePrefab;
            }
            if (_prefab == null) return null;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.Normalize();
            var go = Instantiate(_prefab, origin, Quaternion.LookRotation(dir, Vector3.up));
            go.gameObject.SetActive(true);
            var p = go.GetComponent<Projectile>();
            if (p == null) return null;
            p.Damage = dmg;
            p.Knockback = _stats != null ? _stats.Knockback : 2f;
            p.Pierce = _stats != null ? _stats.Pierce : 0;
            p.Velocity = dir * speed;
            p.Axis = dir;
            p.OwnerStats = _stats;
            p.OwnerHealth = _hp;
            p.LifeSteal = _stats != null ? _stats.LifeSteal : 0f;
            p.IsBullet = true;
            p.ProcVein = true;
            p.Helix = 0f;
            p.ReturnShot = false;
            p.HomingTurn = 0f;
            p.Bounces = 0;
            p.WallBounce = 0;
            p.ExplodeRadius = 0f;
            p.ZoneLife = 0f;
            p.SplitOnHit = false;
            p.IgniteTime = 0f;
            p.SlowTime = 0f;
            p.Pull = 0f;
            p.Duration = 1.1f;
            p.Kind = HitKind.Normal;
            var size = _stats != null ? _stats.ProjectileSize * _stats.Area : 1f;
            ShotFx.ForWeapon(Id, go.gameObject, size);
            return p;
        }
    }

    public sealed class ArmHalo : MonoBehaviour
    {
        public GenericArm Arm;
        public WeaponId Id;
        public int Level = 1;
        int _built = -1;
        float _angle;
        float _pull;

        public void Rebuild()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            var n = Id == WeaponId.Gravity ? 3 + Level : 2 + Level;
            if (Id == WeaponId.DarkOrbit && FusionCatalog.On("shadow_dark")) n += 1;
            var radius = (1.9f + Level * 0.22f);
            var color = Id == WeaponId.Gravity ? new Color(0.62f, 0.4f, 1f) : new Color(0.4f, 0.16f, 0.75f);
            for (var i = 0; i < n; i++)
            {
                var bit = PrimitiveFactory.Sphere("HaloBit", color, 0.28f, true);
                bit.transform.SetParent(transform, false);
                PrimitiveFactory.StripRigidbody(bit);
                bit.transform.localScale = Id == WeaponId.Gravity
                    ? new Vector3(0.22f, 0.22f, 0.22f)
                    : new Vector3(0.16f, 0.08f, 0.48f);
                var t = (Mathf.PI * 2f) * i / n;
                bit.transform.localPosition = new Vector3(Mathf.Cos(t) * radius, 0.7f, Mathf.Sin(t) * radius);
                bit.AddComponent<ArmHaloHit>().Halo = this;
            }

            _built = Level;
        }

        void Update()
        {
            if (GameSession.IsPaused || Arm == null) return;
            if (_built != Level) Rebuild();
            _angle += (70f + Level * 16f) * Time.deltaTime;
            transform.localRotation = Quaternion.Euler(0f, _angle, 0f);
            if (Id != WeaponId.Gravity) return;
            _pull -= Time.deltaTime;
            if (_pull > 0f) return;
            _pull = 0.35f;
            var origin = Arm.transform.position;
            var r = 3.4f + Level * 0.4f;
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null) continue;
                if (World.Planar(origin, enemies[i].transform.position).sqrMagnitude > r * r) continue;
                enemies[i].PullToward(origin, (1.15f + Level * 0.2f) * (FusionCatalog.On("thorns_gravity") ? 1.45f : 1f));
            }
        }

        public float HitDamage
        {
            get
            {
                var stats = Arm.GetComponent<PlayerCombatStats>();
                return (stats != null ? stats.ScaledDamage : 12f) * (0.4f + Level * 0.1f);
            }
        }
    }

    public sealed class ArmHaloHit : MonoBehaviour
    {
        public ArmHalo Halo;

        void OnTriggerStay(Collider other)
        {
            if (GameSession.IsPaused || Halo == null || Halo.Arm == null) return;
            var enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy == null || !enemy.TryDot(0.22f)) return;
            var stats = Halo.Arm.GetComponent<PlayerCombatStats>();
            var crit = false;
            var dmg = stats != null ? stats.RollHit(Halo.HitDamage, out crit) : Halo.HitDamage;
            enemy.Damage(dmg, transform.position, 1.1f, crit, true, Halo.Id == WeaponId.DarkOrbit ? HitKind.Chaos : HitKind.Normal);
            if (Halo.Id == WeaponId.DarkOrbit) enemy.ApplySlow(0.82f, 0.4f);
        }
    }

    public sealed class ArmTotem : MonoBehaviour
    {
        public GenericArm Arm;
        public float Damage;
        public float Life = 3f;
        float _cd;

        void Update()
        {
            if (GameSession.IsPaused) return;
            Life -= Time.deltaTime;
            if (Life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            _cd -= Time.deltaTime;
            if (_cd > 0f || Arm == null) return;
            _cd = FusionCatalog.On("totem_nade") ? 0.55f : 0.7f;
            var radius = FusionCatalog.On("totem_nade") ? 3.4f : FusionCatalog.On("totem_shock") ? 3.6f : 2.3f;
            var knock = FusionCatalog.On("totem_shock") ? 6.2f : 3.2f;
            CombatUtil.DamageEnemiesInRadius(transform.position, radius, Damage, knock);
            var tint = FusionCatalog.On("totem_nade") ? new Color(1f, 0.35f, 0.08f) : new Color(1f, 0.45f, 0.15f);
            ShotFx.Shock(transform.position, tint, radius * 1.8f);
        }
    }

    public sealed class ArmRain : MonoBehaviour
    {
        public GenericArm Owner;
        public float Damage;
        public int Count = 6;
        public float Radius = 1.8f;
        public float Delay = 0.35f;

        void Update()
        {
            if (GameSession.IsPaused) return;
            Delay -= Time.deltaTime;
            if (Delay > 0f) return;
            var stats = Owner != null ? Owner.GetComponent<PlayerCombatStats>() : null;
            var hp = Owner != null ? Owner.GetComponent<PlayerHealth>() : null;
            for (var i = 0; i < Count; i++)
            {
                var yaw = i * (360f / Count);
                var at = transform.position + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * (Radius * (0.2f + (i % 3) * 0.25f));
                CombatUtil.DamageEnemiesInRadius(at, 0.7f, Damage * 0.45f, 1.2f, hp, stats != null ? stats.LifeSteal : 0f, stats);
                var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shaft.name = "Arrow";
                shaft.transform.position = at + Vector3.up * 8.5f;
                shaft.transform.localScale = new Vector3(0.07f, 1.8f, 0.07f);
                PrimitiveFactory.Paint(shaft, new Color(0.82f, 0.78f, 0.55f));
                var col = shaft.GetComponent<Collider>();
                if (col != null) Destroy(col);
                var fall = shaft.AddComponent<VfxFall>();
                fall.Target = at + Vector3.up * 0.2f;
                fall.Duration = 0.18f;
            }

            Destroy(gameObject);
        }
    }
}
