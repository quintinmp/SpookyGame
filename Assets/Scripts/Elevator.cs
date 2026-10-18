using System.Collections;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private SlidingDoor door;

    [Header("Audio")]
    [SerializeField] private AudioSource elevatorSource;
    [SerializeField] private AudioClip elevatorHum;
    [SerializeField] private AudioClip arrivalSound;
    [SerializeField] private float arrivalSoundVolume = .5f;

    [Header("Timing")]
    [SerializeField] private float screenFadeDuration = 1.5f;
    [SerializeField] private float rideTime = 2.5f;
    [SerializeField] private float arrivalPause = 1.5f;

    [Header("Active Button FX")]
    [SerializeField] private ElevatorButton[] buttons;
    [SerializeField] private string startFloor = "MainFloorClown";

    private bool _busy = false;
    private string _currentFloor;

    // Opening: plays once at game start. Bootstrap loads once, so this runs once.
    void Start() => StartCoroutine(Intro());

    private IEnumerator Intro()
    {
        _busy = true;
        ScreenFade.Instance.SetAlpha(1f);
        BootstrapLoader.Instance.SetPlayerInputActive(false);

        StartHum();
        yield return StartCoroutine(ScreenFade.Instance.FadeIn(screenFadeDuration));
        BootstrapLoader.Instance.SetPlayerInputActive(true);

        yield return new WaitForSeconds(rideTime);
        yield return Arrive();
        _currentFloor = startFloor;
        RefreshButtons(startFloor);
        _busy = false;
    }

    // Called by a floor button.
    public void GoToFloor(string scene)
    {
        if (_busy) return;
        if (scene == _currentFloor) return;
        StartCoroutine(Ride(scene));
    }

    private IEnumerator Ride(string scene)
    {
        _busy = true;
        door.Close();
        StartHum();

        yield return new WaitForSeconds(rideTime);

        BootstrapLoader.Instance.LoadRoom(scene, "", false); // swap floor, player stays in elevator

        yield return Arrive();
        _currentFloor = scene;
        RefreshButtons(scene);
        _busy = false;
    }

    private IEnumerator Arrive()
    {
        elevatorSource.Stop();
        if (arrivalSound != null) elevatorSource.PlayOneShot(arrivalSound, arrivalSoundVolume);
        yield return new WaitForSeconds(arrivalPause);
        door.Open();
    }

    private void StartHum()
    {
        if (elevatorHum == null) return;
        elevatorSource.clip = elevatorHum;
        elevatorSource.loop = true;
        elevatorSource.Play();
    }

    private void RefreshButtons(string currentScene)
    {
        foreach (var b in buttons)
            b.SetLit(b.TargetScene == currentScene);
    }
}