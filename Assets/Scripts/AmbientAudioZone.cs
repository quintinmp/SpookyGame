using System.Collections;
using UnityEngine;

public class AmbientAudioZone : MonoBehaviour
{
    public AudioClip ambientClip;
    public float fadeDuration = 1.5f;
    [Range(0f, 1f)] public float targetVolume = 0.5f;

    private static AudioSource currentSource;
    private static Coroutine fadeCoroutine;
    private static MonoBehaviour runner;
    public bool playOnStart = false;

    private void Start()
    {


        if (currentSource == null)
        {
            currentSource = gameObject.AddComponent<AudioSource>();
            currentSource.loop = true;
            if (playOnStart)
            {
                currentSource.clip = ambientClip;
                currentSource.volume = targetVolume;
                currentSource.Play();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (fadeCoroutine != null) runner.StopCoroutine(fadeCoroutine);
        runner = this;
        fadeCoroutine = StartCoroutine(CrossFade(ambientClip));
    }

    IEnumerator CrossFade(AudioClip newClip)
    {
        // Fade out current
        float startVolume = currentSource.volume;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            currentSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        currentSource.clip = newClip;
        currentSource.Play();

        // Fade in new
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            currentSource.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }
    }
}