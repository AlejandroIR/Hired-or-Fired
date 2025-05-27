using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Knife : MonoBehaviour
{
    public Collider bladeCollider;
    public Rigidbody knifeRigidbody;
    private bool isStuck = false;

    private void Awake()
    {
        knifeRigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isStuck) return;

        ContactPoint contact = collision.GetContact(0);

        if (contact.thisCollider != bladeCollider) return;
        if (!collision.gameObject.CompareTag("Target")) return;

        isStuck = true;
        knifeRigidbody.isKinematic = true;
        knifeRigidbody.velocity = Vector3.zero;
        knifeRigidbody.angularVelocity = Vector3.zero;

        Quaternion currentRot = transform.rotation;
        Vector3 localOffset = transform.InverseTransformPoint(contact.point);
        transform.rotation = currentRot;
        transform.position = contact.point - (transform.rotation * localOffset);
        transform.SetParent(collision.transform);
    }
}
