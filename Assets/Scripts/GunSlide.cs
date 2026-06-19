using System.Collections;
using UnityEngine;

public class GunSlide : MonoBehaviour
{
    [SerializeField] private float slideDistance = -2f;
    [SerializeField] private float slideDuration = 0.05f;
    [SerializeField] private float returnDuration = 0.1f;

    private Vector3 _restPosition;
    private Coroutine _current;

    private void Start()
    {
        _restPosition = transform.localPosition;
    }

    public void Pull()
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(Slide());
    }

    private IEnumerator Slide()
    {
        Vector3 slidPos = new Vector3(_restPosition.x + slideDistance, _restPosition.y, _restPosition.z);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / slideDuration;
            transform.localPosition = Vector3.Lerp(_restPosition, slidPos, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / returnDuration;
            transform.localPosition = Vector3.Lerp(slidPos, _restPosition, t);
            yield return null;
        }

        transform.localPosition = _restPosition;
    }
}