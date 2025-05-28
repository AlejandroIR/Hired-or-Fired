using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ReturnToTable : MonoBehaviour
{
    private float returnDelay = 5f;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private XRGrabInteractable grabInteractable;

    private float timer = 0f;
    private bool isOnTable = true;

    [Header("Special Return for Phone")]
    public bool usePhoneReturnPoint = false;
    public Transform phoneReturnPoint;


    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    public void SetOnTable(bool value)
    {
        isOnTable = value;

        if (isOnTable)
            timer = 0f;
    }

    void Update()
    {
        if (!isOnTable && (grabInteractable == null || !grabInteractable.isSelected))
        {
            timer += Time.deltaTime;
            if (timer >= returnDelay)
            {
                ReturnToStart();
            }
        }
    }

    void ReturnToStart()
    {
        Vector3 targetPosition = initialPosition;
        Quaternion targetRotation = initialRotation;

        if (tag == "Phone" && usePhoneReturnPoint && phoneReturnPoint != null)
        {
            targetPosition = phoneReturnPoint.position;
            targetRotation = phoneReturnPoint.rotation;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.MovePosition(targetPosition);
            rb.MoveRotation(targetRotation);
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }

        timer = 0f;
        isOnTable = true;
    }
}
