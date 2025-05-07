using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Estadisticaa")]
    public float speed = 50f;
    public float lifetime = 5f;

    private RevolverVR owner;

    public void Init(RevolverVR owner)
    {
        this.owner = owner;
    }

    private void Start()
    {
        owner?.ResetFire();
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = transform.forward * speed;

        Invoke(nameof(DestroySelf), lifetime);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}
