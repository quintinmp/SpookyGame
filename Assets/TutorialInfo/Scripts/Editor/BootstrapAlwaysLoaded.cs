#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BootstrapAlwaysLoaded
{
    const string BootstrapName = "Bootstrap";   // change if your scene is named differently
    const string PrefKey = "BootstrapAlwaysLoaded_Enabled";

    static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    static BootstrapAlwaysLoaded()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += EnsureBootstrapLoaded;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;   // add this
    }

    static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingEditMode)
        {
            // About to play ? restore Bootstrap's real components.
            Scene bootstrap = SceneManager.GetSceneByName(BootstrapName);
            if (!bootstrap.IsValid() || !bootstrap.isLoaded) return;

            foreach (GameObject root in bootstrap.GetRootGameObjects())
            {
                foreach (Camera cam in root.GetComponentsInChildren<Camera>(true))
                    cam.enabled = true;
                foreach (AudioListener al in root.GetComponentsInChildren<AudioListener>(true))
                    al.enabled = true;
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                    canvas.enabled = true;
            }
        }
        else if (change == PlayModeStateChange.EnteredEditMode)
        {
            // Back from play ? suppress again for editing.
            if (Enabled) SuppressBootstrap();
        }
    }

    static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (!Enabled) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        // The additive Bootstrap load itself fires this event — don't recurse.
        if (scene.name == BootstrapName) return;

        // A Single open closed everything, so Bootstrap needs re-adding. Additive
        // opens leave existing scenes alone, so it's already there if it was loaded.
        if (mode == OpenSceneMode.Single)
            EditorApplication.delayCall += EnsureBootstrapLoaded;
    }

    static void EnsureBootstrapLoaded()
    {
        if (!Enabled) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        // Already loaded? Still make sure its edit-mode components are suppressed, then bail.
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).name == BootstrapName)
            {
                SuppressBootstrap();
                return;
            }

        string path = FindBootstrapPath();
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"[BootstrapAlwaysLoaded] No scene named '{BootstrapName}' found.");
            return;
        }

        Scene active = SceneManager.GetActiveScene();
        EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        if (active.IsValid() && active.isLoaded)
            SceneManager.SetActiveScene(active);

        SuppressBootstrap();
    }

    // Edit-mode only: turn off Bootstrap's camera/audio/canvas so they don't cover the room.
    static void SuppressBootstrap()
    {
        Scene bootstrap = SceneManager.GetSceneByName(BootstrapName);
        if (!bootstrap.IsValid() || !bootstrap.isLoaded) return;

        foreach (GameObject root in bootstrap.GetRootGameObjects())
        {
            foreach (Camera cam in root.GetComponentsInChildren<Camera>(true))
                cam.enabled = false;
            foreach (AudioListener al in root.GetComponentsInChildren<AudioListener>(true))
                al.enabled = false;
            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                canvas.enabled = false;
        }
    }

    static string FindBootstrapPath()
    {
        foreach (string guid in AssetDatabase.FindAssets($"t:Scene {BootstrapName}"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == BootstrapName)
                return path;
        }
        return null;
    }

    // Tools menu toggle — flip it off if it's ever in the way, no need to delete the script.
    [MenuItem("Tools/Bootstrap Always Loaded", false, 100)]
    static void Toggle() => Enabled = !Enabled;

    [MenuItem("Tools/Bootstrap Always Loaded", true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked("Tools/Bootstrap Always Loaded", Enabled);
        return true;
    }
}
#endif