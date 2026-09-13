using UnityEngine;

namespace Veinfire
{
    public sealed class EnemyHealth : MonoBehaviour
    {
        public EnemyKind Kind = EnemyKind.Swarm;
        public float Armor;
        public float MaxHp = 24f;
        public int XpValue = 4;
        public float DropLuck = 0.08f;
        public XpOrb OrbPrefab;
        public Pickup PickupPrefab;
        public int BossIndex;
        public bool LeavePuddleOnDeath;

        public float Hp01 => MaxHp > 1f ? Mathf.Clamp01(_hp / MaxHp) : 1f;

        Rigidbody _body;
        HitFlash _flash;
        float _hp;
        float _slowUntil;
        float _slowMul = 1f;
        float _dotLock;
        float _dotGate;
        float _purgeAt;
        float _plagueUntil;
        float _plagueAcc;
        bool _dead;
        Vector3 _knock;
        float _burnUntil;
        float _burnDps;
        float _burnAcc;
        float _wetUntil;
        int _petrify;
        StatusFx _fx;
        bool _burnShown;
        bool _wetShown;
        float _markUntil;
        int _shred;
        float _shredUntil;
        bool _burnRefreshed;
        bool _wetRefreshed;
        float _shredGate;
        bool _burnBurst;
        bool _fusing;
        float _fuse;
        bool _phase2;
        float _bossAct;
        bool _summoned;
        bool _tankWaved;
        PlayerHealth _player;

        public float Current => _hp;
        public bool IsWet => Time.time < _wetUntil;
        public int PetrifyStacks => _petrify;

        public float MoveMul
        {
            get
            {
                if (_fusing) return 0f;
                var mul = Time.time < _slowUntil ? _slowMul : 1f;
                if (GameSession.Clock >= 240f && MaxHp > 0f && _hp / MaxHp <= 0.2f) mul *= 1.15f;
                if (Kind == EnemyKind.Elite && GameSession.Clock >= 180f) mul *= 1.08f;
                if (Kind == EnemyKind.Boss && BossIndex == 4) mul *= _phase2 ? 1.2f : 1.35f;
                return mul;
            }
        }

        public Vector3 ConsumeKnock()
        {
            var value = _knock;
            _knock *= 0.72f;
            if (_knock.sqrMagnitude < 0.05f) _knock = Vector3.zero;
            return value;
        }

        public void PullToward(Vector3 at, float amount)
        {
            var pull = World.Planar(transform.position, at);
            if (pull.sqrMagnitude < 0.01f) return;
            _knock += pull.normalized * amount;
        }

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _flash = GetComponent<HitFlash>();
            _hp = MaxHp;
        }

        WorldHealthBar _bar;

        void OnEnable()
        {
            if (!gameObject.activeInHierarchy) return;
            EnsureBar();
        }

        void OnDestroy()
        {
            if (_bar != null) Destroy(_bar.gameObject);
        }

        StatusFx Fx
        {
            get
            {
                if (_fx == null) _fx = StatusFx.Ensure(transform);
                return _fx;
            }
        }

        void EnsureBar()
        {
            if (_bar == null)
                _bar = WorldHealthBar.Attach(transform, new Color(0.85f, 0.18f, 0.18f), 1.85f, Kind == EnemyKind.Boss);
            _bar.Set(_hp > 0f ? _hp : MaxHp, Mathf.Max(1f, MaxHp));
        }

        public void Setup(float hp, int xp)
        {
            MaxHp = hp;
            _hp = hp;
            XpValue = xp;
            if (_bar != null)
            {
                Destroy(_bar.gameObject);
                _bar = null;
            }

            EnsureBar();
            if (Kind == EnemyKind.Boss && MapCatalog.HighStar)
            {
                _phase2 = true;
                _purgeAt = Time.time + 12f;
            }
        }

        public bool TryDot(float interval)
        {
            var wait = Mathf.Max(BalanceTables.DotLatch, interval);
            if (Time.time < _dotLock) return false;
            _dotLock = Time.time + wait;
            return true;
        }

