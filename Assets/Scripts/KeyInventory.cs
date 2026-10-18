using UnityEngine;

public class KeyInventory : MonoBehaviour
{
    public int keyCount;

    public void AddKey()
    {
        keyCount++;
        Debug.Log($"Keys: {keyCount}");
    }
}