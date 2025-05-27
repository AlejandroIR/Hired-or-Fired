using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToTable : MonoBehaviour
{
    private float returnDelay = 5f;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

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
        if (!isOnTable)
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
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        timer = 0f;
        isOnTable = true;
    }
}
