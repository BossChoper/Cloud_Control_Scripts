using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject regularProjectilePrefab;
    [SerializeField] private GameObject cloneProjectilePrefab;
    [SerializeField] private GameObject aimProjectilePrefab;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private KeyCode controlKey = KeyCode.P; // Changed to E for example, adjust as needed
    [SerializeField] private Camera playerCamera;           // Reference to player's main camera
    private float nextFireTime = 0f;
    private bool useCloneProjectile = false;
    private bool useAimProjectile = false;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        float deltaTime = Time.timeScale > 0f ? Time.deltaTime : Time.unscaledDeltaTime;
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if(useCloneProjectile)
            {
                Debug.Log("Shot in Clone mode");
                Shoot();
            }
            else if (useAimProjectile)
            {
                
                Shoot();
                Debug.Log("Shot in Aim mode");
            }
            else
            {
                Debug.Log("Shot in Regular mode");
                Shoot();
            }
            nextFireTime = Time.time + (1f / fireRate) * (Time.timeScale > 0f ? 1f : deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
  
            useCloneProjectile = !useCloneProjectile;
            useAimProjectile = false; // Ensure only one mode is active
            Debug.Log("Switched to " + (useCloneProjectile ? "Clone" : "Regular") + " projectile");
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
        
            useAimProjectile = !useAimProjectile;
            useCloneProjectile = false; // Ensure only one mode is active
            Debug.Log("Switched to " + (useAimProjectile ? "Aim" : "Regular") + " projectile");
        }
    }

    void Shoot()
    {
        GameObject projectileToSpawn = regularProjectilePrefab;
        if (useCloneProjectile)
        {
            projectileToSpawn = cloneProjectilePrefab;
        }
        else if (useAimProjectile)
        {
            projectileToSpawn = aimProjectilePrefab;
        }
        Instantiate(projectileToSpawn, firePoint.position, firePoint.rotation);
    }
}