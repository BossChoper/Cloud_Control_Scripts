using UnityEngine;

public class CloudBehavior : MonoBehaviour
{
    // Cloud prefabs and variables
    public GameObject target;
    public Vector3 offset = new Vector3(0, 3f, 0);
    public Material inkMaterial;
    public GameObject bigCloudPrefab;
    public GameObject stormCloudPrefab;
    public GameObject superpowerPickupPrefab; // Ensure this is assigned in Inspector
    public float rainInterval = 1f;
    public float hoverForce = 9.8f;
    public float combineRadius = 2f;
    public float followSpeed = 10f;
    public float verticalFollowSpeed = 2f;
    public float burstRainInterval = 0.2f;
    public float proximityDistance = 5f;

    // Cloud behavior variables
    private float lingerTime = 3f;
    private float lingerTimer = 0f;
    private float rainTimer = 0f;
    private float burstDuration = 0f;
    private bool enemyDead = false;
    private bool hasCombined = false;
    private bool launched = false;
    private bool isBursting = false;
    private bool isPreparingLaunch = false;
    private Renderer cloudRenderer;
    private Color originalColor;
    private GameObject player;
    private Rigidbody rb;
    private float maxLaunchHeight = 5f;
    private float launchDistanceLimit = 10f;
    private Vector3 launchStartPosition;
    private float launchTargetLockoutDuration = 1f;
    private float timeSinceLaunch = 0f;
    private bool canReTarget = false;

    private bool isActivated = false;
    private float activationTimer = 0f;
    private Color activatedColor = Color.yellow;

    void Start()
    {
        // Initialize cloud renderer
        cloudRenderer = GetComponent<Renderer>();
        // Initialize original color
        originalColor = cloudRenderer.material.color;
        // Initialize player
        player = GameObject.FindWithTag("Player");
        // Initialize rain timer
        rainTimer = rainInterval;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (gameObject.name.Contains("BigCloud"))
        {
            followSpeed = 3f;
            verticalFollowSpeed = 1f;
        }
        Debug.Log($"Cloud Start - Target: {(target != null ? target.name : "None")}, Proximity Distance: {proximityDistance}");
    }

    void Update()
    {
        Debug.Log($"Cloud Update - Target: {(target != null ? target.name : "None")}, Launched: {launched}, EnemyDead: {enemyDead}");

        if (isActivated)
        {
            activationTimer -= Time.deltaTime;
            if (activationTimer <= 0)
            {
                DeactivateCloud();
            }
            else
            {
                DropInk();
            }
        }

        // Handle launched state
        if (launched)
        {
            HandleLaunchedState();
            return;
        }

        // Update rain timer
        UpdateRainTimer();

        // Handle target stat   e
        if (target != null && !isPreparingLaunch)
        {
            // Follow target
            FollowTarget();

            // Check if target is an enemy
            if (target != null && target.CompareTag("Enemy"))
            {
                Debug.Log($"Target {target.name} is an enemy, checking proximity...");
                // Check enemy proximity
                CheckEnemyProximity();
            }

            if (target == player && !hasCombined)
            {
                CombineWithNearbyCloud();
            }
            if (target == player)
            {
                lingerTimer = lingerTime;
            }
        }
        else
        {
            HandleTargetlessState();
        }
    }

