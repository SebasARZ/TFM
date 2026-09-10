using System;
using UnityEngine;

public class Orve : MonoBehaviour, ICollectable
{
    public static event Action OnOrveCollected;
    public void Collect()
    {
        OnOrveCollected?.Invoke();
        Destroy(gameObject);
    }
}
