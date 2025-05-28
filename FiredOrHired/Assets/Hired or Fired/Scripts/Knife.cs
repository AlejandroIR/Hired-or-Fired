using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Knife : MonoBehaviour
{
    public Rigidbody knifeRigidbody;
    private bool isStuck = false;

    private void Awake()
    {
        knifeRigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Target")) return;

        knifeRigidbody.isKinematic = true;
    }
}
