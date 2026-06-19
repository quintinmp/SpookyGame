using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BunkerSceneIntro : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ScreenFade screenFade;
    [SerializeField] private AmbientAudioManager ambientAudioManager;
    [SerializeField] private PlayerInput playerInput;

    [Header("Intro SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip carDoorClip;

    [Header("Timing")]
    [SerializeField] private float ambientFadeInDuration = 0.5f;
    [SerializeField] private float carDoorDelay = 1.5f;
    [SerializeField] private float screenFadeDelay = 2.5f;
    [SerializeField] private float screenFadeDuration = 1.5f;

    [Header("Ambient")]
    [SerializeField][Range(0f, 1f)] private float ambientTargetVolume = 0.3f;

    private void Start()
    {
        // Make sure screen is black and player can't move
        screenFade.SetAlpha(1f);
        playerInput.DeactivateInput();

        StartCoroutine(RunIntro());
    }

    private IEnumerator RunIntro()
    {
        // Start ambient quietly
        ambientAudioManager.Play();
        yield return StartCoroutine(FadeAmbientIn(ambientFadeInDuration));

        // Wait then play car door
        yield return new WaitForSeconds(carDoorDelay);
        if (carDoorClip != null)
            sfxSource.PlayOneShot(carDoorClip);

        // Wait then fade screen in
        yield return new WaitForSeconds(screenFadeDelay - carDoorDelay);
        yield return StartCoroutine(screenFade.FadeIn(screenFadeDuration));

        // Intro done, give player control
        playerInput.ActivateInput();
    }

    private IEnumerator FadeAmbientIn(float duration)
    {
        float elapsed = 0f;
        AudioSource ambientSource = ambientAudioManager.GetAudioSource();

        ambientSource.volume = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ambientSource.volume = Mathf.Lerp(0f, ambientTargetVolume, elapsed / duration);
            yield return null;
        }

        ambientSource.volume = ambientTargetVolume;
    }
}