using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float deathFadeDuration = 1.5f;

    // PLAYER TAKE DAMAGE
    [SerializeField] private CinemachineImpulseSource hitImpulse;
    [SerializeField] private PlayerKnockback knockback;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] hurtClips;
    [SerializeField] private AudioClip deathClip;

    private int currentHealth;
    private bool isDead;
    public bool IsDead => isDead;
    public float HealthPercent => (float)currentHealth / maxHealth;
    public event System.Action<float> OnHealthChanged;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        Debug.Log($"Player hit for {amount}. Health: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(HealthPercent);

        if (hitImpulse != null) hitImpulse.GenerateImpulse();
        if (DamageFlash.Instance != null) DamageFlash.Instance.Flash();


        if (knockback != null) knockback.Apply(sourcePosition);

        if (currentHealth == 0)
        {
            PlayClip(deathClip);
            StartCoroutine(Die());
        }
        else if (hurtClips != null && hurtClips.Length > 0)
        {
            PlayClip(hurtClips[Random.Range(0, hurtClips.Length)]);
        }
    }

    void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    IEnumerator Die()
    {
        isDead = true;
        Debug.Log("Player died");

        BootstrapLoader.Instance.SetPlayerInputActive(false);
        yield return ScreenFade.Instance.FadeOut(deathFadeDuration);
        yield return new WaitForSeconds(0.5f);

        BootstrapLoader.Instance.ReloadAll();
    }
}