using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BloodPool : MonoBehaviour
{
    [SerializeField] private GameObject poolPrefab;
    [SerializeField] private float spawnDelay = 1.2f;    // after death, roughly when the body lands
    [SerializeField] private float startSize = 0.2f;
    [SerializeField] private float maxSize = 1.8f;
    [SerializeField] private float growDuration = 6f;
    [SerializeField] private LayerMask groundMask = ~0;  // exclude the Enemy layer

    private EnemyHealth health;
    private Animator animator;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
    }

    void OnEnable() { if (health != null) health.OnDied += HandleDied; }
    void OnDisable() { if (health != null) health.OnDied -= HandleDied; }

    void HandleDied() => StartCoroutine(SpawnPool());

    IEnumerator SpawnPool()
    {
        yield return new WaitForSeconds(spawnDelay);

        // Spawn under the torso, not the root (the body falls away from the root)
        Transform hips = animator != null ? animator.GetBoneTransform(HumanBodyBones.Hips) : null;
        Vector3 origin = (hips != null ? hips.position : transform.position) + Vector3.up * 0.5f;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 3f,
            groundMask, QueryTriggerInteraction.Ignore))
            yield break;

        Quaternion rotation = Quaternion.LookRotation(-hit.normal)
                            * Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        GameObject pool = Instantiate(poolPrefab, hit.point + hit.normal * 0.01f, rotation);

        DecalProjector projector = pool.GetComponent<DecalProjector>();
        if (projector == null) yield break;

        float depth = projector.size.z;
        float elapsed = 0f;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / growDuration);
            float eased = 1f - (1f - t) * (1f - t);   // spreads fast, then slows
            float s = Mathf.Lerp(startSize, maxSize, eased);
            projector.size = new Vector3(s, s, depth);
            yield return null;
        }
    }
}