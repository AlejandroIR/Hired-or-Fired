using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator animator;
    private Rigidbody[] ragdollBodies;

    void Awake()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();

        SetRagdollState(false);
    }

    public void ActivateRagdoll()
    {
        animator.enabled = false;
        SetRagdollState(true);
    }

    void SetRagdollState(bool state)
    {
        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = !state;
            rb.detectCollisions = state;
        }
    }
}
