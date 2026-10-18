using System.Collections;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private float distance = 1f;
    [SerializeField] private float duration = 0.2f;

    private Coroutine current;

    public void Apply(Vector3 fromPosition)
    {
        Vector3 dir = transform.position - fromPosition;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = -transform.forward;
        dir.Normalize();

        if (current != null) StopCoroutine(current);
        current = StartCoroutine(Push(dir));
    }

    IEnumerator Push(Vector3 dir)
    {
        float elapsed = 0f;
        float prev = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - t) * (1f - t);   // strong shove, then settles

            controller.Move(dir * (eased - prev) * distance);
            prev = eased;
            yield return null;
        }

        current = null;
    }
}