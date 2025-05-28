using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignSway : MonoBehaviour
{
    public float swayAngle = 10f;        
    public float swaySpeed = 2f;         

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
        transform.localRotation = startRotation * Quaternion.Euler(0f, angle, 0f);
    }
}