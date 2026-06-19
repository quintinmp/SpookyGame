using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunShoot : MonoBehaviour
{
    [SerializeField] private float recoilAngle = 10f;
    [SerializeField] private float recoilDuration = 0.05f;
    [SerializeField] private float returnDuration = 0.1f;
    [SerializeField] private GunSlide gunSlide;
    [SerializeField] private GunEject gunEject;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Light muzzleLight;
    [SerializeField] private float muzzleLightDuration = 0.05f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private float fireClipVolume = 1f;
    [SerializeField] private SpriteRenderer muzzleFlashSprite;
    [SerializeField] private float spriteDuration = 0.05f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float range = 50f;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private float decalOffset = 0.01f;
    [SerializeField] private float spread = 0.02f;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private WeaponEquip weaponEquip;

    private Coroutine _recoilCurrent;
    private float _currentX;
    private float _currentY;
    private float _currentZ;
    private float _restZ;

    private void Start()
    {
        _currentX = transform.localEulerAngles.x;
        _currentY = transform.localEulerAngles.y;
        _currentZ = transform.localEulerAngles.z;
        _restZ = _currentZ;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && weaponEquip.IsEquipped)
            Shoot();
        if (Keyboard.current.tKey.wasPressedThisFrame)
            Time.timeScale = Time.timeScale == 1f ? 0.1f : 1f;
    }

    private void Shoot()
    {
        if (_recoilCurrent != null) StopCoroutine(_recoilCurrent);
        _recoilCurrent = StartCoroutine(Recoil());
        gunSlide.Pull();
        gunEject.Eject();
        muzzleFlash.Play();
        StartCoroutine(MuzzleLight());
        StartCoroutine(MuzzleSprite());
        if (audioSource != null)
        {
            float randomVolume = fireClipVolume + Random.Range(-.1f, .1f);
            audioSource.pitch = Random.Range(0.96f, 1.04f);
            audioSource.PlayOneShot(fireClip, randomVolume);
        }

        Vector3 direction = playerCamera.transform.forward;
        direction += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        );
        Ray ray = new Ray(playerCamera.transform.position, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
            Vector3 position = hit.point + hit.normal * decalOffset;
            Quaternion rotation = Quaternion.LookRotation(-hit.normal);
            rotation *= Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            Instantiate(bulletHolePrefab, position, rotation);
            Quaternion effectRotation = Quaternion.LookRotation(hit.normal);
            GameObject effect = Instantiate(hitEffectPrefab, position, effectRotation);
        }

    }

    private IEnumerator Recoil()
    {
        // kick
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / recoilDuration;
            float angle = Mathf.Lerp(_restZ, _restZ - recoilAngle, t);
            _currentZ = angle;
            transform.localEulerAngles = new Vector3(_currentX, _currentY, angle);
            yield return null;
        }

        // return
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / returnDuration;
            float angle = Mathf.Lerp(_restZ - recoilAngle, _restZ, t);
            _currentZ = angle;
            transform.localEulerAngles = new Vector3(_currentX, _currentY, angle);
            yield return null;
        }

        _currentZ = _restZ;
        transform.localEulerAngles = new Vector3(_currentX, _currentY, _restZ);
    }

    private IEnumerator MuzzleLight()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(muzzleLightDuration);
        muzzleLight.enabled = false;
    }

    private IEnumerator MuzzleSprite()
    {
        muzzleFlashSprite.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        Color c = muzzleFlashSprite.color;
        c.a = 1f;
        muzzleFlashSprite.color = c;
        muzzleFlashSprite.transform.localRotation = Quaternion.Euler(0f, 90f, Random.Range(0f, 360f));
        yield return new WaitForSeconds(spriteDuration);
        c.a = 0f;
        muzzleFlashSprite.color = c;
    }
}