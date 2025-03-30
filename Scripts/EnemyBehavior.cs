using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Rigidbody rb;
    private Vector3 originalVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
    }

    public void Freeze()
    {
        if (rb != null)
        {
            originalVelocity = rb.velocity;
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void Unfreeze()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = originalVelocity;
        }
    }
}