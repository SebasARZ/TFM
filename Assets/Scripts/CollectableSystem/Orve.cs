using System;
using System.Collections;
using UnityEngine;

public class Orve : MonoBehaviour, ICollectable
{
    private AudioSource _audioSource;
    private ParticleSystem _particles;
    public static event Action OnOrveCollected;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _particles = GetComponentInChildren<ParticleSystem>();
    }
    IEnumerator PlaySound()
    {
        _audioSource.Play();
        _particles.Stop();
        yield return new WaitForSeconds(_audioSource.clip.length);
        Destroy(gameObject);
    }
    public void Collect()
    {
        StartCoroutine(PlaySound());
        OnOrveCollected?.Invoke();
                }
}
