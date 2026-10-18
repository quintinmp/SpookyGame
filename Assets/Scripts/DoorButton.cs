using System.Collections;
using UnityEngine;

public class DoorButton : MonoBehaviour, IInteractable
{
    [SerializeField] private SlidingDoor door;
    [SerializeField] private string prompt = " ";

    public string InteractPrompt => door.IsOpen ? "Close" : "Open";

    public void Interact()
    {
        if (door.IsOpen) door.Close();
        else door.Open();
    }


}