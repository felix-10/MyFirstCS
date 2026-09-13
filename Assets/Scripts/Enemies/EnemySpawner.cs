using UnityEngine;

namespace Veinfire
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        public Transform Player;
        public HudView Hud;
        public XpOrb OrbPrefab;
        public Pickup PickupPrefab;
        public EnemyBolt BoltPrefab;
        public EnemyChase SwarmPrefab;
        public EnemyChase RunnerPrefab;
        public EnemyChase TankPrefab;
        public EnemyChase ExploderPrefab;
        public EnemyChase SpitterPrefab;
        public EnemyChase ElitePrefab;
        public EnemyChase BossPrefab;

        public void WriteSave(RunSnapshot data)
        {
            data.boss2 = _boss2;
            data.boss5 = _boss5;
            data.boss8 = _boss8;
            data.boss12 = _boss12;
            data.boss17 = _boss17;
        }

        public void RestoreTime(float elapsed, bool boss2, bool boss5, bool boss8, bool boss12 = false, bool boss17 = false)
        {
            _elapsed = Mathf.Max(0f, elapsed);
            _boss2 = boss2 || elapsed >= ScaleTime(150f);
            _boss5 = boss5 || elapsed >= ScaleTime(330f);
            _boss8 = boss8 || elapsed >= ScaleTime(510f);
            _boss12 = boss12 || elapsed >= ScaleTime(750f);
            _boss17 = boss17 || elapsed >= ScaleTime(1050f);
            _h1 = elapsed >= ScaleTime(48f);
            _h2 = elapsed >= ScaleTime(198f);
            _h3 = elapsed >= ScaleTime(408f);
            _h4 = elapsed >= ScaleTime(600f);
            _h5 = elapsed >= ScaleTime(888f);
            _won = RunRules.TimeVictory && elapsed >= RunRules.RunSeconds;
        }

        static float ScaleTime(float t)
        {
            return RunConfig.Mode == GameModeId.Short ? t * 0.5f : t;
        }

        static EnemySpawner _i;
        float _timer;
        float _elapsed;
        bool _boss2;
        bool _boss5;
        bool _boss8;
        bool _boss12;
        bool _boss17;
        bool _h1;
        bool _h2;
        bool _h3;
        bool _h4;
        bool _h5;
        bool _won;
        int _nextBoss = 1;
        int _loopBoss;

        void OnEnable() => _i = this;
        void OnDisable() { if (_i == this) _i = null; }

        void Update()
        {
            if (GameSession.IsPaused || Player == null || GameSession.IsGameOver) return;
            _elapsed += Time.deltaTime;
            MaybeEvents();

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            var difficulty = 1f + _elapsed / 90f;
            _timer = Mathf.Max(0.28f, 1.05f / difficulty);

            var alive = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;
            var budget = 48;
            if (RunConfig.Mode == GameModeId.BossRush)
                budget = Mathf.Min(16, 4 + Mathf.FloorToInt(_elapsed / 25f));
            else
                budget = Mathf.Min(48, 9 + Mathf.FloorToInt(_elapsed / 12f));
            if (alive >= budget) return;
            if (RunConfig.Mode == GameModeId.BossRush) return;

            SpawnOne(PickKind(), 1f);
        }

        void MaybeEvents()
        {
            if (RunConfig.Mode == GameModeId.BossRush)
            {
                MaybeRush();
                return;
            }

            TryHorde(ref _h1, ScaleTime(48f), 8, 0, "尸潮来了！");
            TryHorde(ref _h2, ScaleTime(198f), 10, 1, "第二轮尸潮");
            TryHorde(ref _h3, ScaleTime(408f), 12, 2, "第三轮尸潮");
            TryHorde(ref _h4, ScaleTime(600f), 14, 3, "中期尸潮");
            TryHorde(ref _h5, ScaleTime(888f), 16, 4, "第五轮尸潮");

            if (!_boss2 && _elapsed >= ScaleTime(150f))
            {
                _boss2 = true;
                SpawnBoss("血脉守卫", 1, EnemyKind.Runner, 4);
            }

            if (!_boss5 && _elapsed >= ScaleTime(330f))
            {
                _boss5 = true;
                SpawnBoss("静脉巨像", 2, EnemyKind.Runner, 4);
            }

            if (!_boss8 && _elapsed >= ScaleTime(510f))
            {
                _boss8 = true;
                SpawnBoss("夜火领主", 3, EnemyKind.Runner, 4);
            }

            if (!_boss12 && _elapsed >= ScaleTime(750f))
            {
                _boss12 = true;
                SpawnBoss("熔脉先锋", 4, EnemyKind.Tank, 2);
                for (var i = 0; i < 2; i++) SpawnOne(EnemyKind.Exploder, 1f);
            }

            if (!_boss17 && _elapsed >= ScaleTime(1050f))
            {
                _boss17 = true;
                SpawnBoss("烬核主宰", 5, EnemyKind.Spitter, 2);
                for (var i = 0; i < 2; i++) SpawnOne(EnemyKind.Exploder, 1f, true);
            }

            if (RunConfig.Mode == GameModeId.Endless && _elapsed >= ScaleTime(1230f))
            {
                var n = Mathf.FloorToInt((_elapsed - ScaleTime(1050f)) / 180f);
                if (n > _loopBoss)
                {
                    _loopBoss = n;
                    SpawnBoss("循环首领", 1 + n % 5, EnemyKind.Runner, 3);
                }
            }

            if (!_won && RunRules.TimeVictory && _elapsed >= RunRules.RunSeconds)
            {
                _won = true;
                GameSession.Win();
            }
        }

        void MaybeRush()
        {
            if (!_boss2 && _elapsed >= 6f)
            {
                _boss2 = true;
                SpawnBoss("狂袭 · 血脉守卫", 1, EnemyKind.Runner, 2);
            }

            if (!_boss5 && _elapsed >= 50f)
            {
                _boss5 = true;
                SpawnBoss("狂袭 · 静脉巨像", 2, EnemyKind.Runner, 2);
            }

            if (!_boss8 && _elapsed >= 100f)
            {
                _boss8 = true;
                SpawnBoss("狂袭 · 夜火领主", 3, EnemyKind.Tank, 1);
            }

            if (!_boss12 && _elapsed >= 160f)
            {
                _boss12 = true;
                SpawnBoss("狂袭 · 熔脉先锋", 4, EnemyKind.Spitter, 2);
            }

            if (!_boss17 && _elapsed >= 230f)
            {
                _boss17 = true;
                SpawnBoss("狂袭 · 烬核主宰", 5, EnemyKind.Exploder, 2);
            }

            if (!_won && RunConfig.BossKills >= 5)
            {
                _won = true;
                GameSession.Win();
            }
        }

        void TryHorde(ref bool flag, float at, int swarm, int elite, string banner)
        {
            if (flag || _elapsed < at) return;
            flag = true;
            Hud?.ShowEvent(banner);
            GameFeel.EventPulse();
            var alive = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;
            var room = Mathf.Max(0, 48 - alive);
            var swarmN = Mathf.Min(swarm, room);
            for (var i = 0; i < swarmN; i++) SpawnOne(EnemyKind.Swarm, 0.9f);
            room = Mathf.Max(0, 48 - FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length);
            var eliteN = Mathf.Min(elite, room);
            for (var i = 0; i < eliteN; i++) SpawnOne(EnemyKind.Elite, 0.85f);
        }

        void SpawnBoss(string banner, int index, EnemyKind extra, int extraCount)
        {
            Hud?.ShowEvent(banner);
            GameFeel.Boss();
            _nextBoss = index;
            SpawnOne(EnemyKind.Boss, 0.85f);
            if (RunConfig.Mode != GameModeId.BossRush)
            {
                for (var i = 0; i < extraCount; i++)
                    SpawnOne(extra, 1f);
            }
        }

        public static void SpawnSupport(EnemyKind kind, Vector3 from, int count, bool puddleOnDeath = false)
        {
            if (_i == null) return;
            for (var i = 0; i < count; i++)
                _i.SpawnOne(kind, 1f, puddleOnDeath, from);
        }

        void SpawnOne(EnemyKind kind, float hpScale, bool puddleOnDeath = false, Vector3? around = null)
        {
            var prefab = PrefabFor(kind);
            if (prefab == null) return;
            var origin = around ?? (Player != null ? Player.position : Vector3.zero);
            var pos = origin;
            for (var n = 0; n < 8; n++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var radius = kind == EnemyKind.Boss ? 16f : 8.5f;
                pos = origin + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                if (Player != null) pos.y = Player.position.y;
                if (!Physics.CheckSphere(pos + Vector3.up * 0.9f, 0.7f, ~0, QueryTriggerInteraction.Ignore))
                    break;
            }

            var enemy = Instantiate(prefab, pos, Quaternion.identity);
            enemy.gameObject.SetActive(true);
            Tune(enemy.gameObject, kind, hpScale, puddleOnDeath);
        }

        EnemyKind PickKind()
        {
            var t = _elapsed;
            var roll = Random.value;
            if (t > 120f && roll < 0.04f) return EnemyKind.Elite;
            if (t > 100f && roll < 0.12f) return EnemyKind.Exploder;
            if (t > 80f && roll < 0.22f) return EnemyKind.Spitter;
            if (t > 55f && roll < 0.36f) return EnemyKind.Tank;
            if (t > 30f && roll < 0.58f) return EnemyKind.Runner;
            return EnemyKind.Swarm;
        }

        EnemyChase PrefabFor(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Runner: return RunnerPrefab;
                case EnemyKind.Tank: return TankPrefab;
                case EnemyKind.Exploder: return ExploderPrefab;
                case EnemyKind.Spitter: return SpitterPrefab;
                case EnemyKind.Elite: return ElitePrefab;
                case EnemyKind.Boss: return BossPrefab;
                default: return SwarmPrefab;
            }
        }

        void Tune(GameObject go, EnemyKind kind, float hpScale, bool puddleOnDeath = false)
        {
            var health = go.GetComponent<EnemyHealth>();
            var chase = go.GetComponent<EnemyChase>();
            var hpDiv = RunConfig.Mode == GameModeId.Short ? 110f : 160f;
            var timeScale = 1f + _elapsed / hpDiv;
            if (RunConfig.Mode == GameModeId.Endless) timeScale = Mathf.Min(4f, timeScale);
            float hp;
            int xp;
            float speed;
            var contact = go.GetComponent<ContactDamager>();
            if (contact == null) contact = go.AddComponent<ContactDamager>();
            contact.DamagePerSecond = 9f;
            switch (kind)
            {
                case EnemyKind.Runner:
                    hp = 12f; xp = 4; speed = 3.6f;
                    go.transform.localScale = Vector3.one * 0.75f;
                    break;
                case EnemyKind.Tank:
                    hp = 58f; xp = 9; speed = 1.45f;
                    go.transform.localScale = Vector3.one * 1.28f;
                    contact.DamagePerSecond = 14f;
                    break;
                case EnemyKind.Exploder:
                    hp = 20f; xp = 6; speed = 2.5f;
                    go.transform.localScale = Vector3.one * 0.9f;
                    PrimitiveFactory.Paint(go, new Color(1f, 0.45f, 0.12f));
                    break;
                case EnemyKind.Spitter:
                    hp = 24f; xp = 7; speed = 1.9f;
                    chase.KeepDistance = true;
                    chase.PreferredRange = 9f;
                    var ranged = go.GetComponent<EnemyRanged>() ?? go.AddComponent<EnemyRanged>();
                    ranged.BoltPrefab = BoltPrefab;
                    ranged.Damage = 6f + _elapsed / 80f;
                    PrimitiveFactory.Paint(go, new Color(0.55f, 0.35f, 0.85f));
                    break;
                case EnemyKind.Elite:
                    hp = 95f; xp = 18; speed = 2.4f;
                    go.transform.localScale = Vector3.one * 1.4f;
                    health.DropLuck = 1f;
                    contact.DamagePerSecond = 12f;
                    PrimitiveFactory.Paint(go, new Color(0.95f, 0.85f, 0.2f));
                    break;
                case EnemyKind.Boss:
                    hp = 280f; xp = 80; speed = 1.45f;
                    go.transform.localScale = Vector3.one * 2.1f;
                    health.DropLuck = 1f;
                    contact.DamagePerSecond = 18f;
                    PrimitiveFactory.Paint(go, new Color(0.7f, 0.08f, 0.18f));
                    break;
                default:
                    hp = 16f; xp = 5; speed = 2.2f;
                    break;
            }

            health.Kind = kind;
            health.BossIndex = kind == EnemyKind.Boss ? _nextBoss : 0;
            health.LeavePuddleOnDeath = puddleOnDeath;
            health.Armor = MapCatalog.EnemyArmor(GameInstaller.CurrentMap) + RunConfig.EnemyArmorAdd;
            health.OrbPrefab = OrbPrefab;
            health.PickupPrefab = PickupPrefab;
            health.Setup(hp * hpScale * timeScale * MapCatalog.EnemyHpMul(GameInstaller.CurrentMap)
                * (RunConfig.HasCurse(CurseId.SparseLoot) ? 0.88f : 1f)
                * (RunConfig.Mode == GameModeId.BossRush && kind == EnemyKind.Boss ? 1.2f : 1f), xp);
            chase.Speed = speed * Mathf.Min(RunConfig.Mode == GameModeId.Endless ? 2f : 99f,
                1f + _elapsed / (RunConfig.Mode == GameModeId.Short ? 280f : 360f)) * RunConfig.EnemySpeedMul * RunConfig.MonsterMoveMul;
            var dmgMul = MapCatalog.EnemyDamageMul(GameInstaller.CurrentMap);
            if (Time.time < RunConfig.EventUntil) dmgMul *= 1.2f;
            var dmgTime = 1f + _elapsed / 200f;
            if (RunConfig.Mode == GameModeId.Endless) dmgTime = Mathf.Min(3.5f, dmgTime);
            dmgMul *= dmgTime;
            if (kind == EnemyKind.Elite && RunConfig.HasCurse(CurseId.EliteFury)) dmgMul *= 1.2f;
            contact.DamagePerSecond *= dmgMul;
            var spit = go.GetComponent<EnemyRanged>();
            if (spit != null) spit.Damage *= dmgMul;
            chase.Speed *= MapCatalog.EnemyMoveMul;
            if (RunConfig.MapStar >= 3 && kind != EnemyKind.Boss)
            {
                chase.Speed *= 1.06f;
                go.transform.localScale *= 1.08f;
            }
            if (RunConfig.MapStar >= 4)
            {
                contact.DamagePerSecond *= 1.12f;
                if (spit != null) spit.Damage *= 1.12f;
            }
            if ((RunConfig.HasMod(RelicMod.VeinResonance) || RunConfig.HasCurse(CurseId.ChaosMutate)) && kind != EnemyKind.Boss && Random.value < 0.22f)
            {
                if (Random.value < 0.5f) health.Ignite(4f);
                else health.ApplyWet(4f);
            }
        }
    }
}
