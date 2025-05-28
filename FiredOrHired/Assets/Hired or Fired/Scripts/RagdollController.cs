using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator animator;

    private Rigidbody[] ragdollBodies;
    private Collider[] ragdollColliders;

    [Header("Head Only")]
    public Rigidbody headRigidbody;
    public Collider headCollider;

    void Awake()
    {
        ragdollBodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        SetFullRagdollState(false);   // Desactiva todo el cuerpo
        ActivateHeadOnly();           // Solo deja activa la cabeza
    }

    public void ActivateRagdoll()
    {
        if (animator != null)
            animator.enabled = false;

        SetFullRagdollState(true);
    }

    void SetFullRagdollState(bool state)
    {
        foreach (var rb in ragdollBodies)
        {
            rb.isKinematic = !state;
        }

        foreach (var col in ragdollColliders)
        {
            col.enabled = state;
        }
    }

    void ActivateHeadOnly()
    {
        if (headRigidbody != null)
            headRigidbody.isKinematic = false;

        if (headCollider != null)
            headCollider.enabled = true;
    }
}
