using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ambientTrack;
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    public void Play()
    {
        if (ambientTrack == null)
        {
            Debug.LogWarning("AmbientAudioManager: No ambient track assigned.");
            return;
        }

        audioSource.clip = ambientTrack;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public AudioSource GetAudioSource() => audioSource;
}