using UnityEngine;

namespace Veinfire
{
    public static class GameSettings
    {
        const string MusicKey = "vf_music";
        const string SfxKey = "vf_sfx";
        const string MapKey = "vf_lastmap";
        const string HeroKey = "vf_lasthero";
        const string VeinKey = "vf_lastvein";
        const string UniqueKey = "vf_lastunique";
        const string StarKey = "vf_laststar";
        const string PlayedKey = "vf_played";
        const string DevKey = "vf_developer";

        public static float Music = 0.7f;
        public static float Sfx = 1f;
        public static bool Developer = true;
        public static int LastStar = 1;
        public static MapId LastMap = MapId.VeinWaste;
        public static HeroId LastHero = HeroId.Geralt;
        public static VeinId LastVein = VeinId.Fire;
        public static UniqueId LastUnique = UniqueId.Hymn;

        public static void Load()
        {
            Music = PlayerPrefs.GetFloat(MusicKey, 0.7f);
            Sfx = PlayerPrefs.GetFloat(SfxKey, 1f);
            LastMap = (MapId)Mathf.Clamp(PlayerPrefs.GetInt(MapKey, 0), 0, 4);
            LastStar = Mathf.Clamp(PlayerPrefs.GetInt(StarKey, 1), 1, 5);
            LastHero = (HeroId)Mathf.Clamp(PlayerPrefs.GetInt(HeroKey, 0), 0, 4);
            LastVein = (VeinId)Mathf.Clamp(PlayerPrefs.GetInt(VeinKey, 0), 0, 8);
            LastUnique = (UniqueId)Mathf.Clamp(PlayerPrefs.GetInt(UniqueKey, 0), 0, 4);
            Developer = PlayerPrefs.GetInt(DevKey, 1) == 1;
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(MusicKey, Music);
            PlayerPrefs.SetFloat(SfxKey, Sfx);
            PlayerPrefs.SetInt(MapKey, (int)LastMap);
            PlayerPrefs.SetInt(StarKey, LastStar);
            PlayerPrefs.SetInt(HeroKey, (int)LastHero);
            PlayerPrefs.SetInt(VeinKey, (int)LastVein);
            PlayerPrefs.SetInt(UniqueKey, (int)LastUnique);
            PlayerPrefs.SetInt(DevKey, Developer ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool HasContinue => RunSave.Exists;

        public static bool HasPlayed => PlayerPrefs.GetInt(PlayedKey, 0) == 1;

        public static void MarkPlayed(MapId map, HeroId hero, VeinId vein, UniqueId unique)
        {
            LastMap = map;
            LastHero = hero;
            LastVein = vein;
            LastUnique = unique;
            LastStar = RunConfig.MapStar;
            PlayerPrefs.SetInt(PlayedKey, 1);
            Save();
        }

        public static void ApplyAudio()
        {
            AudioHub.SetMix(Music, Sfx);
        }
    }
}
