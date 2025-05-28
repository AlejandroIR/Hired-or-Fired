using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Debug.Log("Hit NPC: " + other.gameObject.name);

            Vector3 targetPosition = new Vector3(500, 100, 500);

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.MovePosition(targetPosition);
            }

            // Find the GameManager and trigger the Fire logic
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.Hire();
            }
        }
    }
}
