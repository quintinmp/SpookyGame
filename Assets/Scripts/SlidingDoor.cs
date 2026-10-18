using System.Collections;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Header("Slide")]
    [SerializeField] private Vector3 openOffset = new Vector3(3f, 0f, 0f); // local slide into wall
    [SerializeField] private float slideDuration = 1.5f;

    [Header("Auto-close")]
    [SerializeField] private bool autoClose = false;
    [SerializeField] private float autoCloseDelay = 8f;

    [Header("Audio")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Vector3 _closedPos;
    private Vector3 _openPos;
    private Coroutine _move;
    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private Coroutine _autoCloseRoutine;

    void Awake()
    {
        _closedPos = transform.position;
        // offset is in the door's own local axes, so a rotated door still slides correctly
        _openPos = _closedPos + transform.TransformVector(openOffset);
    }

    public void Open()
    {
        if (openSound != null) doorAudioSource.PlayOneShot(openSound);
        if (_move != null) StopCoroutine(_move);
        _move = StartCoroutine(Slide(_openPos));
        isOpen = true;

        if (autoClose)
        {
            if (_autoCloseRoutine != null) StopCoroutine(_autoCloseRoutine);
            _autoCloseRoutine = StartCoroutine(AutoCloseTimer());
        }
    }

    public void Close()
    {
        if (_autoCloseRoutine != null) StopCoroutine(_autoCloseRoutine);
        if (closeSound != null) doorAudioSource.PlayOneShot(closeSound);
        if (_move != null) StopCoroutine(_move);
        _move = StartCoroutine(Slide(_closedPos));
        isOpen = false;
    }

    private IEnumerator AutoCloseTimer()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        Close();
    }

    private IEnumerator Slide(Vector3 target)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.position = target;
    }
}