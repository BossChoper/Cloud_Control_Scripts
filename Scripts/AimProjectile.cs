using UnityEngine;

public class AimProjectile : MonoBehaviour
{
    /*
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 5f;

    private Rigidbody rb;
    private ProjectileController playerControl;
    private GameObject player;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;

        // Register with player control
        player = GameObject.FindGameObjectWithTag("Player");
        playerControl = player.GetComponent<ProjectileController>();
        if (playerControl != null)
        {
            playerControl.RegisterProjectile(gameObject);
        }
        else{
            Debug.LogError("Player or ProjectileController not found!");
        }

        Destroy(gameObject, lifetime); // Auto-destroy after lifetime
    }

    void OnCollisionEnter(Collision collision)
    {
        // Destroy on impact
        if (playerControl != null)
        {
            playerControl.OnProjectileDestroyed();
        }
        else{
            Debug.LogError("ProjectileController not found!");
        }
        Destroy(gameObject);
    }
    */

    public float speed = 20f;
    public float lifetime = 10f;
    public GameObject playerPrefab;
    private Rigidbody rb;
    private ProjectileController playerControl;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
        playerControl = FindObjectOfType<ProjectileController>();
        if (playerControl != null)
        {
            playerControl.RegisterProjectile(gameObject);
        }
        else{
            Debug.LogError("ProjectileController not found!");
        }
        Destroy(gameObject, lifetime);
    }

    void OnDestroy()
    {
        if(playerControl != null)
        {
            playerControl.OnProjectileDestroyed();
        }
    }
}
    /* 
    using UnityEngine;

public class AimProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 10f;
    
    private Rigidbody rb;
    private ProjectileController playerControl;
    private GameObject player;

    void Start()
    {
        // Get and set rigidbody velocity
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.velocity = transform.forward * speed;

        // Register with player control
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerControl = player.GetComponent<ProjectileController>();
            if (playerControl != null)
            {
                playerControl.RegisterProjectile(gameObject);
            }
            else
            {
                Debug.LogError("ProjectileController not found on player!");
            }
        }
        else
        {
            Debug.LogError("Player not found!");
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Notify controller and destroy on impact
        if (playerControl != null)
        {
            playerControl.OnProjectileDestroyed();
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Ensure controller is notified when destroyed for any reason
        if (playerControl != null && gameObject != null)
        {
            playerControl.OnProjectileDestroyed();
        }
    }
}

}
*/