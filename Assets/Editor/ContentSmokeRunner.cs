using System.IO;
using UnityEditor;
using UnityEngine;

namespace Veinfire
{
    [InitializeOnLoad]
    static class ContentSmokeRunner
    {
        static ContentSmokeRunner()
        {
            // Re-bind after script reload so .smoke-once can start Play Mode.
            EditorApplication.delayCall += TryStart;
        }

            [MenuItem("Veinfire/Run Content Smoke")]
            static void Menu()
            {
                EditorPrefs.SetBool("Veinfire.Smoke", true);
                if (EditorApplication.isPlaying)
                {
                    if (Object.FindFirstObjectByType<ContentSmoke>() == null)
                        new GameObject("ContentSmoke").AddComponent<ContentSmoke>();
                    return;
                }

                EditorApplication.isPlaying = true;
            }

        static void TryStart()
        {
            var root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var marker = Path.Combine(root, ".smoke-once");
            var kick = Path.Combine(root, "SmokeKick.txt");
            File.WriteAllText(kick, "delayCall playing=" + EditorApplication.isPlaying + " marker=" + File.Exists(marker) + "\n");
            if (!File.Exists(marker)) return;
            try { File.Delete(marker); } catch { return; }
            EditorPrefs.SetBool("Veinfire.Smoke", true);
            File.AppendAllText(kick, "prefs set, entering play\n");
            if (EditorApplication.isPlaying)
            {
                if (Object.FindFirstObjectByType<ContentSmoke>() == null)
                    new GameObject("ContentSmoke").AddComponent<ContentSmoke>();
                return;
            }

            EditorApplication.isPlaying = true;
        }
    }
}
