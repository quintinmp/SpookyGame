using System.Collections;
using UnityEngine;

public class DoorwayScare : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject monster;
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scareSound;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 4f;

    [SerializeField] private FlashlightToggle flashlightToggle;

    private Animator _animator;
    private bool _triggered = false;
    private Vector3 _hiddenPosition = new Vector3(0f, -1000f, 0f);

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        StartCoroutine(RunScare());
    }

    private void Start()
    {
        _animator = monster.GetComponent<Animator>();
        monster.transform.position = _hiddenPosition;
    }

    private IEnumerator RunScare()
    {
        flashlightToggle.StartFlicker();

        monster.transform.position = pointA.position;
        monster.transform.LookAt(pointB.position);

        _animator.SetFloat("Speed", 3f);

        if (scareSound != null)
            audioSource.PlayOneShot(scareSound, .5f);

        while (Vector3.Distance(monster.transform.position, pointB.position) > 0.05f)
        {
            monster.transform.position = Vector3.MoveTowards(
                monster.transform.position,
                pointB.position,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        _animator.SetFloat("Speed", 0f);
        monster.transform.position = _hiddenPosition;
    }
}