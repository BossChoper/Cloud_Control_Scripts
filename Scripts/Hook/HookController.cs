using UnityEngine;

public class HookController : MonoBehaviour
{
    [Header("Hook Settings")]
    public GameObject hookPrefab; // Assign your hook prefab in the Inspector
    public float hookSpeed = 20f;
    public float latchDuration = 10f;
    public float pullForce = 10f; // Force to pull karts together
    public float maxDistance = 10f; // Maximum tether distance
    
    [Header("Visual Tether")]
    public Material tetherMaterial; // Assign in inspector
    public float tetherWidth = 0.2f;
    public Color tetherColor = Color.yellow;
    
    [Header("Hook State")]
    private GameObject activeHook;
    private Rigidbody hookedKart;
    private bool isHooked = false;
    private Vector3 offsetToHookedKart;
    private float hookTime;
    private LineRenderer tetherLine;
    private Transform attachPoint; // Point on this kart where the tether connects


    void Awake()
    {
        // Create attach point (slightly elevated from kart center)
        GameObject attachObj = new GameObject("TetherAttachPoint");
        attachPoint = attachObj.transform;
        attachPoint.parent = transform;
        attachPoint.localPosition = new Vector3(0, 0.5f, 0); // Adjust based on your kart model
        
        // Create the line renderer but keep it disabled until hooked
        tetherLine = gameObject.AddComponent<LineRenderer>();
        tetherLine.positionCount = 2;
        tetherLine.startWidth = tetherWidth;
        tetherLine.endWidth = tetherWidth;
        tetherLine.material = tetherMaterial;
        tetherLine.startColor = tetherColor;
        tetherLine.endColor = tetherColor;
        tetherLine.enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        // Helper launches hook with a key press (e.g., Spacebar)
        if (Input.GetKeyDown(KeyCode.Space) && !isHooked && activeHook == null)
        {
            LaunchHook();
        }
        
        // Handle tether physics and visuals if hooked
        if (isHooked && hookedKart != null)
        {
            HandleTether();
            UpdateTetherVisuals();
            
            // Check if hook duration has expired
            if (Time.time > hookTime + latchDuration)
            {
                Unhook();
            }
        }
    }

    // Helper function to launch the hook
    void LaunchHook()
    {
        // Instantiate hook in front of kart
        activeHook = Instantiate(hookPrefab, transform.position + transform.forward * 2f, transform.rotation);
        
        // Add HookProjectile component to the hook
        HookProjectile hookProjectile = activeHook.AddComponent<HookProjectile>();
        hookProjectile.parentController = this;
        
        // Set velocity
        Rigidbody hookRb = activeHook.GetComponent<Rigidbody>();
        if (hookRb == null) {
            hookRb = activeHook.AddComponent<Rigidbody>();
            hookRb.useGravity = false;
        }
        hookRb.velocity = transform.forward * hookSpeed;
        
        // Trail effect for hook (optional)
        TrailRenderer trail = activeHook.GetComponent<TrailRenderer>();
        if (trail == null) {
            trail = activeHook.AddComponent<TrailRenderer>();
            trail.material = tetherMaterial;
            trail.startWidth = tetherWidth;
            trail.endWidth = 0.05f;
            trail.time = 0.3f;
            trail.startColor = tetherColor;
            trail.endColor = new Color(tetherColor.r, tetherColor.g, tetherColor.b, 0);
        }
        
        // Destroy hook if it doesn't hit anything after 2 seconds
        Destroy(activeHook, 2f);
    }

    // Call this when the hook collides with another kart
    public void LatchOntoKart(GameObject targetKart)
    {
        // Prevent multiple hooks
        if (isHooked) return;
        
        // Get the Rigidbody component of the target kart
        Rigidbody targetRb = targetKart.GetComponent<Rigidbody>();
        
        // If the target kart has no Rigidbody, return
        if (targetRb == null) {
            Debug.LogError("Target kart has no Rigidbody component!");
            return;
        }
        // Assign the target kart's Rigidbody to hookedKart
        hookedKart = targetRb;
        isHooked = true;
        
        // Record the time when the hook was latched
        hookTime = Time.time;
        
        // Store the relative position between karts at hook time
        offsetToHookedKart = targetKart.transform.position - transform.position;
        
        // Enable visual tether
        tetherLine.enabled = true;
        
        // Reset activeHook reference
        activeHook = null;
    }

    // Handle tether physics and visuals
    void HandleTether()
    {
        // Calculate current distance between karts
        Vector3 currentOffset = hookedKart.transform.position - transform.position;
        float currentDistance = currentOffset.magnitude;
        
        // Apply forces only if we exceed the maximum distance
        if (currentDistance > maxDistance)
        {
            Vector3 pullDirection = currentOffset.normalized;
            
            // Apply opposing forces to both karts
            Rigidbody myRigidbody = GetComponent<Rigidbody>();
            
            // Pull the player toward the hooked kart (with less force)
            myRigidbody.AddForce(pullDirection * pullForce * 0.3f, ForceMode.Force);
            
            // Pull the hooked kart toward the player (with more force)
            hookedKart.AddForce(-pullDirection * pullForce, ForceMode.Force);
        }
    }
    
    void UpdateTetherVisuals()
    {
        if (tetherLine && hookedKart)
        {
            // Update line renderer positions
            tetherLine.SetPosition(0, attachPoint.position);
            
            // Find a good attach point on the target kart (approximately at the same height)
            Vector3 targetPos = hookedKart.transform.position;
            targetPos.y = attachPoint.position.y; // Match height for a cleaner look
            
            tetherLine.SetPosition(1, targetPos);
            
            // Optional: Adjust tether color based on stretch
            float distance = Vector3.Distance(attachPoint.position, targetPos);
            float stretchFactor = Mathf.Clamp01(distance / maxDistance);
            // Adjust color based on stretch
            tetherLine.startColor = Color.Lerp(tetherColor, Color.red, stretchFactor);
            tetherLine.endColor = Color.Lerp(tetherColor, Color.red, stretchFactor);
        }
    }

    // Release the hooked kart
    void Unhook()
    {
        // If not hooked, return
        if (!isHooked) return;
        
        isHooked = false;
        hookedKart = null;
        
        // Disable visual tether
        if (tetherLine) {
            tetherLine.enabled = false;
        }
    }
}