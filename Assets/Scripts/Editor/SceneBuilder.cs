using System.IO;
using Tilevault.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tilevault.Editor
{
    /// <summary>
    /// Generates the one scene the game ships. The scene holds a single object;
    /// the camera, canvas, event system and every screen are built by
    /// <see cref="App"/> at runtime, which keeps the interface reviewable as code
    /// instead of as a YAML blob nobody can diff.
    /// </summary>
    public static class SceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Tilevault/Rebuild Main Scene")]
        public static void RebuildMainScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("App");
            root.AddComponent<App>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            AssetDatabase.Refresh();
            Debug.Log($"[Tilevault] Wrote {ScenePath}");
        }

        /// <summary>Command-line entry point: <c>-executeMethod Tilevault.Editor.SceneBuilder.CI</c>.</summary>
        public static void CI()
        {
            RebuildMainScene();
            EditorApplication.Exit(0);
        }
    }
}
