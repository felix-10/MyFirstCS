using UnityEngine;

namespace Veinfire
{
    public struct CodexCard
    {
        public string Title;
        public string Mark;
        public string Preview;
        public string Detail;
        public bool Unlocked;
        public Color Tint;
    }

    public static class CodexVault
    {
        public static CodexCard[] Enemies()
        {
            var d = MetaProgress.Data;
            return new[]
            {
                Enemy("尸潮", "Swarm", d.seenSwarm, "生命 16  经验 5  移速 2.2\n接触约 9/秒。开局即出现的基础单位，数量堆压力。"),
                Enemy("奔行者", "Runner", d.seenRunner, "生命 12  经验 4  移速 3.6\n体型较小，约 30 秒后出现，贴身纠结。"),
                Enemy("重甲", "Tank", d.seenTank, "生命 58  经验 9  移速 1.45\n体型更大，接触伤害更高，约 55 秒出现。"),
                Enemy("喷吐", "Spitter", d.seenSpitter, "生命 24  经验 7  移速 1.9\n保持约 9 距射击，约 80 秒出现。"),
                Enemy("自爆", "Exploder", d.seenExploder, "生命 20  经验 6  移速 2.5\n死亡时近距爆炸约 12 点，约 100 秒出现。"),
                Enemy("精英", "Elite", d.seenElite, "生命 95  经验 18  移速 2.4\n必掉磁铁或回血。约 120 秒出现。"),
                Enemy("首领", "Boss", d.seenBoss, "生命 280  经验 80  移速 1.45\n定时出现，必掉宝箱，可能掉出局内遗物。")
            };
        }

        public static CodexCard[] Loadout()
        {
            var maps = BalanceTables.Maps;
            var cards = new System.Collections.Generic.List<CodexCard>();
            for (var i = 0; i < maps.Length; i++)
            {
                var spec = maps[i];
                var open = MetaProgress.MapUnlocked(spec.MapId, spec.star);
                cards.Add(new CodexCard
                {
                    Title = spec.title,
                    Mark = open ? "地图" : "锁定",
                    Preview = $"★{spec.star}",
                    Detail = MapCatalog.Hover(spec, open),
                    Unlocked = open,
                    Tint = open ? UiStyle.Accent : UiStyle.Muted
                });
            }

            cards.AddRange(new[]
            {
                Open("杰罗特", "角色", "生命 130  伤害 16  移速 5.5\n暴击 0%  暴伤 ×2.0  护甲 0\n被动：协同打击。每带一件普通武器加快攻速，击杀有概率标记敌人。"),
                Open("利兹", "角色", "生命 112  伤害 15.5  移速 5.65\n暴击 15%  暴伤 ×2.7  护甲 0\n被动：裂隙裁决。暴击刷新血脉并打出脉冲。"),
                Open("安塔洛", "角色", "生命 168  伤害 13.5  移速 5.15\n暴击 0%  暴伤 ×2.0  护甲 5\n被动：石肤共振。护甲转化生命，残血击退。"),
                Open("卡伦", "角色", "生命 122  伤害 14  移速 6.0  护甲 2\n被动：奔袭回响。冲刺造成范围伤害。元研究解锁。"),
                Open("莫拉", "角色", "生命 118  伤害降低 12%\n被动：溃烂蔓延。DOT 可传染。元研究解锁。"),
                Open("火之血脉", "血脉", VeinCatalog.Blurb(VeinId.Fire) + "\n与地狱绝唱配对可强化灼烧。"),
                Open("水之血脉", "血脉", VeinCatalog.Blurb(VeinId.Water) + "\n与特莫利斯之剑配对可强化扩散。"),
                Open("土之血脉", "血脉", VeinCatalog.Blurb(VeinId.Earth) + "\n与神罚天降配对可强化石化爆发。"),
                Open("地狱绝唱 / 神罚 / 长剑", "专武", "三件专武互斥，最高 5 级进化。\n绝唱远程弹幕，神罚落地雷域，长剑近战扇斩。"),
                Open("幽猎螺旋 / 星陨仪", "专武", "军械专武Ⅳ解锁。螺旋穿透弹与星陨轰炸，互斥，5级进化。"),
                Open("普通武器 5-30", "武器", "分三包由军械研究解锁。栏位为 1 专武 + 7 普通；融合后两把占 1 栏。"),
                Open("武器组合", "融合", "对局按 Tab 打开档案，切到「组合」分页。一把普通武器可与多件武器配对。\n双方满 5 级后，升级卡会同时列出可选的组合技能，由你选定一条。合成后这两件武器不再提示其它组合。\n爆裂图腾：基础可合成「暴虐图腾」（+榴弹）或「震地巫柱」（+冲击波）。"),
                Open("弹幕灼烧流", "流派", "杰罗特 / 地狱绝唱 / 火血脉 / 环绕毒牙+血爆脉冲。"),
                Open("雷霆连锁冲刺流", "流派", "卡伦 / 特莫利斯之剑 / 雷霆血脉。"),
                Open("暴击石化爆发流", "流派", "利兹 / 神罚天降 / 土血脉 / 尖刺弹幕+碎裂星屑。"),
                Open("腐蚀溃烂传染流", "流派", "莫拉 / 地狱绝唱 / 腐蚀血脉 / 毒雾+孢子炮。"),
                Open("寒冰冻结控制流", "流派", "杰罗特 / 幽猎螺旋 / 寒冰血脉。"),
                Open("瘟疫持续传染流", "流派", "莫拉 / 神罚天降 / 瘟疫血脉。"),
                Open("圣光护盾苟活流", "流派", "安塔洛 / 星陨仪 / 圣光血脉。"),
                Open("混沌畸变摇摆流", "流派", "利兹 / 幽猎螺旋 / 混沌血脉。"),
                Open("近战残血搏命流", "流派", "安塔洛 / 特莫利斯之剑 / 水血脉。"),
                Open("多武器分流混合流", "流派", "杰罗特 / 星陨仪 / 雷霆血脉。")
            });
            return cards.ToArray();
        }

