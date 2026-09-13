using System;
using UnityEngine;

namespace Veinfire
{
    public sealed class UpgradeOption
    {
        public string Title;
        public string Description;
        public Action Apply;
        public string Mark;
        public Color Color;
        public bool IsWeapon;
        public string Category = "通用";
        public WeaponId SourceWeapon;
        public bool FusionHint;
        public bool OwnedHint;
        public bool ReplaceHint;

        public UpgradeOption(string title, string description, Action apply, string mark = "◆", bool weapon = false, string category = "通用")
        {
            Title = title;
            Description = description;
            Apply = apply;
            Mark = mark;
            IsWeapon = weapon;
            Category = category;
            Color = weapon ? new Color(0.95f, 0.55f, 0.2f) : new Color(0.45f, 0.55f, 0.85f);
        }
    }

    public sealed class TraitRecord
    {
        public string Title;
        public string Mark;
        public Color Color;
        public bool IsWeapon;
        public int Stacks = 1;
    }
}
