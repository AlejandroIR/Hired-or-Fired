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

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
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
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.MovePosition(initialPosition);
            rb.MoveRotation(initialRotation);
        }
        else
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }

        timer = 0f;
        isOnTable = true;
    }
}
