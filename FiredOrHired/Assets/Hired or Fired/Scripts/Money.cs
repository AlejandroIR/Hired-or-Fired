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

            transform.position = new Vector3(500, 100, 500);

            // Find the GameManager and trigger the Fire logic
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.Hire();
            }
        }
    }
}
