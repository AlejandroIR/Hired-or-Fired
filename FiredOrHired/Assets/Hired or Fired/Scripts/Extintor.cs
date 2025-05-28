using UnityEngine;
using System.Collections;

public class Extintor : MonoBehaviour
{
    public ParticleSystem explosionPS;
    public ParticleSystem polvoPS;
    public ParticleSystem polvofinalPS;

    public AudioClip polvoClip;
    public AudioClip explosionClip;

    AudioSource audioPowder;
    AudioSource audioExplosion;

    void Awake()
    {
        audioPowder = gameObject.AddComponent<AudioSource>();
        audioExplosion = gameObject.AddComponent<AudioSource>();

        audioPowder.playOnAwake = false;
        audioExplosion.playOnAwake = false;
        audioPowder.spatialBlend = 1f;
        audioExplosion.spatialBlend = 1f;
    }

    public void ActivarEfectos()
    {
        explosionPS.Play();

        var main = polvoPS.main;
        main.loop = true;
        polvoPS.Play();

        audioPowder.clip = polvoClip;
        audioPowder.loop = true;
        audioPowder.Play();


        StartCoroutine(FinEfectos());
    }

    IEnumerator FinEfectos()
    { 
        yield return new WaitForSeconds(3.2f);

        audioExplosion.PlayOneShot(explosionClip);
        yield return new WaitForSeconds(0.8f);

        Destroy(gameObject);
    }
}
