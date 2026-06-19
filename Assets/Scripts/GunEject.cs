using System.Collections;
using UnityEngine;

public class GunEject : MonoBehaviour
{
    [SerializeField] private GameObject casingPrefab;
    [SerializeField] private Transform ejectPoint;
    [SerializeField] private float ejectForce = 3f;
    [SerializeField] private float ejectUpForce = 2f;
    [SerializeField] private Vector2 forceRandomRange = new Vector2(0.8f, 1.2f);
    [SerializeField] private float destroyAfter = 3f;

    public void Eject()
    {
        GameObject casing = Instantiate(casingPrefab, ejectPoint.position, ejectPoint.rotation);
        Rigidbody rb = casing.GetComponent<Rigidbody>();
        if (rb == null) rb = casing.AddComponent<Rigidbody>();

        float rand = Random.Range(forceRandomRange.x, forceRandomRange.y);
        Vector3 force = (ejectPoint.right * ejectForce + ejectPoint.up * ejectUpForce) * rand;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);

        Destroy(casing, destroyAfter);
    }
}