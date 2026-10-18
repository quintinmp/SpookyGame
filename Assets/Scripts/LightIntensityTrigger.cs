using UnityEngine;

public class LightIntensityTrigger : MonoBehaviour
{
    [SerializeField] private Light[] lights;
    [SerializeField] private float targetIntensity = 1f;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip lightOnSound;
    [SerializeField] private float lightOnSoundVolume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (Light light in lights)
            light.intensity = targetIntensity;
            if (source != null) source.PlayOneShot(lightOnSound, lightOnSoundVolume);
    }
}