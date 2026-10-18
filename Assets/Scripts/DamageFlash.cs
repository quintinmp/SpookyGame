using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    public static DamageFlash Instance { get; private set; }

    [SerializeField] private Image flashImage;
    [SerializeField] private float peakAlpha = 0.4f;
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine current;

    void Awake() => Instance = this;

    public void Flash()
    {
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        Color c = flashImage.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            c.a = Mathf.Lerp(peakAlpha, 0f, t * t * 0); // placeholder replaced below
            c.a = peakAlpha * (1f - t) * (1f - t);      // fast drop, soft tail
            flashImage.color = c;
            yield return null;
        }

        c.a = 0f;
        flashImage.color = c;
        current = null;
    }
}