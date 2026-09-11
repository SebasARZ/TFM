using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform reference;
    AudioSource audioSource;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioSource.Play();
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) { cc.enabled = false; }

            other.transform.position = reference.position;

            if (cc != null) { cc.enabled = true; }

        }
    }
}

