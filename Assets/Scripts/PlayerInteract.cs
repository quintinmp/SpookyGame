using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public Camera playerCamera;
    [SerializeField] private TextMeshProUGUI promptUI;

    void Start()
    {
        if (promptUI != null) promptUI.enabled = false;
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange)
            && hit.collider.TryGetComponent(out IInteractable interactable))
        {
            promptUI.text = interactable.InteractPrompt;
            promptUI.enabled = true;

            if (Keyboard.current.eKey.wasPressedThisFrame)
                interactable.Interact();
        }
        else
        {
            promptUI.enabled = false;
        }
    }
}