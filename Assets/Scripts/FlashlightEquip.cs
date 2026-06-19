using UnityEngine;
using System.Collections;

public class FlashlightEquip : MonoBehaviour
{
    [SerializeField] private float equipAngle = 60f;
    [SerializeField] private float unequipAngle = 0f;
    [SerializeField] private float duration = 0.3f;

    private bool _equipped = true;
    private Coroutine _current;

    public void Equip()
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(Rotate(equipAngle));
        _equipped = true;
    }

    public void Unequip()
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(Rotate(unequipAngle));
        _equipped = false;
    }

    public void Toggle()
    {
        if (_equipped) Unequip();
        else Equip();
    }

    private IEnumerator Rotate(float targetAngle)
    {
        float startAngle = transform.localEulerAngles.x;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float angle = Mathf.Lerp(startAngle, targetAngle, t);
            transform.localEulerAngles = new Vector3(angle, 0f, 0f);
            yield return null;
        }
        transform.localEulerAngles = new Vector3(targetAngle, 0f, 0f);
    }
}