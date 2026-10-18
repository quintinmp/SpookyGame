using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    public static BootstrapLoader Instance { get; private set; }

    [SerializeField] private string sceneToLoad;
    [SerializeField] private Transform player;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private string spawnID = "Default";
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private bool placePlayerOnStart = false; // off = player stays in elevator at boot

    private string currentRoom = "";
    private bool isLoading = false;
    public string CurrentRoom => currentRoom;

    void Awake() => Instance = this;
    public Transform Player => player;

    void Start()
    {
        string scene = sceneToLoad;
#if UNITY_EDITOR
        string openScene = UnityEditor.EditorPrefs.GetString(
            PlayFromBootstrap.OpenScenePrefKey, "");
        if (!string.IsNullOrEmpty(openScene))
            scene = openScene;
#endif
        if (string.IsNullOrEmpty(scene)) return;
        LoadRoom(scene, spawnID, placePlayerOnStart);
    }

    public void LoadRoom(string sceneName, string id, bool placeAtSpawn = true)
    {
        if (isLoading) return;
        StartCoroutine(LoadAndPlace(sceneName, id, placeAtSpawn));
    }

    IEnumerator LoadAndPlace(string sceneName, string id, bool placeAtSpawn)
    {
        isLoading = true;
        string previous = currentRoom;

        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
        }

        if (placeAtSpawn)
            PlacePlayer(sceneName, id);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        currentRoom = sceneName;

        if (!string.IsNullOrEmpty(previous) && previous != sceneName)
            yield return SceneManager.UnloadSceneAsync(previous);

        isLoading = false;
    }

    void PlacePlayer(string sceneName, string id)
    {
        foreach (SpawnPoint spawn in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
        {
            if (spawn.gameObject.scene.name != sceneName) continue;
            if (spawn.SpawnID != id) continue;

            if (playerController != null) playerController.enabled = false;
            player.position = spawn.transform.position;
            player.rotation = spawn.transform.rotation;
            if (playerController != null) playerController.enabled = true;
            return;
        }

        Debug.LogWarning($"SpawnPoint '{id}' not found in scene '{sceneName}'");
    }

    public void SetPlayerInputActive(bool active)
    {
        if (playerInput == null) return;
        if (active) playerInput.ActivateInput();
        else playerInput.DeactivateInput();
    }

    public void ReloadAll()
    {
        SceneManager.LoadScene(gameObject.scene.name, LoadSceneMode.Single);
    }
}