using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ThrowGlass : MonoBehaviour
{
    public GameObject impactGlassEffect;
    public float impactThreshold = 3f;
    
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private Vector3 previousVelocity;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on " + gameObject.name);
        }
    }

    void Start()
    {
        previousVelocity = rb.velocity;
    }

    // Update is called once per frame
    void Update()
    {
        CheckVelocity();
        previousVelocity = rb.velocity;
    }

    public void CheckVelocity()
    {  
        if (rb == null) return;
        
        bool isBeingGrabbed = grabInteractable != null && grabInteractable.isSelected;
        
        if (isBeingGrabbed) return;
        
        float velocityDifference = (previousVelocity - rb.velocity).magnitude;
        
        if (velocityDifference > impactThreshold)
        {
            DestroySelf();
        }
    }

    public void DestroySelf()
    {
        Instantiate(impactGlassEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
