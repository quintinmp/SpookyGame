using System.Collections;
using UnityEngine;
using EasyDoorSystem;

public class BasementDoorScare : MonoBehaviour
{
    public EasyDoor door;
    public DoorInteractable doorInteractable;
    public AudioClip slamSound;
    public float slamDelay = 0.2f;
    public float slamVolume = 1f;
    public Light scareLight;
    public GameObject lightCapsule;

    private AudioSource audioSource;
    private bool triggered = false;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(SlamDoor());
    }

    IEnumerator SlamDoor()
    {
        yield return new WaitForSeconds(slamDelay);
        door.CloseDoor();

        yield return new WaitForSeconds(slamDelay);
        audioSource.PlayOneShot(slamSound, slamVolume);
        scareLight.enabled = false;
        lightCapsule.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
        doorInteractable.Lock();
    }
}