        public void ClearDots()
        {
            _burnUntil = 0f;
            _burnDps = 0f;
            _plagueUntil = 0f;
            _wetUntil = 0f;
            _petrify = 0;
            _burnShown = _wetShown = false;
            if (_fx != null)
            {
                _fx.ShowBurn(false);
                _fx.ShowWet(false);
            }
        }

        public void Mark(float seconds)
        {
            _markUntil = Time.time + seconds;
            DamagePopup.SpawnTag(transform.position, "标记", HitKind.Crit);
        }

        public void ShredArmor()
        {
            if (Time.time < _shredGate) return;
            _shredGate = Time.time + 0.25f;
            if (Time.time > _shredUntil) _shred = 0;
            _shred = Mathf.Min(2, _shred + 1);
            _shredUntil = Time.time + 3f;
        }

        public void BoostBurn()
        {
            if (Time.time >= _burnUntil) return;
            _burnDps = 5.2f;
            _burnBurst = true;
        }

        public void RefreshVeinOnce()
        {
            if (Time.time < _burnUntil && !_burnRefreshed)
            {
                _burnRefreshed = true;
                var remain = Mathf.Max(0f, _burnUntil - Time.time);
                _burnUntil = Time.time + Mathf.Min(remain + VeinCatalog.BurnTime, VeinCatalog.BurnTime * 2f);
            }

            if (Time.time < _wetUntil && !_wetRefreshed)
            {
                _wetRefreshed = true;
                var remain = Mathf.Max(0f, _wetUntil - Time.time);
                _wetUntil = Time.time + Mathf.Min(remain + 1.8f, 3.6f);
            }
        }

        void Update()
        {
            if (_dead || GameSession.IsPaused) return;
            if (_player == null)
            {
                var motor = FindFirstObjectByType<PlayerMotor>();
                if (motor != null) _player = motor.GetComponent<PlayerHealth>();
            }

            var latch = BalanceTables.DotLatch;
            if (Time.time >= _dotGate)
            {
                _dotGate = Time.time + latch;
                var burning = Time.time < _burnUntil && _burnDps > 0f;
                if (burning && _burnAcc > 0f)
                {
                    var tick = _burnDps * _burnAcc;
                    _burnAcc = 0f;
                    Damage(tick, transform.position, 0f, false, false, HitKind.Burn);
                }

                if (Time.time < _plagueUntil && _plagueAcc > 0f)
                {
                    var tick = 4.2f * _plagueAcc;
                    _plagueAcc = 0f;
                    Damage(tick, transform.position, 0f, false, false, HitKind.Plague);
                }
            }

            var burningFx = Time.time < _burnUntil && _burnDps > 0f;
            if (burningFx)
                _burnAcc += Time.deltaTime;
            else if (_burnShown)
            {
                _burnShown = false;
                Fx.ShowBurn(false);
                if (_burnBurst || RunRules.HymnFire)
                    BurstBurn();
                _burnBurst = false;
            }

            if (Time.time < _plagueUntil)
                _plagueAcc += Time.deltaTime;

            var wet = Time.time < _wetUntil;
            if (!wet && _wetShown)
            {
                _wetShown = false;
                Fx.ShowWet(false);
            }

            if (!_fusing && Kind == EnemyKind.Exploder && GameSession.Clock >= 140f && MaxHp > 0f && _hp / MaxHp <= 0.3f)
            {
                _fusing = true;
                _fuse = 1.15f;
            }

            if (_fusing)
            {
                _fuse -= Time.deltaTime;
                if (_fuse <= 0f)
                {
                    Die();
                    return;
                }
            }

            if (Kind == EnemyKind.Boss && MapCatalog.HighStar)
            {
                if (_purgeAt <= 0f) _purgeAt = Time.time + 16f;
                if (Time.time >= _purgeAt)
                {
                    _purgeAt = Time.time + 16f;
                    ClearDots();
                    DamagePopup.SpawnTag(transform.position, "净化", HitKind.Holy);
                }
            }

            TickBoss();
        }

