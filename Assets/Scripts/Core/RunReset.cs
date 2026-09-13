using System.Collections;
using UnityEngine;

namespace Veinfire
{
    public sealed class RunReset : MonoBehaviour
    {
        public static bool Busy { get; private set; }

        public static void Begin()
        {
            if (Busy) return;
            Busy = true;
            Time.timeScale = 1f;
            var go = new GameObject("VeinfireReset");
            DontDestroyOnLoad(go);
            go.AddComponent<RunReset>();
        }

        void Start()
        {
            StartCoroutine(ResetRoutine());
        }

        IEnumerator ResetRoutine()
        {
            GameInstaller.WipeRunObjects();
            yield return null;
            GameSession.ClearRunFlags();
            GameInstaller.BuildWorld(GameInstaller.CurrentMap);
            Busy = false;
            Destroy(gameObject);
        }
    }
}
