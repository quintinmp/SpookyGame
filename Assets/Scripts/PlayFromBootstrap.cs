using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromBootstrap
{
    public const string OpenScenePrefKey = "PlayFromBootstrap_OpenScene";

    static PlayFromBootstrap()
    {
        string[] guids = AssetDatabase.FindAssets("t:SceneAsset Bootstrap");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            EditorSceneManager.playModeStartScene = scene;
        }

        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var active = EditorSceneManager.GetActiveScene();
            // If Bootstrap itself is open, record nothing so the loader just does Bootstrap alone.
            string name = active.name == "Bootstrap" ? "" : active.name;
            EditorPrefs.SetString(OpenScenePrefKey, name);
        }
    }
}