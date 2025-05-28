using UnityEngine;

public class NpcWalker : MonoBehaviour
{
    public Transform target;
    public System.Action OnArrived;

    private float speed = 1f;
    private float stoppingDistance = 0.1f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
            animator.SetBool("IsWalking", true); // Activa animación caminar
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0;

        if (direction.magnitude > stoppingDistance)
        {
            transform.position += direction.normalized * speed * Time.deltaTime;
            transform.forward = direction.normalized;
        }
        else
        {
            transform.rotation = target.rotation;

            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetTrigger("SitDown"); // Activar animación de sentarse
            }

            OnArrived?.Invoke();
            Destroy(this); // Ya no se necesita
        }
    }
}
