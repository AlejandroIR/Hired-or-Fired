using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [Header("Estadisticaa")]
    public float speed = 15f;
    public float lifetime = 2f;
    public ParticleSystem impactEffect;

    private RevolverVR owner;

    public void Init(RevolverVR owner)
    {
        this.owner = owner;
    }

    private void Start()
    {
        StartCoroutine(EnableCollider());

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = transform.forward * speed;

        Invoke(nameof(DestroySelf), lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Revolver")) return;

        if (impactEffect != null)
        {
            ParticleSystem effect = Instantiate(impactEffect, collision.contacts[0].point, Quaternion.identity);

            effect.transform.forward = collision.contacts[0].normal;
            effect.Play();

            float destroyTime = effect.main.duration + effect.main.startLifetime.constant;
            Destroy(effect.gameObject, destroyTime);
        }

        DestroySelf();
    }

    private IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(0.1f);
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

}
