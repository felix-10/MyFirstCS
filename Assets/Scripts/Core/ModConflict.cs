using UnityEngine;

namespace Veinfire
{
    public static class ModConflict
    {
        public static bool Blocks(RelicMod mod, CurseId curse)
        {
            if (mod == RelicMod.WeaponTrial && curse == CurseId.Armor) return true;
            if (mod == RelicMod.VeinResonance && curse == CurseId.Bleed) return true;
            if (mod == RelicMod.DashWay && curse == CurseId.Swift) return true;
            if (mod == RelicMod.VeinResonance && curse == CurseId.ChaosMutate) return true;
            return false;
        }

        public static bool WouldBlockMod(RelicMod id)
        {
            return Blocks(id, RunConfig.CurseA) || Blocks(id, RunConfig.CurseB) || Blocks(id, RunConfig.CurseC);
        }

        public static bool WouldBlockCurse(CurseId id)
        {
            return Blocks(RunConfig.ModA, id) || Blocks(RunConfig.ModB, id);
        }

        public static string WarnLine()
        {
            var w = "";
            if (RunConfig.HasMod(RelicMod.GaleField) && RunConfig.HasCurse(CurseId.Swift))
                w += "警告：疾风战场 + 迅疾，怪物极快。\n";
            if (RunConfig.HasMod(RelicMod.FleshOffering) && RunConfig.HasCurse(CurseId.Bleed))
                w += "警告：血肉献祭 + 流血，生存压力极大。\n";
            if (RunConfig.HasMod(RelicMod.RotSpread) && RunConfig.HasCurse(CurseId.PlagueSpread))
                w += "警告：溃烂蔓延 + 瘟疫侵染，帧率压力升高。\n";
            return w;
        }
    }
}