        void BurstBurn()
        {
            var dmg = 16f * 0.6f;
            var stats = FindFirstObjectByType<PlayerCombatStats>();
            if (stats != null) dmg = stats.Damage * 0.6f;
            CombatUtil.DamageEnemiesInRadius(transform.position, 2.2f, dmg, 1.4f);
            GameFeel.Pulse(transform.position + Vector3.up * 0.3f);
        }

        void TickBoss()
        {
            if (Kind != EnemyKind.Boss || BossIndex <= 0) return;
            if (!_phase2 && MaxHp > 0f && _hp / MaxHp <= 0.5f)
            {
                _phase2 = true;
                if (BossIndex == 3)
                {
                    EnemySpawner.SpawnSupport(EnemyKind.Runner, transform.position, 2);
                    EnemySpawner.SpawnSupport(EnemyKind.Elite, transform.position, 0);
                }
            }

            _bossAct -= Time.deltaTime;
            if (_bossAct > 0f) return;
            _bossAct = 3f;
            if (!_phase2) return;
            switch (BossIndex)
            {
                case 1:
                    EnemySpawner.SpawnSupport(EnemyKind.Swarm, transform.position, 1);
                    break;
                case 2:
                    if (_player != null && !_player.IsInvulnerable)
                    {
                        var d = World.Planar(transform.position, _player.transform.position);
                        if (d.magnitude < 8.5f && Vector3.Angle(transform.forward, d) < 55f)
                            _player.Damage(18f, true);
                    }
                    break;
                case 3:
                    if (_player != null)
                        MapHazard.SpawnPuddle(_player.transform.position + Random.insideUnitSphere * 6f, 3.2f, 12f, 3.2f);
                    break;
                case 4:
                    EnemySpawner.SpawnSupport(EnemyKind.Runner, transform.position, 1);
                    if (_player != null && !_player.IsInvulnerable
                        && World.Planar(transform.position, _player.transform.position).magnitude < 3.4f)
                        _player.Damage(8f);
                    break;
                case 5:
                    if (!_summoned)
                    {
                        _summoned = true;
                        EnemySpawner.SpawnSupport(EnemyKind.Exploder, transform.position, 2, true);
                    }
                    if (_player != null)
                        MapHazard.SpawnPuddle(_player.transform.position, 2.4f, 14f, 2.8f);
                    break;
            }
        }

        public void Ignite(float duration)
        {
            var fresh = Time.time >= _burnUntil;
            _burnDps = RunRules.HymnFire ? 5.2f : VeinCatalog.BurnDps;
            if (RunRules.HymnFire) _burnBurst = true;
            _burnUntil = Time.time + Mathf.Max(0.4f, duration);
            if (fresh) _burnRefreshed = false;
            _burnShown = true;
            Fx.ShowBurn(true);
            if (fresh) DamagePopup.SpawnTag(transform.position, "灼烧", HitKind.Burn);
        }

        public void ApplyWet(float duration)
        {
            var fresh = Time.time >= _wetUntil;
            _wetUntil = Time.time + Mathf.Max(0.4f, duration);
            if (fresh) _wetRefreshed = false;
            _wetShown = true;
            Fx.ShowWet(true);
            if (fresh) DamagePopup.SpawnTag(transform.position, "湿润", HitKind.Wet);
        }

        public void AddPetrify(float hitDamage, Vector3 from)
        {
            if (_dead) return;
            _petrify += 1;
            Fx.SetPetrify(_petrify);
            DamagePopup.SpawnTag(transform.position, $"石化 {_petrify}/3", HitKind.Petrify);
            if (_petrify < 3) return;
            _petrify = 0;
            Fx.SetPetrify(0);
            ApplySlow(0.4f, 1f);
            var burst = Mathf.Max(18f, hitDamage * 0.85f);
            var radius = 2f * (RunRules.BoltEarth ? 1.4f : 1f);
            if (RunConfig.RelicOn("石化棱镜")) radius *= 2f;
            CombatUtil.DamageEnemiesInRadius(transform.position, radius, burst, 2.4f);
        }

