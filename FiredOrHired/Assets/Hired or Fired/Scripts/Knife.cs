using UnityEngine;
using System.Collections;

public class Knife : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float stickTime = 2f;
    [SerializeField] private Rigidbody knifeRigidbody;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isStuck = false;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isStuck) return;

        if (collision.gameObject.CompareTag("Target"))
        {
            StickKnife(collision);
            StartCoroutine(UnstickAfterDelay());
        }
        else if (collision.gameObject.CompareTag("Mango"))
        {
            ResetKnifePosition();
        }
    }

    private void StickKnife(Collision collision)
    {
        isStuck = true;

        knifeRigidbody.isKinematic = true;
        transform.SetParent(collision.transform);
    }

    private IEnumerator UnstickAfterDelay()
    {
        yield return new WaitForSeconds(stickTime);

        knifeRigidbody.isKinematic = false;
        transform.SetParent(null);
        ResetKnifePosition();
        isStuck = false;
    }

    private void ResetKnifePosition()
    {
        StopAllCoroutines();
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (knifeRigidbody != null)
        {
            knifeRigidbody.velocity = Vector3.zero;
            knifeRigidbody.angularVelocity = Vector3.zero;
            knifeRigidbody.isKinematic = false;
        }

        isStuck = false;
    }
}