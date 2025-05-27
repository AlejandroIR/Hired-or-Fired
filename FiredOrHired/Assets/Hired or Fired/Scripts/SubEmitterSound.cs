using UnityEngine;

public class SubEmitterSound : MonoBehaviour
{
    public AudioClip deathSound;
    public AudioSource audioSource;
    
    void Start()
    {
        // Reproducir sonido cuando se crea el sub-emisor
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
}