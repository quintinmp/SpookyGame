using System.Collections;
using UnityEngine;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delay = 0f;

    private IEnumerator Start()
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        yield return StartCoroutine(ScreenFade.Instance.FadeIn(fadeDuration));
    }
}