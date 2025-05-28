using UnityEngine;
using UnityEngine.AI;

public class NpcWalker : MonoBehaviour
{
    public Transform target;
    public float speed = 1.5f;
    public float stopDistance = 0.1f;

    public System.Action OnArrived;

    private bool arrived = false;

    void Update()
    {
        if (arrived || target == null) return;

        // Mueve hacia el objetivo
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rota suavemente hacia el objetivo
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Verifica llegada
        if (Vector3.Distance(transform.position, target.position) <= stopDistance)
        {
            arrived = true;
            transform.position = target.position;
            transform.rotation = target.rotation;

            OnArrived?.Invoke();
        }
    }
}
