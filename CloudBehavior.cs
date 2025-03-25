using UnityEngine;

public class CloudBehavior : MonoBehaviour
{
    [Header("Cloud Properties")]
    public GameObject target;
    public Vector3 offset = new Vector3(0, 3f, 0);
    public Material inkMaterial;
    public GameObject bigCloudPrefab;
    public GameObject stormCloudPrefab;
    public GameObject superpowerPickupPrefab;
    public float rainInterval = 1f;
    public float hoverForce = 9.8f;
    public float combineRadius = 2f;
    public float followSpeed = 10f;
    public float verticalFollowSpeed = 2f;
    public float burstRainInterval = 0.2f;
    public float proximityDistance = 5f;
    public bool hasCombined = false;

    [Header("Cloud States")]
    private float lingerTime = 3f;
    private float lingerTimer = 0f;
    private float rainTimer = 0f;
    private float burstDuration = 0f;
    private bool enemyDead = false;
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

    [Header("Activation Properties")] 
    private bool isActivated = false;
    private float activationTimer = 0f;
    private Color activatedColor = Color.yellow;

    [Header("Ink Properties")]
    [SerializeField] private GameObject inkPuddlePrefab;
    private InkSystem inkSystem;
    private CloudTransform cloudTransform;
    private CloudMovement cloudMovement;

