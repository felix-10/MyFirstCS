using UnityEngine;

namespace Veinfire
{
    public static class RunRules
    {
        public static float RunSeconds
        {
            get
            {
                switch (RunConfig.Mode)
                {
                    case GameModeId.Short: return 600f;
                    case GameModeId.Endless: return 0f;
                    case GameModeId.BossRush: return 0f;
                    default: return 1200f;
                }
            }
        }

        public static bool TimeVictory => RunSeconds > 0.1f;

        public static bool HymnFire =>
            GameInstaller.CurrentUnique == UniqueId.Hymn && GameInstaller.CurrentVein == VeinId.Fire;

        public static bool BladeWater =>
            GameInstaller.CurrentUnique == UniqueId.Blade && GameInstaller.CurrentVein == VeinId.Water;

        public static bool BoltEarth =>
            GameInstaller.CurrentUnique == UniqueId.Bolt && GameInstaller.CurrentVein == VeinId.Earth;

        public static float UniqueRange(UniqueId id)
        {
            switch (id)
            {
                case UniqueId.Bolt: return 24f;
                case UniqueId.Blade: return 12f;
                case UniqueId.Spiral: return 20f;
                case UniqueId.Star: return 26f;
                default: return 22f;
            }
        }

        public static int Threat(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Boss: return 0;
                case EnemyKind.Elite: return 1;
                case EnemyKind.Spitter: return 2;
                case EnemyKind.Exploder: return 3;
                case EnemyKind.Tank: return 4;
                case EnemyKind.Runner: return 5;
                default: return 6;
            }
        }
    }
}
