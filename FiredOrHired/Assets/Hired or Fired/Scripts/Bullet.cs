using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [Header("Estadisticaa")]
    public float speed = 15f;
    public float lifetime = 2f;
    public ParticleSystem impactEffect;

    private RevolverVR owner;

    public GameObject impactBloodEffect;
    public GameObject impactGlassEffect;

    public void Init(RevolverVR owner)
    {
        this.owner = owner;
    }

    private void Start()
    {
        StartCoroutine(EnableCollider());

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Invoke(nameof(DestroySelf), lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Revolver")) return;

        if (collision.gameObject.CompareTag("Extintor"))
        {
            var ext = collision.gameObject.GetComponent<Extintor>();
            if (ext != null) ext.ActivarEfectos();
            return;

        }

        // Check if the collision is with an NPC
        if (collision.gameObject.CompareTag("NPC"))
        {
            Debug.Log("Hit NPC: " + collision.gameObject.name);
            // Find the GameManager and trigger the Fire logic
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.FireByBullet(collision.gameObject);
            }
        }

        if (impactEffect != null)
        {
            ParticleSystem effect = Instantiate(impactEffect, collision.contacts[0].point, Quaternion.identity);

            effect.transform.forward = collision.contacts[0].normal;
            effect.Play();

            float destroyTime = effect.main.duration + effect.main.startLifetime.constant;
            Destroy(effect.gameObject, destroyTime);
        }

        if (collision.gameObject.CompareTag("Glass"))
        {
            Debug.Log("Hit Glass: " + collision.gameObject.name);
            Vector3 bulletPosition = transform.position;
            Instantiate(impactGlassEffect, bulletPosition, Quaternion.identity);
            Destroy(collision.gameObject);
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Bullet] Trigger con '{other.gameObject.name}' (Tag: {other.gameObject.tag})");

        if (other.CompareTag("Revolver"))
            return;

        if (other.CompareTag("Extintor"))
        {
            var ext = other.GetComponent<Extintor>();
            //Rigidbody rb = other.attachedRigidbody;
            //if (rb != null)
            //{
            //    rb.isKinematic = false;
            //    rb.useGravity = true;
            //}
            if (ext != null) ext.ActivarEfectos();
            return;
        }

        if (other.CompareTag("NPC"))
        {
            Debug.Log("Hit NPC: " + other.gameObject.name);
            // Find the GameManager and trigger the Fire logic
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.FireByBullet(other.gameObject);
            }
            if (impactBloodEffect != null)
            {
                Vector3 bulletPosition = transform.position;
                Instantiate(impactBloodEffect, bulletPosition, Quaternion.identity);
            }
        }

        if (other.CompareTag("Glass"))
        {
            Debug.Log("Hit Glass: " + other.gameObject.name);
            Destroy(other.gameObject);
            Vector3 bulletPosition = transform.position;
            Instantiate(impactGlassEffect, bulletPosition, Quaternion.identity);
        }

        DestroySelf();
    }

}