    void Start()
    {
        cloudTransform = GetComponent<CloudTransform>();
        if (cloudTransform == null)
        {
            cloudTransform = gameObject.AddComponent<CloudTransform>();
        }
        // Varies from cloud to cloud
        cloudTransform.bigCloudPrefab = bigCloudPrefab;
        cloudTransform.stormCloudPrefab = stormCloudPrefab;
        cloudTransform.superpowerPickupPrefab = superpowerPickupPrefab;
        cloudTransform.combineRadius = combineRadius;
        cloudTransform.target = target;
        cloudTransform.player = GameObject.FindWithTag("Player");

        inkSystem = GetComponent<InkSystem>();
        cloudRenderer = GetComponent<Renderer>();
        originalColor = cloudRenderer.material.color;
        player = GameObject.FindWithTag("Player");
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

        if (launched)
        {
            HandleLaunchedState();
            return;
        }

        UpdateRainTimer();

        if (target != null && !isPreparingLaunch)
        {
            FollowTarget();

            if (target != null && target.CompareTag("Enemy"))
            {
                Debug.Log($"Target {target.name} is an enemy, checking proximity...");
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

    private void HandleLaunchedState()
    {
        timeSinceLaunch += Time.deltaTime;

        if (timeSinceLaunch >= launchTargetLockoutDuration)
        {
            canReTarget = true;
        }

        ApplyHoverForce(0.5f);
        ConstrainLaunchTrajectory();
        UpdateRainTimer();

        if (canReTarget)
        {
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

    private void ApplyHoverForce(float multiplier = 1f)
    {
        if (rb != null)
        {
            rb.AddForce(Vector3.up * hoverForce * multiplier, ForceMode.Acceleration);
            Vector3 velocity = rb.velocity;
            velocity.y *= 0.98f;
            rb.velocity = velocity;
        }
    }

    private void ConstrainLaunchTrajectory()
    {
        if (rb == null) return;
        if (transform.position.y > launchStartPosition.y + maxLaunchHeight)
        {
            Vector3 velocity = rb.velocity;
            velocity.y = Mathf.Min(velocity.y, 0);
            rb.velocity = velocity;
            transform.position = new Vector3(transform.position.x, launchStartPosition.y + maxLaunchHeight, transform.position.z);
        }
        float distanceTraveled = Vector3.Distance(launchStartPosition, transform.position);
        if (timeSinceLaunch > 1f || distanceTraveled > launchDistanceLimit)
        {
            Vector3 velocity = rb.velocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            horizontalVelocity *= 0.7f;
            rb.velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        }
    }

    private void UpdateRainTimer()
    {
        rainTimer -= Time.deltaTime;
        if (rainTimer <= 0 && !isActivated)
        {
            DropInk();
            rainTimer = isBursting ? burstRainInterval : rainInterval;
            Debug.Log($"Rain triggered (burst: {isBursting})");
        }
    }

    private void FollowTarget()
    {
        Vector3 targetPos = target.transform.position;
        Vector3 desiredPos = targetPos + offset + new Vector3(0, Mathf.Sin(Time.time * 5f) * 0.2f, 0);

        Vector3 currentPos = transform.position;
        float horizontalLerpFactor = followSpeed * Time.deltaTime;
        float verticalLerpFactor = verticalFollowSpeed * Time.deltaTime;
        float newX = Mathf.Lerp(currentPos.x, desiredPos.x, horizontalLerpFactor);
        float newZ = Mathf.Lerp(currentPos.z, desiredPos.z, horizontalLerpFactor);
        float newY = Mathf.Lerp(currentPos.y, desiredPos.y, verticalLerpFactor);

        transform.position = new Vector3(newX, newY, newZ);
    }

    private void CheckEnemyProximity()
    {
        if (target == null)
        {
            Debug.Log("Target is null, cannot check proximity.");
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        Debug.Log($"Distance to enemy {target.name}: {distance}, Proximity Distance: {proximityDistance}");
       
        if (distance <= proximityDistance)
        {
            Debug.Log($"Enemy {target.name} is within proximity range ({distance} <= {proximityDistance})! Triggering transformation.");
            TransformCloudOnEnemyProximity();
        }
    }

    private void TransformCloudOnEnemyProximity()
    {
        cloudTransform.target = target;
        cloudTransform.TransformCloudOnEnemyProximity();
    }

    private void HandleTargetlessState()
    {
        if (!enemyDead)
        {
            enemyDead = true;
            lingerTimer = lingerTime;
        }
        else
        {
            if (rb != null && !rb.isKinematic)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            CheckForTargetBelow();
            UpdateLingerTimer();
        }
    }

    private void CheckForTargetBelow()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3.5f))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject == player)
            {
                target = player;
                lingerTimer = lingerTime;
                cloudRenderer.material.color = originalColor;
                Debug.Log("Cloud now following Player!");
                AssignCloudToPlayer();
            }
            else if (hitObject.CompareTag("Enemy"))
            {
                target = hitObject;
                lingerTimer = lingerTime;
                cloudRenderer.material.color = originalColor;
                Debug.Log($"Cloud now following Enemy: {hitObject.name}!");
            }
        }
    }

    private void AssignCloudToPlayer()
    {
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        if (playerCombat != null)
        {
            Debug.Log("Setting currentCloud to this Cloud!");
            playerCombat.currentCloud = gameObject;
        }
        else
        {
            Debug.LogWarning("PlayerCombat component not found on Player!");
        }
    }

    private void UpdateLingerTimer()
    {
        lingerTimer -= Time.deltaTime;
        float alpha = lingerTimer / lingerTime;
        cloudRenderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        if (lingerTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void PrepareForLaunch()
    {
        isPreparingLaunch = true;
        target = null;
        Debug.Log("Cloud preparing for launch, stopping follow behavior!");
    }

    public void Launch()
    {
        launched = true;
        target = null;
        lingerTimer = lingerTime;
        launchStartPosition = transform.position;
        timeSinceLaunch = 0f;
        canReTarget = false;
        isPreparingLaunch = false;
        Debug.Log("Cloud marked as launched!");
    }

    public void TriggerBurstRain()
    {
        isBursting = true;
        burstDuration = 2f;
        rainTimer = 0f;
        Debug.Log("Burst rain triggered!");
    }

    public void ActivateCloud(float duration)
    {
        isActivated = true;
        activationTimer = duration;
        cloudRenderer.material.color = activatedColor;
        Debug.Log($"Cloud {gameObject.name} activated for {duration} seconds!");
    }

    private void DeactivateCloud()
    {
        isActivated = false;
        activationTimer = 0f;
        cloudRenderer.material.color = originalColor;
        Debug.Log($"Cloud {gameObject.name} deactivated!");
    }

    private void DropInk()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10f))
        {
            Debug.Log($"DropInk raycast hit: {hit.collider.gameObject.name} at position: {hit.point}");
            if (hit.collider.gameObject.CompareTag("Ground") || hit.collider.gameObject.CompareTag("Ink"))
            {
                inkSystem.PlaceInk(hit);
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
    
    // Refactored
    private void CombineWithNearbyCloud()
    {
        cloudTransform.target = player;
        cloudTransform.CombineWithNearbyCloud();
    }

    // Refactored
    private void CreateBigCloud(CloudBehavior otherCloud)
    {
        cloudTransform.target = player;
        cloudTransform.CreateBigCloud(otherCloud);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cloud") && !hasCombined)
        {
            CloudBehavior otherScript = collision.gameObject.GetComponent<CloudBehavior>();
            if (otherScript != null)
            {
                CreateBigCloud(otherScript);
            }
        }
    }
}