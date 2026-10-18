using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance { get; private set; }

    [SerializeField] private Image fadeImage;

    public bool IsFading { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public IEnumerator FadeIn(float duration) => Fade(1f, 0f, duration);
    public IEnumerator FadeOut(float duration) => Fade(0f, 1f, duration);

    private IEnumerator Fade(float from, float to, float duration)
    {
        IsFading = true;
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t;             
            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
        IsFading = false;
    }

    public void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}