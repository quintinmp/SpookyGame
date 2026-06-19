using EasyDoorSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public Camera playerCamera;
    [SerializeField] private TextMeshProUGUI doorPromptUI;



    private void Start()
    {
       doorPromptUI.enabled = false;
    }


    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            DoorInteractable doorInteractable = hit.collider.GetComponent<DoorInteractable>();
            EasyDoor door = hit.collider.GetComponent<EasyDoor>();

            if (doorInteractable != null && doorInteractable.IsLocked)
                doorPromptUI.text = "Locked";
            else if (door != null)
                doorPromptUI.text = door.IsOpen ? "Close" : "Open";
            doorPromptUI.enabled = interactable != null;

            if (interactable != null && Keyboard.current.eKey.wasPressedThisFrame)
                interactable.Interact();
        }
        else
        {
            doorPromptUI.enabled = false;
        }
    }


}