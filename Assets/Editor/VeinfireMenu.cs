#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Veinfire.EditorTools
{
    public static class VeinfireMenu
    {
        const string ScenePath = "Assets/Scenes/Prototype.unity";

        [MenuItem("Veinfire/创建原型场景")]
        public static void CreatePrototypeScene()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var bootstrap = new GameObject("GameInstaller");
            bootstrap.AddComponent<GameInstaller>();
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
            EditorSceneManager.OpenScene(ScenePath);
            Debug.Log("Veinfire 2.5D 原型场景已创建。Play：WASD 在地面移动，自动射击，R 复活。");
        }

        [MenuItem("Veinfire/打开开发说明")]
        public static void OpenReadme()
        {
            var readme = AssetDatabase.LoadAssetAtPath<Object>("Assets/../README.md");
            if (readme != null) AssetDatabase.OpenAsset(readme);
            else EditorUtility.DisplayDialog("Veinfire", "请在资源管理器中打开项目根目录的 README.md", "好");
        }
    }
}
#endif
