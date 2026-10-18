using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;

    private void OnTriggerEnter(Collider other)
    {
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        KeyInventory inventory = other.GetComponent<KeyInventory>();
        if (inventory != null)
            inventory.AddKey();

        Destroy(gameObject);
    }
}