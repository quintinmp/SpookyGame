using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneDebugOverlay : MonoBehaviour
{
    [SerializeField] private string excludeScene = "Bootstrap";
    [SerializeField] private string defaultSpawnID = "Default";
    [SerializeField] private PlayerInput playerInput;

    private bool isOpen = false;
    private Vector2 scrollPos;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
            SetOpen(!isOpen);
    }

    void OnGUI()
    {
        if (!isOpen) return;

        GUILayout.BeginArea(new Rect(20, 20, 260, 500), GUI.skin.box);
        GUILayout.Label("[ SCENES ]");

        string current = BootstrapLoader.Instance != null ? BootstrapLoader.Instance.CurrentRoom : "—";
        GUILayout.Label($"Current: {current}");

        scrollPos = GUILayout.BeginScrollView(scrollPos);
        foreach (string sceneName in GetRoomScenes())
        {
            bool isCurrent = sceneName == current;
            GUI.enabled = !isCurrent;
            if (GUILayout.Button(isCurrent ? $"► {sceneName}" : sceneName, GUILayout.Height(28)))
            {
                BootstrapLoader.Instance?.LoadRoom(sceneName, defaultSpawnID);
                SetOpen(false);
            }
            GUI.enabled = true;
        }
        GUILayout.EndScrollView();

        if (GUILayout.Button("Close [ ` ]")) SetOpen(false);
        GUILayout.EndArea();
    }

    string[] GetRoomScenes()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        var result = new System.Collections.Generic.List<string>();
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == excludeScene) continue;
            result.Add(name);
        }
        return result.ToArray();
    }

    void SetOpen(bool open)
    {
        isOpen = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
        if (playerInput != null) playerInput.enabled = !open;
    }
}