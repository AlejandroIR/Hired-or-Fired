using UnityEngine;
using System.Collections;

public class Extintor : MonoBehaviour
{
    public ParticleSystem explosionPS;
    public ParticleSystem polvoPS;

    public AudioClip polvoClip;

    AudioSource audioPowder;
    AudioSource audioExplosion;

    void Awake()
    {
        audioPowder = gameObject.AddComponent<AudioSource>();
        audioExplosion = gameObject.AddComponent<AudioSource>();

        audioPowder.playOnAwake = false;
        audioExplosion.playOnAwake = false;
        audioPowder.spatialBlend = 1f;
        audioPowder.volume = 0.3f;
        audioExplosion.spatialBlend = 1f;
    }

    public void ActivarEfectos()
    {
        explosionPS.Play();

        Collider[] cols = GetComponents<Collider>();
        foreach (var col in cols)
        {
            col.enabled = false;
        }

        var main = polvoPS.main;
        main.loop = true;
        polvoPS.Play();

        audioPowder.clip = polvoClip;
        audioPowder.loop = true;
        audioPowder.Play();
    }
}
