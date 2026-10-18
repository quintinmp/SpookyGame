using UnityEngine;
using UnityEngine.Rendering;

public class LowHealthEffects : MonoBehaviour
{
    [SerializeField] private PlayerHealth health;
    [SerializeField] private Volume lowHealthVolume;
    [SerializeField] private float startThreshold = 0.7f; // effects begin below 70% health
    [SerializeField] private float blendSpeed = 2f;

    [Header("Heartbeat")]
    [SerializeField] private AudioSource heartbeatSource;
    [SerializeField] private float maxHeartbeatVolume = 0.8f;
    [SerializeField] private Vector2 heartbeatPitch = new Vector2(0.9f, 1.2f); // slow → fast

    private float targetWeight;

    void OnEnable() => health.OnHealthChanged += HandleHealthChanged;
    void OnDisable() => health.OnHealthChanged -= HandleHealthChanged;

    void Start()
    {
        lowHealthVolume.weight = 0f;
    }

    void HandleHealthChanged(float percent)
    {
        targetWeight = Mathf.InverseLerp(startThreshold, 0f, percent);
    }

    void Update()
    {
        lowHealthVolume.weight = Mathf.MoveTowards(
            lowHealthVolume.weight, targetWeight, blendSpeed * Time.deltaTime);

        float w = lowHealthVolume.weight;

        if (heartbeatSource != null)
        {
            heartbeatSource.volume = w * maxHeartbeatVolume;
            heartbeatSource.pitch = Mathf.Lerp(heartbeatPitch.x, heartbeatPitch.y, w);

            if (w > 0.01f && !heartbeatSource.isPlaying) heartbeatSource.Play();
            else if (w <= 0.01f && heartbeatSource.isPlaying) heartbeatSource.Stop();
        }
    }
}