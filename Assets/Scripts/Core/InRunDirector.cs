using UnityEngine;

namespace Veinfire
{
    public sealed class InRunDirector : MonoBehaviour
    {
        float _eventAt = 75f;
        int _cycle;

        void Update()
        {
            if (GameSession.IsGameOver) return;
            if (GameSession.IsPaused) return;
            if (RunConfig.HasCurse(CurseId.Miasma) && Time.frameCount % 220 == 0)
            {
                var p = FindFirstObjectByType<PlayerMotor>();
                if (p != null) MapHazard.SpawnPuddle(p.transform.position + Random.insideUnitSphere * 8f, 2.2f, 10f, 3f);
            }
            if (RunConfig.Mode == GameModeId.BossRush) return;
            TryEvent();
        }

        void TryEvent()
        {
            if (GameSession.Clock < _eventAt) return;
            _eventAt += RunConfig.Mode == GameModeId.Short ? 66f : 95f;
            if (IsNearBoss(GameSession.Clock)) return;
            if (RunConfig.Mode == GameModeId.Short && Random.value < 0.3f) return;
            var roll = Random.value;
            var hud = FindFirstObjectByType<HudView>();
            if (MapCatalog.Current.risk > 0.45f && roll < 0.28f)
            {
                RunConfig.EventName = RunConfig.MapStar >= 5 ? "炼狱抉择" : "洞窟塌方";
                var p = FindFirstObjectByType<PlayerMotor>();
                if (p != null)
                {
                    MapHazard.SpawnPuddle(p.transform.position + Random.insideUnitSphere * 6f, 3.4f, 14f, 3.6f);
                    EnemySpawner.SpawnSupport(EnemyKind.Elite, p.transform.position, 1);
                }
                hud?.ShowEvent(RunConfig.EventName + "：高风险事件");
                return;
            }
            if (roll < 0.34f)
            {
                RunConfig.EventName = "残烬涌流";
                SpawnOrbs(12);
                hud?.ShowEvent("残烬涌流：经验球爆发");
            }
            else if (roll < 0.67f)
            {
                RunConfig.EventName = "畸变狂潮";
                var at = FindFirstObjectByType<PlayerMotor>() != null
                    ? FindFirstObjectByType<PlayerMotor>().transform.position
                    : Vector3.zero;
                EnemySpawner.SpawnSupport(EnemyKind.Swarm, at, 8);
                EnemySpawner.SpawnSupport(EnemyKind.Elite, at, 1);
                hud?.ShowEvent("畸变狂潮：怪物暴涨");
            }
            else
            {
                RunConfig.EventName = "战场抉择";
                var health = FindFirstObjectByType<PlayerHealth>();
                if (health != null && Random.value < 0.5f)
                {
                    health.Heal(40f);
                    hud?.ShowEvent("抉择：回复生命 +40");
                }
                else
                {
                    FindFirstObjectByType<LevelDirector>()?.GrantRare("血脉回响");
                    hud?.ShowEvent("抉择：获得稀有被动，怪物短暂狂暴");
                    RunConfig.EventUntil = Time.time + 20f;
                }
            }
        }

        static bool IsNearBoss(float t)
        {
            float[] nodes = { 150f, 330f, 510f, 750f, 1050f };
            for (var i = 0; i < nodes.Length; i++)
                if (Mathf.Abs(t - nodes[i]) < 18f) return true;
            return false;
        }

        static void SpawnOrbs(int n)
        {
            var player = FindFirstObjectByType<PlayerMotor>();
            var spawner = FindFirstObjectByType<EnemySpawner>();
            if (player == null || spawner == null || spawner.OrbPrefab == null) return;
            for (var i = 0; i < n; i++)
            {
                var p = player.transform.position + Random.insideUnitSphere * 6f;
                p.y = player.transform.position.y + 0.35f;
                var orb = Instantiate(spawner.OrbPrefab, p, Quaternion.identity);
                orb.gameObject.SetActive(true);
                orb.Amount = 6;
            }
        }

        public static void OnDash(Vector3 at)
        {
            var karen = GameInstaller.CurrentHero == HeroId.Karen;
            if (!RunConfig.DashPulse && !karen) return;
            var stats = Object.FindFirstObjectByType<PlayerCombatStats>();
            var dash = Object.FindFirstObjectByType<PlayerDash>();
            var scale = 0.55f;
            if (karen && dash != null) scale += Mathf.Clamp(2f - dash.Cooldown, 0f, 1.2f) * 0.15f;
            if (RunConfig.HasMod(RelicMod.DashWay)) scale *= 1.45f;
            if (RunConfig.RelicOn("奔袭护符")) scale *= 1.6f;
            var dmg = stats != null ? stats.ScaledDamage * scale : 10f;
            CombatUtil.DamageEnemiesInRadius(at, 2.2f, dmg, 2.4f, null, 0f, stats);
        }

        public static void OnBossDead(Vector3 at)
        {
            RunConfig.BossKills += 1;
            if (Random.value > (MetaProgress.RelicPackUnlocked ? 0.55f : 0.45f) * (RunConfig.Mode == GameModeId.Short ? 0.7f : 1f)) return;
            var hud = FindFirstObjectByType<HudView>();
            string[] names =
            {
                "复活护盾", "冷静风暴", "血脉倍增", "血脉之心", "冲刺纹章",
                "武器过载", "腐蚀烙印", "奔袭护符", "石化棱镜", "生命馈赠"
            };
            var pick = names[Random.Range(0, MetaProgress.RelicPackUnlocked ? names.Length : 3)];
            RunConfig.RelicName = pick;
            RunConfig.RelicUntil = Time.time + 40f;
            if (pick == "复活护盾") RunConfig.HasRevive = true;
            if (pick == "生命馈赠")
            {
                var hp = FindFirstObjectByType<PlayerHealth>();
                hp?.RaiseMaxHp(30f, false);
                RunConfig.RelicUntil = 0f;
            }
            hud?.ShowEvent("局内遗物：" + pick);
        }
    }
}
