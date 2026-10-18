using System.Collections;
using UnityEngine;

public class FirstClownScare : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject monster;
    [SerializeField] private Transform pointA;    
    [SerializeField] private Transform pointB;        
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip screamSound;
    [SerializeField] private AudioSource screamSource;   // dedicated, for the scream
    [SerializeField] private AudioClip backupSound;
    [SerializeField] private Light sceneLight;
    [SerializeField] private Renderer emissiveObject;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float pauseAfterJump = 0.3f;
    [SerializeField] private float pauseAfterScream = 0.3f;

    private Animator _animator;
    private bool _triggered = false;
    private Vector3 _hiddenPosition = new Vector3(0f, -1000f, 0f);
    private Material _emissiveMat;
    private Color _emissiveColor;
    private Transform player;

    private void Start()
    {
        _animator = monster.GetComponent<Animator>();
        monster.transform.position = _hiddenPosition;

        _emissiveMat = emissiveObject.material;             
        _emissiveColor = _emissiveMat.GetColor("_EmissionColor");

        player = BootstrapLoader.Instance.Player;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;
        _triggered = true;
        StartCoroutine(RunScare());
    }

    private IEnumerator RunScare()
    {
        sceneLight.enabled = false;
        _emissiveMat.SetColor("_EmissionColor", Color.black);
        _emissiveMat.DisableKeyword("_EMISSION");

        // jump out from the dark
        monster.transform.position = pointA.position;
        monster.transform.LookAt(player.position);

        _animator.Play("Forward_Jump");
        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound);
        yield return new WaitForSeconds(GetClipLength("Forward_Jump"));

        if (screamSound != null)
        {
            screamSource.clip = screamSound;
            screamSource.Play();
        }


        yield return new WaitForSeconds(pauseAfterJump);

        // scream, tracking player
        _animator.Play("Scream");

        float screamTime = GetClipLength("Scream");
        float t = 0f;
        while (t < screamTime)
        {
            monster.transform.LookAt(player.position);
            t += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(pauseAfterScream);

        // walk backward toward dark, still facing player
        _animator.Play("Walk_Back");
        screamSource.Stop();
        if (backupSound != null)
            audioSource.PlayOneShot(backupSound);
        while (Vector3.Distance(monster.transform.position, pointB.position) > 0.05f)
        {
            monster.transform.position = Vector3.MoveTowards(
                monster.transform.position,
                pointB.position,
                moveSpeed * Time.deltaTime
            );
            monster.transform.LookAt(player.position);
            yield return null;
        }

        // despawn
        monster.transform.position = _hiddenPosition;

        sceneLight.enabled = true;

        _emissiveMat.EnableKeyword("_EMISSION");
        _emissiveMat.SetColor("_EmissionColor", _emissiveColor);
    }

    private float GetClipLength(string clipName)
    {
        RuntimeAnimatorController rac = _animator.runtimeAnimatorController;
        foreach (AnimationClip clip in rac.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 0f;
    }
}