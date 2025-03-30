using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float controlRange = 10f;       // Max distance to control projectile
    [SerializeField] private float steerSpeed = 5f;         // Speed of projectile steering
    [SerializeField] private KeyCode controlKey = KeyCode.E; // Changed to E for example, adjust as needed
    [SerializeField] private Camera playerCamera;           // Reference to player's main camera

    private GameObject controlledProjectile;
    private Rigidbody projectileRb;
    private Camera projectileCam;
    private bool isControlling = false;
    private float lastShotTime;
    private float controlWindow = 2f;

    void Update()
    {
        if (Input.GetKeyDown(controlKey) && Time.time <= lastShotTime + controlWindow && !isControlling)
        {
            TryTakeControl();
        }

        if (isControlling && controlledProjectile != null)
        {
            SteerProjectile();
        }
    }

    public void RegisterProjectile(GameObject projectile)
    {
        controlledProjectile = projectile;
        projectileRb = projectile.GetComponent<Rigidbody>();
        lastShotTime = Time.time;
    }

    void TryTakeControl()
    {
        if (controlledProjectile != null && Vector3.Distance(transform.position, controlledProjectile.transform.position) <= controlRange)
        {
            StartControlling();
        }
    }

    void StartControlling()
    {
        isControlling = true;
        playerCamera.enabled = false;

        Camera projectileCam = controlledProjectile.GetComponent<Camera>();
        if (projectileCam == null)
        {
            projectileCam = controlledProjectile.AddComponent<Camera>();
        }
        projectileCam.enabled = true;
        projectileCam.fieldOfView = 60f;
    }

    void SteerProjectile()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");
        Vector3 steerDirection = new Vector3(hInput, vInput, 0).normalized;
        Vector3 newDirection = Vector3.Lerp(projectileRb.velocity.normalized, steerDirection, Time.deltaTime * steerSpeed);
        float currentSpeed = projectileRb.velocity.magnitude;
        projectileRb.velocity = newDirection * currentSpeed;

        if (newDirection != Vector3.zero)
        {
            controlledProjectile.transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    public void OnProjectileDestroyed()
    {
        if (isControlling)
        {
            StopControlling();
        }
        controlledProjectile = null;
        projectileRb = null;
        projectileCam = null;
    }

    void StopControlling()
    {
        isControlling = false;

        if (playerCamera != null)
        {
            playerCamera.enabled = true;
        }

        if (projectileCam != null && controlledProjectile != null)
        {
            projectileCam.enabled = false;
            projectileCam = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, controlRange);
    }
}