    // Handle launched state
    private void HandleLaunchedState()
    {
        // Increment time since launch
        timeSinceLaunch += Time.deltaTime;

        // Check if launch lockout duration has passed
        if (timeSinceLaunch >= launchTargetLockoutDuration)
        {
            canReTarget = true;
        }

        // Apply hover force
        ApplyHoverForce(0.5f);
        // Constrain launch trajectory
        ConstrainLaunchTrajectory();
        // Update rain timer
        UpdateRainTimer();

        // Check if can re-target
        if (canReTarget)
        {
            // Check for target below
            CheckForTargetBelow();
            if (target != null)
            {
                launched = false;
                rb.isKinematic = true;
                rb.useGravity = false;
                lingerTimer = lingerTime;
                cloudRenderer.material.color = originalColor;
                timeSinceLaunch = 0f;
                canReTarget = false;
                Debug.Log($"Cloud found new target during launch: {target.name}, resuming follow behavior!");
                return;
            }
        }

        lingerTimer -= Time.deltaTime;
        if (lingerTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Apply hover force
    private void ApplyHoverForce(float multiplier = 1f)
    {
        // Check if rb is null
        if (rb != null)
        {       
            // Apply hover force
            rb.AddForce(Vector3.up * hoverForce * multiplier, ForceMode.Acceleration);
            // Get current velocity
            Vector3 velocity = rb.velocity;
            // Apply damping to vertical velocity
            velocity.y *= 0.98f;
            // Update velocity
            rb.velocity = velocity;
        }
    }

    // Constrain launch trajectory
    private void ConstrainLaunchTrajectory()
    {
        // Check if rb is null
        if (rb == null) return;

        // Constrain vertical position
        if (transform.position.y > launchStartPosition.y + maxLaunchHeight)
        {
            Vector3 velocity = rb.velocity;
            velocity.y = Mathf.Min(velocity.y, 0);
            rb.velocity = velocity;
            transform.position = new Vector3(transform.position.x, launchStartPosition.y + maxLaunchHeight, transform.position.z);
        }
        // Constrain horizontal position
        float distanceTraveled = Vector3.Distance(launchStartPosition, transform.position);
        if (timeSinceLaunch > 1f || distanceTraveled > launchDistanceLimit)
        {
            Vector3 velocity = rb.velocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            horizontalVelocity *= 0.7f;
            rb.velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        }
    }

    // Update rain timer
    private void UpdateRainTimer()
    {
        // Update rain timer
        rainTimer -= Time.deltaTime;
        // Check if rain should be triggered
        if (rainTimer <= 0 && !isActivated)
        {   
            // Drop ink
            DropInk();
            // Reset rain timer
            rainTimer = isBursting ? burstRainInterval : rainInterval;
            // Log rain triggered
            Debug.Log($"Rain triggered (burst: {isBursting})");
        }
    }

    // Follow target
    private void FollowTarget()
    {
        // Get target position
        Vector3 targetPos = target.transform.position;
        // Calculate desired position with offset and sinusoidal movement
        Vector3 desiredPos = targetPos + offset + new Vector3(0, Mathf.Sin(Time.time * 5f) * 0.2f, 0);
        // Get current position 
        Vector3 currentPos = transform.position;
        // Calculate horizontal and vertical lerp factors
        float horizontalLerpFactor = followSpeed * Time.deltaTime;
        float verticalLerpFactor = verticalFollowSpeed * Time.deltaTime;
        // Interpolate position
        float newX = Mathf.Lerp(currentPos.x, desiredPos.x, horizontalLerpFactor);
        float newZ = Mathf.Lerp(currentPos.z, desiredPos.z, horizontalLerpFactor);
        float newY = Mathf.Lerp(currentPos.y, desiredPos.y, verticalLerpFactor);
        // Set new position
        transform.position = new Vector3(newX, newY, newZ);
    }

    // Check enemy proximity for cloud transformation
    private void CheckEnemyProximity()
    {
        // Check if target is null
        if (target == null)
        {
            Debug.Log("Target is null, cannot check proximity.");
            return;
        }

        // Calculate distance to target
        float distance = Vector3.Distance(transform.position, target.transform.position);
        // Log distance to target
        Debug.Log($"Distance to enemy {target.name}: {distance}, Proximity Distance: {proximityDistance}");
        // Check if within proximity distance
        if (distance <= proximityDistance)
        {
            // Log enemy proximity and trigger transformation
            Debug.Log($"Enemy {target.name} is within proximity range ({distance} <= {proximityDistance})! Triggering transformation.");
            TransformCloudOnEnemyProximity();
        }
    }

    // Transform cloud on enemy proximity
    private void TransformCloudOnEnemyProximity()
    {
        // Check if storm cloud prefab is assigned
        if (stormCloudPrefab == null)
        {
            Debug.LogWarning("StormCloudPrefab not assigned! Cannot transform cloud.");
            return;
        }

        // Destroy enemy
        GameObject enemyToDestroy = target;
        target = null;
        // Set enemy dead
        if (enemyToDestroy != null)
        {
            Destroy(enemyToDestroy);
            Debug.Log($"Enemy {enemyToDestroy.name} destroyed!");
        }

        // Spawn storm cloud
        Vector3 cloudPosition = transform.position;
        Quaternion cloudRotation = transform.rotation;
        GameObject stormCloud = Instantiate(stormCloudPrefab, cloudPosition, cloudRotation);
        // Get storm cloud script
        CloudBehavior stormCloudScript = stormCloud.GetComponent<CloudBehavior>();
        if (stormCloudScript != null)
        {
            // Set target to null
            stormCloudScript.target = null;
            // Set rain interval
            stormCloudScript.rainInterval = 0.3f;
            // Log storm cloud spawning and transformation
            Debug.Log("Storm Cloud spawned at: " + cloudPosition);
        }

        // Spawn superpower pickup with random offset near ground
        if (superpowerPickupPrefab != null)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f; // Random offset within 2-unit radius
            Vector3 pickupPosition = new Vector3(
                cloudPosition.x + randomOffset.x,
                cloudPosition.y + 1f, // Start 1 unit above ground for falling
                cloudPosition.z + randomOffset.y
            );
            GameObject pickup = Instantiate(superpowerPickupPrefab, pickupPosition, Quaternion.identity);
            Debug.Log($"Superpower pickup spawned at: {pickupPosition}");
        }
        else
        {
            Debug.LogWarning("SuperpowerPickupPrefab not assigned! No pickup spawned.");
        }

        // Assign storm cloud to player
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        // Check if player combat component is found
        if (playerCombat != null && playerCombat.currentCloud == gameObject)
        {
            // Set current cloud to storm cloud
            playerCombat.currentCloud = stormCloud;
        }
        // Destroy original cloud
        Destroy(gameObject);
    }

    // Handle targetless state
    private void HandleTargetlessState()
    {
        // Check if enemy is dead
        if (!enemyDead)
        {
            // Set enemy dead
            enemyDead = true;
            // Set linger timer
            lingerTimer = lingerTime;
        }
        else
        {
            // If rb is null or not kinematic, set it to kinematic and disable gravity
            if (rb != null && !rb.isKinematic)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Check for target below
            CheckForTargetBelow();
            // Update linger timer
            UpdateLingerTimer();
        }
    }

    // Check for target below
    private void CheckForTargetBelow()
    {
        // Check if raycast hits a collider
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3.5f))
        {
            // Log raycast hit
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
            // Get hit object
            GameObject hitObject = hit.collider.gameObject;

            // Check if hit object is player
            if (hitObject == player)
            {
                // Set target to player
                target = player;
                // Set linger timer
                lingerTimer = lingerTime;
                // Set cloud color to original
                cloudRenderer.material.color = originalColor;
                // Assign cloud to player and log it
                Debug.Log("Cloud now following Player!");
                AssignCloudToPlayer();
            }
            else if (hitObject.CompareTag("Enemy"))
            {
                // Set target to enemy
                target = hitObject;
                // Set linger timer
                lingerTimer = lingerTime;
                // Set cloud color to original
                cloudRenderer.material.color = originalColor;
                // Assign cloud to enemy and log it
                Debug.Log($"Cloud now following Enemy: {hitObject.name}!");
            }
        }
    }

    // Assign cloud to player
    private void AssignCloudToPlayer()
    {
        // Get player combat component
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        // Check if player combat component is found
        if (playerCombat != null)
        {
            // Set current cloud to this cloud
            Debug.Log("Setting currentCloud to this Cloud!");
            playerCombat.currentCloud = gameObject;
        }
        else
        {
            // Log error if player combat component is not found
            Debug.LogWarning("PlayerCombat component not found on Player!");
        }
    }

    // Update linger timer
    private void UpdateLingerTimer()
    {
        // Decrease linger timer
        lingerTimer -= Time.deltaTime;
        // Calculate alpha for fade
        float alpha = lingerTimer / lingerTime;
        // Update cloud color
        cloudRenderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        // Destroy cloud when linger timer expires
        if (lingerTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Prepare cloud for launch
    public void PrepareForLaunch()
    {
        // Set cloud to preparing launch
        isPreparingLaunch = true;
        // Reset target
        target = null;
        // Log preparation
        Debug.Log("Cloud preparing for launch, stopping follow behavior!");
    }

    // Launch cloud
    public void Launch()
    {
        // Set cloud to launched
        launched = true;
        // Reset target
        target = null;
        // Set linger timer till self destruction
        lingerTimer = lingerTime;
        // Set launch start position
        launchStartPosition = transform.position;
        // Reset time since launch
        timeSinceLaunch = 0f;
        // Reset re-target
        canReTarget = false;
        isPreparingLaunch = false;
        // Log launch
        Debug.Log("Cloud marked as launched!");
    }

    // Trigger burst rain
    public void TriggerBurstRain()
    {
        // Set cloud to bursting
        isBursting = true;
        burstDuration = 2f;
        rainTimer = 0f;
        // Log burst rain
        Debug.Log("Burst rain triggered!");
    }

    // Activate cloud
    public void ActivateCloud(float duration)
    {
        // Set cloud to active  
        isActivated = true;
        activationTimer = duration;
        cloudRenderer.material.color = activatedColor;
        // Log activation
        Debug.Log($"Cloud {gameObject.name} activated for {duration} seconds!");
    }

    // Deactivate cloud
    private void DeactivateCloud()
    {
        // Set cloud to inactive
        isActivated = false;
        activationTimer = 0f;
        cloudRenderer.material.color = originalColor;
        // Log deactivation
        Debug.Log($"Cloud {gameObject.name} deactivated!");
    }

    // Spawn ink at the ground or existing ink
    private void DropInk()  
    {
        // Raycast down to find ground or existing ink
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f))
        {
            Debug.Log($"DropInk raycast hit: {hit.collider.gameObject.name} at position: {hit.point}");
            // Check if the hit object is ground or ink
            if (hit.collider.gameObject.CompareTag("Ground") || hit.collider.gameObject.CompareTag("Ink"))
            {
                // Create ink spot
                GameObject inkSpot = GameObject.CreatePrimitive(PrimitiveType.Plane);
                // Position ink spot above ground or existing ink
                inkSpot.transform.position = hit.point + Vector3.up * 0.01f;
                float inkSize = isBursting ? 0.3f : 0.2f;
                // Set ink size
                inkSpot.transform.localScale = new Vector3(inkSize, inkSize, inkSize);
                // Set ink material
                inkSpot.GetComponent<Renderer>().material = inkMaterial;
                // Tag ink
                inkSpot.tag = "Ink";
                Debug.Log($"Ink dropped at: {inkSpot.transform.position}");
            }
            else
            {
                Debug.Log($"Hit object not tagged as Ground or Ink: {hit.collider.gameObject.name}");
            }
        }
        else
        {
            Debug.Log("DropInk raycast missed!");
        }
    }

    // Combine with nearby cloud
    private void CombineWithNearbyCloud()
    {
        // Find nearby clouds
        Collider[] nearbyClouds = Physics.OverlapSphere(transform.position, combineRadius);
        // Iterate through nearby clouds
        foreach (Collider col in nearbyClouds)
        {
            // Check if the cloud is not the current cloud and is a cloud
            if (col.gameObject != gameObject && col.CompareTag("Cloud"))
            {
                // Get the other cloud script
                CloudBehavior otherCloud = col.GetComponent<CloudBehavior>();
                // If the other cloud script exists
                if (otherCloud != null && !otherCloud.hasCombined && otherCloud.target != player)
                {
                    // Create a big cloud from the two smaller clouds
                    CreateBigCloud(otherCloud);
                    return;
                }
            }
        }
    }

    // Spawn a big cloud from two smaller clouds
    private void CreateBigCloud(CloudBehavior otherCloud)
    {   
        // Mark both clouds as combined
        hasCombined = true;
        otherCloud.hasCombined = true;

        // Calculate midpoint between the two clouds
        Vector3 midPoint = (transform.position + otherCloud.transform.position) / 2;
        // Instantiate  (create) the big cloud at midpoint location
        GameObject bigCloud = Instantiate(bigCloudPrefab, midPoint, Quaternion.identity);
        // Get the big cloud script
        CloudBehavior bigCloudScript = bigCloud.GetComponent<CloudBehavior>();

        // If the big cloud script exists
        if (bigCloudScript != null)
        {
            // Assign the player as the target
            bigCloudScript.target = player;
            // Set rain interval for the big cloud
            bigCloudScript.rainInterval = 0.5f;
        }

        // Get the player combat script
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        // If the player combat script exists
        if (playerCombat != null)
        {
            // Assign the big cloud to the player
            playerCombat.currentCloud = bigCloud;
            Debug.Log("Big Cloud assigned to Player!");
        }

        // Destroy the original clouds
        Destroy(otherCloud.gameObject);
        Destroy(gameObject);
    }

    // Handle cloud collision
    void OnCollisionEnter(Collision collision)
    {
        // If the collision is with another cloud and it hasn't combined yet
        if (collision.gameObject.CompareTag("Cloud") && !hasCombined)
        {
            // Get the other cloud script
            CloudBehavior otherScript = collision.gameObject.GetComponent<CloudBehavior>();
            // If the other cloud script exists
            if (otherScript != null)
            {
                // Create a big cloud
                CreateBigCloud(otherScript);
            }
        }
    }
}