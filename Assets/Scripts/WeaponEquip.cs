using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponEquip : MonoBehaviour
{
    [SerializeField] private float equipAngle = 12f;
    [SerializeField] private float unequipAngle = -33f;
    [SerializeField] private float equipY = 0f;
    [SerializeField] private float unequipY = -0.3f;
    [SerializeField] private float duration = 0.3f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip equipSound;

    private bool _equipped = true;
    private Coroutine _rotateCurrent;
    private Coroutine _dipCurrent;
    private float _currentAngle;
    private float _currentY;

    private void Start()
    {
        _currentAngle = equipAngle;
        _currentY = transform.localPosition.y;
        equipY = _currentY;
        Unequip();
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            Toggle();
    }

    public void Equip()
    {
        if (_rotateCurrent != null) StopCoroutine(_rotateCurrent);
        if (_dipCurrent != null) StopCoroutine(_dipCurrent);
        _rotateCurrent = StartCoroutine(Rotate(equipAngle));
        _dipCurrent = StartCoroutine(Dip(equipY));
        _equipped = true;

        if (equipSound != null)
            audioSource.PlayOneShot(equipSound);
    }

    public void Unequip()
    {
        if (_rotateCurrent != null) StopCoroutine(_rotateCurrent);
        if (_dipCurrent != null) StopCoroutine(_dipCurrent);
        _rotateCurrent = StartCoroutine(Rotate(unequipAngle));
        _dipCurrent = StartCoroutine(Dip(unequipY));
        _equipped = false;
    }

    private void Toggle()
    {
        if (_equipped) Unequip();
        else Equip();
    }

    private IEnumerator Rotate(float targetAngle)
    {
        float startAngle = _currentAngle;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float angle = Mathf.Lerp(startAngle, targetAngle, t);
            _currentAngle = angle;
            transform.localEulerAngles = new Vector3(4.631f, 280.189f, angle);
            yield return null;
        }
        _currentAngle = targetAngle;
        transform.localEulerAngles = new Vector3(4.631f, 280.189f, targetAngle);
    }

    private IEnumerator Dip(float targetY)
    {
        float startY = _currentY;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float y = Mathf.Lerp(startY, targetY, t);
            _currentY = y;
            Vector3 pos = transform.localPosition;
            transform.localPosition = new Vector3(pos.x, y, pos.z);
            yield return null;
        }
        _currentY = targetY;
        transform.localPosition = new Vector3(transform.localPosition.x, targetY, transform.localPosition.z);
    }


    public bool IsEquipped => _equipped;
}