        public void ApplySlow(float multiplier, float duration)
        {
            _slowMul = Mathf.Min(_slowMul, multiplier);
            _slowUntil = Mathf.Max(_slowUntil, Time.time + duration);
        }

        public void Damage(float amount, Vector3 from, float knockback, bool crit = false, bool veinProc = true, HitKind kind = HitKind.Normal, bool melee = false, bool bullet = false)
        {
            if (_dead || amount <= 0f) return;
            if (veinProc)
            {
                var stats = FindFirstObjectByType<PlayerCombatStats>();
                if (stats != null) amount *= stats.HitMul(this);
            }
            if (Time.time < _markUntil) amount *= 1.12f;
            if (Kind == EnemyKind.Elite && GameSession.Clock >= 180f) amount *= 0.8f;
            if (Kind == EnemyKind.Boss && _phase2 && (BossIndex == 1 || BossIndex == 5)) amount *= 0.85f;

            var armor = Armor;
            if (Time.time < _shredUntil) armor = Mathf.Max(0f, armor - _shred * 4f);
            var resist = 20f / (20f + Mathf.Max(0f, armor));
            amount *= resist;
            RunConfig.DamageDealt += amount;
            _hp -= amount;
            if (_flash == null) _flash = gameObject.AddComponent<HitFlash>();
            if (kind != HitKind.Burn) _flash.Play();
            var shown = kind;
            if (crit && kind == HitKind.Normal) shown = HitKind.Crit;
            DamagePopup.Spawn(transform.position, amount, shown);
            _bar?.Set(_hp, MaxHp);

            var knock = knockback;
            var passives = FindFirstObjectByType<HeroPassives>();
            if (veinProc && passives != null) knock *= passives.KnockMul;
            if (_body != null && knock > 0f)
            {
                var push = World.Planar(from, transform.position);
                if (push.sqrMagnitude > 0.01f)
                    _knock += push.normalized * knock;
            }

            if (Kind == EnemyKind.Tank && GameSession.Clock >= 110f && !_tankWaved && amount > MaxHp * 0.15f)
            {
                _tankWaved = true;
                if (_player != null) _player.Damage(10f, true);
            }

            if (veinProc && _hp > 0f)
                VeinCatalog.OnPlayerHit(this, amount, from);
            if (veinProc && passives != null)
                passives.OnDealtHit(this, amount, crit, melee, bullet);
            if (veinProc && RunConfig.WeaponSplit && Random.value < 0.22f)
                CombatUtil.DamageEnemiesInRadius(transform.position, 2.4f, amount * 0.22f, 0.5f);

            var kill = _hp <= 0f;
            var pos = transform.position;
            if (kill) Die();
            if (kind == HitKind.Normal || kind == HitKind.Crit || kind == HitKind.Petrify)
                GameFeel.Hit(pos, amount, kill);
        }

        void Die()
        {
            if (_dead) return;
            _dead = true;
            MetaProgress.See(Kind);
            GameSession.RegisterKill(Kind, XpValue);
            FindFirstObjectByType<HeroPassives>()?.OnKilled(this);
            if (Kind == EnemyKind.Boss) InRunDirector.OnBossDead(transform.position);

            if (Time.time < _plagueUntil)
            {
                var other = CombatUtil.UniqueTarget(transform.position, 2.8f, this);
                other?.GetComponent<EnemyHealth>()?.ApplyPlague(8f, transform.position);
            }
            if (Kind == EnemyKind.Exploder)
                Explode();
            if (LeavePuddleOnDeath || RunConfig.HasCurse(CurseId.PlagueSpread) && Random.value < 0.35f)
                MapHazard.SpawnPuddle(transform.position, 2.8f, 12f, 2.6f);
            if (RunConfig.HasCurse(CurseId.DeathEcho))
            {
                var player = FindFirstObjectByType<PlayerHealth>();
                if (player != null && World.Planar(transform.position, player.transform.position).magnitude < 2.2f)
                    player.Damage(6f, true);
            }

            if (OrbPrefab != null)
            {
                var combo = GameSession.Combo;
                var amount = XpValue + (combo >= 8 ? 2 : 0);
                var orb = Instantiate(OrbPrefab, transform.position + Vector3.up * 0.35f, Quaternion.identity);
                orb.gameObject.SetActive(true);
                orb.Amount = amount;
            }

            TryDrop();
            Destroy(gameObject);
        }

