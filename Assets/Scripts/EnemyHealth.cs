using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Blood")]
    [SerializeField] private GameObject[] bloodDecals;
    [SerializeField] private float splatterRange = 3f;
    [SerializeField] private Vector2 splatterSizeRange = new Vector2(0.7f, 1.3f);
    [SerializeField] private LayerMask splatterMask = ~0;   // exclude the Enemy layer
    public GameObject HitEffectPrefab => hitEffectPrefab;

    private int currentHealth;
    private bool isDead;
    public bool IsDead => isDead;
    public event System.Action<int, Vector3> OnDamaged;
    public event System.Action OnDied;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isDead) return;

        SpawnSplatter(hitPoint, hitDirection);

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        Debug.Log($"{name} hit for {amount}. Health: {currentHealth}/{maxHealth}");

        if (currentHealth == 0)
        {
            isDead = true;
            Debug.Log($"{name} died");
            OnDied?.Invoke();
        }
        else
        {
            OnDamaged?.Invoke(amount, hitPoint);
        }
    }

    void SpawnSplatter(Vector3 hitPoint, Vector3 direction)
    {
        if (bloodDecals == null || bloodDecals.Length == 0) return;

        if (!Physics.Raycast(hitPoint + direction * 0.05f, direction, out RaycastHit hit,
            splatterRange, splatterMask, QueryTriggerInteraction.Ignore))
            return;

        GameObject prefab = bloodDecals[Random.Range(0, bloodDecals.Length)];
        BloodDecalSettings settings = prefab.GetComponent<BloodDecalSettings>();

        bool isWall = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.7f;

        Quaternion rotation;
        if (settings != null && settings.lockUpright && isWall)
        {
            // Upright on walls: drips point down
            rotation = Quaternion.LookRotation(-hit.normal, Vector3.up)
                     * Quaternion.Euler(0f, 0f, settings.rollOffset);
        }
        else
        {
            rotation = Quaternion.LookRotation(-hit.normal)
                     * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        }

        GameObject decal = Instantiate(prefab, hit.point, rotation);

        DecalProjector projector = decal.GetComponent<DecalProjector>();
        if (projector != null)
        {
            float s = Random.Range(splatterSizeRange.x, splatterSizeRange.y);
            Vector3 size = projector.size;
            Vector3 pivot = projector.pivot;
            projector.size = new Vector3(size.x * s, size.y * s, size.z);
            projector.pivot = new Vector3(pivot.x * s, pivot.y * s, pivot.z);
        }

        decal.transform.SetParent(hit.collider.transform, true);
    }
}