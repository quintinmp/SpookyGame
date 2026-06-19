using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light flashlight;
    [SerializeField] private FlashlightEquip flashlightEquip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    [Header("Flicker")]
    [SerializeField] private float flickerDuration = 2f;
    [SerializeField] private Vector2 flickerInterval = new Vector2(0.05f, 0.15f);

    private bool _isOn = false;

    private void Start()
    {
        flashlight.enabled = false;
        flashlightEquip.Unequip();
    }

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
            Toggle();
    }

    private void Toggle()
    {
        _isOn = !_isOn;
        flashlight.enabled = _isOn;

        if (_isOn) flashlightEquip.Equip();
        else flashlightEquip.Unequip();

        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void StartFlicker()
    {
        if (!flashlight.enabled) return;
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        float elapsed = 0f;
        bool originalState = flashlight.enabled;

        while (elapsed < flickerDuration)
        {
            flashlight.enabled = !flashlight.enabled;
            float wait = Random.Range(flickerInterval.x, flickerInterval.y);
            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }

        flashlight.enabled = originalState;
    }
}