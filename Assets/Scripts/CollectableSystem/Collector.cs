using UnityEngine;

public class Collector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ICollectable collectible = other.GetComponent<ICollectable>();
        if (collectible != null)
        {
            collectible.Collect();
        }
    }
}
