using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject hitEffectPrefab;
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

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (isDead) return;

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
}