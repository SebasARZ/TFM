using System;
using UnityEngine;

public class OrveCounter : MonoBehaviour
{
    public static int Total { get; private set; }
    public static event Action<int> OnCountChanged;

    private void Awake()
    {
        Total = 0; // Reset the count when the game starts
    }

    private void OnEnable()
    {
        Orve.OnOrveCollected += Increment;
    }

    private void OnDisable()
    {
        Orve.OnOrveCollected -= Increment;
    }

    private void Increment()
    {
        Total++;
        OnCountChanged?.Invoke(Total);
    }
}