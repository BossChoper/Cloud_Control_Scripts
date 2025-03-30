using UnityEngine;

public class HookController : MonoBehaviour 
{
    [Header("Hook Settings")]
    public GameObject hookPrefab;
    public float hookSpeed = 20f;
    public float latchDuration = 10f;
    public float pullForce = 10f;
    public float maxDistance = 10f;

    [Header("Visual Tether")]
    public Material tetherMaterial;
    public float tetherWidth = 0.2f;
    public Color tetherColor = Color.yellow;

    [Header("Hook State")]
    private GameObject activeHook;
    private Rigidbody hookedKart;
    private bool isHooked = false;
    private Vector3 offsetToHookedKart;
    private float hookTime;
    private LineRenderer tetherLine;
    private Transform attachPoint;

    void Awake()
    {
        // Create attach point
        GameObject attachObj = new GameObject("TetherAttachPoint");
        attachPoint = attachObj.transform;
        attachPoint.parent = transform;
        attachPoint.localPosition = new Vector3(0, 0.5f, 0);

        // Create the line renderer but keep disabled until hooked
        tetherLine = gameObject.AddComponent<LineRenderer>();
        tetherLine.positionCount = 2;
        tetherLine.startWidth = tetherWidth;
        tetherLine.endWidth = tetherWidth;
        tetherLine.material = tetherMaterial;
        tetherLine.startColor = tetherColor;
        tetherLine.endColor = tetherColor;
        tetherLine.enabled = false;
    }

    void Update()
    {
        // Launch hook with key press
        if (Input.GetKeyDown(KeyCode.Space) && !isHooked && activeHook == null)
        {
            LaunchHook();
        }

        // Handle tether Physics and visuals if hooked
        if (isHooked && hookedKart != null)
        {
            HandleTether();
            UpdateTetherVisuals();

            // Check if hook duration expired
            if (Time.time > hookTime + latchDuration)
            {
                Unhook();
            }
        }
    }

    void LaunchHook()
    {
        // Instantiate hook
        activeHook = Instantiate(hookPrefab, transform.position + transform.forward * 2f, transform.rotation);

        // Add HookProjectile
        HookProjectile hookProjectile = activeHook.AddComponent<HookProjectile>();
        hookProjectile.parentController = this;

        // Set velocity
        Rigidbody hookRb = activeHook.GetComponent<Rigidbody>();
        if (hookRb == null){
            hookRb = activeHook.AddComponent<Rigidbody>();
            hookRb.useGravity = false;
        }
        hookRb.velocity = transform.forward * hookSpeed;

        TrailRenderer trail = activeHook.GetComponent<TrailRenderer>();
        if (trail == null){
            trail = activeHook.AddComponent<TrailRenderer>();
            trail.material = tetherMaterial;
            trail.startWidth = tetherWidth;
            trail.endWidth = tetherWidth;
            trail.time = 0.3f;
            trail.startColor = tetherColor;
            trail.endColor = new Color(tetherColor.r, tetherColor.g, tetherColor.b, 0);
        }
        // Destroy hook
        Destroy(activeHook, 2f);
    }

    public void LatchOnToKart(GameObject targetKart)
    {
        // Prevent multiple hooks
        if (isHooked) return;

        Rigidbody targetRb = targetKart.GetComponent<Rigidbody>();

        // if no rigidbody, return
        if (targetRb == null){
            Debug.LogError("Target kart has no rigidbody");
            return;
        }
        // Assign target kart rigidbody to hook
        hookedKart = targetRb;
        isHooked = true;

        // Record time hook was latched
        hookTime = Time.time;

        // Store relative position between objects
        offsetToHookedKart = targetKart.transform.position - transform.position;

        // Enable tether visuals
        tetherLine.enabled = true;

        // Reset active hook
        activeHook = null;
    }

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

            // Pull the player toward the hooked kart
            myRigidbody.AddForce(pullDirection * pullForce * 0.3f, ForceMode.Force);

            // Pull hooked kart toward player
            hookedKart.AddForce(-pullDirection * pullForce, ForceMode.Force);

        }
    }

    void UpdateTetherVisuals()
    {
        if (tetherLine && hookedKart)
        {
            // Update line renderer positions
            tetherLine.SetPosition(0, attachPoint.position);

            // Find good attach point on target
            Vector3 targetPos = hookedKart.transform.position;
            targetPos.y = attachPoint.position.y;

            tetherLine.SetPosition(1, targetPos);

            // Adjust tether color based on stretch
            float distance = Vector3.Distance(attachPoint.position, targetPos);
            float stretchFactor = Mathf.Clamp01(distance / maxDistance);

            tetherLine.startColor = Color.Lerp(tetherColor, Color.red, stretchFactor);
            tetherLine.endColor = Color.Lerp(tetherColor, Color.blue, stretchFactor);
        }
    }

    void Unhook()
    {
        // if not hooked, return
        if (!isHooked) return;

        isHooked = false;
        hookedKart = null;

        // Disable visual tether
        if (tetherLine){
            tetherLine.enabled = false;
        }
    }
}