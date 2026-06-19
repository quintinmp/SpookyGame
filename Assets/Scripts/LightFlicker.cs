using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float minInterval = 0.05f;
    public float maxInterval = 0.2f;

    private Light flickerLight;

    void Start()
    {
        flickerLight = GetComponent<Light>();
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            flickerLight.intensity = Random.Range(minIntensity, maxIntensity);
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }
}