        void Explode()
        {
            var player = FindFirstObjectByType<PlayerHealth>();
            if (player != null && World.Planar(transform.position, player.transform.position).magnitude < 2.8f)
                player.Damage(12f, true);

            CombatUtil.DamageEnemiesInRadius(transform.position, 2.4f, MaxHp * 0.25f, 5f);
            GameFeel.Explosion(transform.position);
            var burst = PrimitiveFactory.Sphere("Boom", new Color(1f, 0.4f, 0.1f), 0.5f, true);
            burst.transform.position = transform.position;
            burst.GetComponent<Rigidbody>().isKinematic = true;
            Destroy(burst, 0.2f);
        }

        void TryDrop()
        {
            if (PickupPrefab == null) return;
            var chance = DropLuck;
            if (RunConfig.HasCurse(CurseId.SparseLoot)) chance *= 0.55f;
            if (Kind == EnemyKind.Elite) chance = 1f;
            if (Kind == EnemyKind.Boss) chance = 1f;
            if (Random.value > chance) return;

            PickupKind kind;
            if (Kind == EnemyKind.Boss) kind = PickupKind.Chest;
            else if (Kind == EnemyKind.Elite) kind = Random.value < 0.5f ? PickupKind.Magnet : PickupKind.Health;
            else
            {
                var roll = Random.value;
                if (roll < 0.55f) kind = PickupKind.Health;
                else if (roll < 0.8f) kind = PickupKind.Magnet;
                else if (roll < 0.93f) kind = PickupKind.Bomb;
                else kind = PickupKind.Chest;
            }

            var drop = Instantiate(PickupPrefab, transform.position + Vector3.up * 0.4f, Quaternion.identity);
            drop.gameObject.SetActive(true);
            drop.Kind = kind;
            drop.ApplyLook();
        }

        public void ApplyCorrode(float duration)
        {
            Armor = Mathf.Max(0f, Armor - 2f);
            DamagePopup.SpawnTag(transform.position, "蚀", HitKind.Corrode);
            ApplySlow(0.92f, duration);
        }

        public void ApplyShock(float dealt, Vector3 from)
        {
            if (Random.value > 0.35f) return;
            var next = CombatUtil.UniqueTarget(transform.position, 5.5f, this);
            if (next == null) return;
            var eh = next.GetComponent<EnemyHealth>();
            eh?.Damage(dealt * 0.45f, from, 0.4f, false, false, HitKind.Shock);
        }

        public void ApplyFreeze(float duration)
        {
            ApplySlow(0.15f, duration);
            DamagePopup.SpawnTag(transform.position, "冰", HitKind.Ice);
        }

        public void ApplyPlague(float dealt, Vector3 from)
        {
            _plagueUntil = Time.time + 2.8f;
            Damage(dealt * 0.2f, from, 0f, false, false, HitKind.Plague);
        }

        public void ApplyHoly(float dealt, Vector3 from)
        {
            Damage(dealt * 0.22f, from, 0.2f, false, false, HitKind.Holy);
        }

        public void ApplyChaos(float dealt, Vector3 from)
        {
            var mul = Random.Range(0.7f, 1.3f);
            if (Random.value < 0.5f) ApplySlow(0.7f, 1.2f);
            Damage(dealt * 0.18f * mul, from, 0.3f, false, false, HitKind.Chaos);
        }
    }
}