        public static CodexCard[] Archive()
        {
            var rows = MetaProgress.Data.archive ?? System.Array.Empty<RunArchiveRow>();
            if (rows.Length == 0)
                return new[] { Open("尚无档案", "纪录", "打完一局后会出现在这里。最多保留最近 30 局。") };
            var n = Mathf.Min(30, rows.Length);
            var cards = new CodexCard[n];
            for (var i = 0; i < n; i++)
            {
                var r = rows[rows.Length - 1 - i];
                var title = $"{RunConfig.ModeTitle((GameModeId)r.mode)}  ·  {(r.won ? "胜" : "负")}";
                var detail =
                    $"{HeroCatalog.Title((HeroId)r.hero)} / {VeinCatalog.Title((VeinId)r.vein)} / {UniqueCatalog.Title((UniqueId)r.unique)}\n" +
                    $"{MapCatalog.Title((MapId)r.map, r.star > 0 ? r.star : 1)}    存活 {Mathf.FloorToInt(r.time / 60f):00}:{Mathf.FloorToInt(r.time % 60f):00}";
                cards[i] = Open(title, "档案", detail);
            }
            return cards;
        }

        public static CodexCard[] Achievements()
        {
            var d = MetaProgress.Data;
            return new[]
            {
                Ach("夜火初熄", "首次通关", d.achWin, "在标准或其它有胜负的模式中胜利一次。", "已完成。残烬结算会额外记入通关。"),
                Ach("第一次倒下", "首次阵亡", d.achDeath, "任意对局中被击倒一次。", "已完成。失败仍会结算少量残烬。"),
                Ach("猎杀首领", "击杀 Boss", d.achBoss, "对局中击杀至少一只 Boss。", "已完成。首领图鉴同步点亮。"),
                Ach("百人斩", "单局击杀 100", d.achKills, "单局击杀数达到 100。", "已完成。证明你能撑过中期尸潮。"),
                Ach("双融试炼", "过程", d.achFuse, "单局完成至少 2 次武器融合。", "已完成。"),
                Ach("卡运耗尽", "过程", d.achReroll, "一局内用尽全部升级刷新次数。", "已完成。"),
                Ach("三星开拓", "星级", d.achStar3, "通关任意 ★3 地图。", "已完成。"),
                Ach("最高击杀", "纪录", d.bestKills > 0, "任意对局刷新个人最高击杀。", $"当前纪录  {d.bestKills}  击杀"),
                Ach("最长存活", "纪录", d.bestTime > 10f, "任意对局刷新最长存活时间。", $"当前纪录  {Mathf.FloorToInt(d.bestTime / 60f):00}:{Mathf.FloorToInt(d.bestTime % 60f):00}")
            };
        }

        static CodexCard Enemy(string name, string mark, bool on, string detail)
        {
            return new CodexCard
            {
                Title = on ? name : "未知畸变",
                Mark = on ? "已收录" : "未收录",
                Preview = on ? mark : "???",
                Detail = on ? detail : "在对局中遭遇并击杀后，才会把真实名称与数值写入图鉴。",
                Unlocked = on,
                Tint = on ? UiStyle.Accent : UiStyle.Muted
            };
        }

        static CodexCard Open(string name, string mark, string detail)
        {
            return new CodexCard
            {
                Title = name,
                Mark = "已收录",
                Preview = mark,
                Detail = detail,
                Unlocked = true,
                Tint = UiStyle.Accent
            };
        }

        static CodexCard Ach(string name, string mark, bool on, string locked, string done)
        {
            return new CodexCard
            {
                Title = on ? name : name,
                Mark = on ? "已解锁" : "未解锁",
                Preview = mark,
                Detail = on ? done : locked,
                Unlocked = on,
                Tint = on ? new Color(1f, 0.84f, 0.38f) : UiStyle.Muted
            };
        }
    }
}
