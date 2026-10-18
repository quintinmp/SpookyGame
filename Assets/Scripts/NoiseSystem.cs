using System;
using UnityEngine;

public static class NoiseSystem
{
    public static event Action<Vector3, float> OnNoise;

    public static void MakeNoise(Vector3 position, float radius)
    {
        OnNoise?.Invoke(position, radius);
    }
}