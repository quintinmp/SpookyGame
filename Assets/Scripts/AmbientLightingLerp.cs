using System.Collections;
using UnityEngine;

public class AmbientLightingLerp : MonoBehaviour
{
    private bool playerIndoors = false;
    private static Coroutine lerpCoroutine;
    private static MonoBehaviour runner;
    public Color ambientColor;

    private void OnTriggerEnter(Collider other)
    {
        if (lerpCoroutine != null) runner.StopCoroutine(lerpCoroutine);
        runner = this;
        lerpCoroutine = StartCoroutine(LerpAmbient(RenderSettings.ambientLight, ambientColor, 1f));
    }

    IEnumerator LerpAmbient(Color from, Color to, float duration)
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;

            RenderSettings.ambientLight = Color.Lerp(from, to, t);
            yield return null;
        }
    }

    private void ToggleIndoors()
    {
        playerIndoors = !playerIndoors;
    }
}