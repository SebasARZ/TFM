using UnityEngine;

public class LightDisable : MonoBehaviour
{
    public Light lightComponent;
    public bool isLightOn = true;
    float originalIntensity;
    void Start()
    {
        originalIntensity = lightComponent.intensity;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(isLightOn)
            {
                lightComponent.intensity = 10;
            }
            else
            {
                lightComponent.intensity = originalIntensity;
            }
        }
    }
    void Update()
    {
        
    }
}
