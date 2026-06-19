using UnityEngine;
using EasyDoorSystem;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    public AudioClip lockedSound;
    [Range(0f, 1f)] public float lockedVolume = 1f;

    private EasyDoor door;
    private AudioSource audioSource;
    private bool locked = false;

    void Start()
    {
        door = GetComponent<EasyDoor>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    public bool IsLocked => locked;
    public void Lock() => locked = true;

    public void Interact()
    {
        if (locked)
        {
            audioSource.PlayOneShot(lockedSound, lockedVolume);
            return;
        }
        door.ToggleDoor();
    }
}