using UnityEngine;

// Runtime counterpart to the editor-only BootstrapAlwaysLoaded suppressor.
// That editor script disables Bootstrap's Camera/AudioListener/Canvas for editing and
// serializes the off-state into the scene ? so a build ships them disabled with nothing
// to turn them back on. This forces them on at runtime (Awake): a harmless no-op in
// editor Play, the actual guarantee in a build. The runtime owns the on-state here
// instead of trusting whatever the last scene save happened to leave behind.
//
// Drop it on ANY object in the Bootstrap scene (root or child) ? it walks the whole
// scene, so its own location doesn't matter. Leaves the editor suppressor untouched.
public class BootstrapEnableRuntime : MonoBehaviour
{
    [Header("Force-enable on Awake")]
    [SerializeField] private bool enableCameras = true;
    [SerializeField] private bool enableAudioListeners = true;
    [SerializeField] private bool enableCanvases = true;

    void Awake()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (enableCameras)
                foreach (Camera cam in root.GetComponentsInChildren<Camera>(true))
                    if (!cam.CompareTag("EditorOnly")) cam.enabled = true;   // never revive the edit-mode cam

            if (enableAudioListeners)
                foreach (AudioListener al in root.GetComponentsInChildren<AudioListener>(true))
                    if (!al.CompareTag("EditorOnly")) al.enabled = true;

            if (enableCanvases)
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                    if (!canvas.CompareTag("EditorOnly")) canvas.enabled = true;
        }
    }
}