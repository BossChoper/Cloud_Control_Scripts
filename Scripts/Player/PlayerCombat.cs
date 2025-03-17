using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    // Cloud variables
    public GameObject cloudPrefab;
    [HideInInspector] public GameObject currentCloud;
    public float cloudLaunchSpeed = 20f;

    // Jump variables
    public float jumpForce = 10f;
    public float aerialBoost = 3f;
    public int maxAerialCombo = 3;
    public float slamForce = 15f;
    public float aerialLift = 2f;

    // Combo variables  
    private float comboTimer = 0f;
    private float comboWindow = 2f;
    private bool lastWasSpace = false;
    public int comboCount = 0;
    public string styleRank = "D";
    public TextMeshProUGUI comboText;

    // Enemy variables
    public GameObject enemy;
    private Rigidbody enemyRb;
    private Renderer enemyRenderer;
    private Color originalColor;
    private float flashDuration = 0.1f;
    private float flashTimer = 0f;

    // Player variables
    public float attackRange = 2f;
    private Rigidbody playerRb;
    private bool isGrounded = true;
    private int hitCount = 0;
    private int aerialHitCount = 0;
    private Camera mainCamera;

    // Superpower variables
    private bool hasSuperpower = false;
    private float superpowerDuration = 10f;
    private float superpowerTimer = 0f;
    private Color superpowerColor = Color.red;
    private Renderer playerRenderer;

    // Enemy weight system
    private float enemyWeight = 1f; // Starting weight (1 = normal)
    private float weightReductionPerHit = 0.2f; // How much weight decreases per hit
    private float minWeight = 0.1f; // Minimum weight to avoid zero/negative
    private float weightResetDelay = 3f; // Time before weight resets
    private float lastHitTimer = 0f; // Tracks time since last hit

    void Start()
    {
        // Initialize enemy 
        InitializeEnemy();
        // Initialize player
        playerRb = GetComponent<Rigidbody>();
        // Initialize camera
        mainCamera = Camera.main;
        // Initialize player renderer
        playerRenderer = GetComponent<Renderer>();
        // Initialize original color
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("Player Renderer not found! Superpower color change won’t work.");
        }
        // Initialize main camera
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Cursor system requires a camera.");
        }
    }

    private void InitializeEnemy()
    {
        // Initialize enemy
        if (enemy == null)
        {
            // Find enemy by tag
            enemy = GameObject.FindWithTag("Enemy");
            // Check if enemy was found
            if (enemy == null)
            {
                Debug.LogError("No GameObject tagged 'Enemy' found in the scene!");
                return;
            }
        }
        // Initialize enemy components
        enemyRb = enemy.GetComponent<Rigidbody>();
        enemyRenderer = enemy.GetComponent<Renderer>();
        // Initialize original color
        if (enemyRenderer != null) originalColor = enemyRenderer.material.color;
    }

    void Update()
    {
        // Update timers
        UpdateTimers();
        // Check if player is grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1f);
        // Handle player inputs
        HandleJumpInput();
        HandleLaunchInput();
        HandleAttackInput();
        HandleSlamInput();
        HandleTestCloudInput(); 
        HandleCursorInput();
        HandleSuperpower();
        // Update enemy weight
        UpdateEnemyWeight();
        // Validate current cloud
        ValidateCurrentCloud();
    }

    private void UpdateTimers()
    {
        // Update combo timer
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0) ResetCombo();
        }
        // Update flash timer
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0 && enemyRenderer != null) enemyRenderer.material.color = originalColor;
        }
        // Update last hit timer
        if (lastHitTimer > 0)
        {
            lastHitTimer -= Time.deltaTime;
            if (lastHitTimer <= 0 && enemyWeight < 1f)
            {
                ResetEnemyWeight();
            }
        }
    }

    private void HandleJumpInput()
    {
        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // Apply jump force
            playerRb.velocity = new Vector3(playerRb.velocity.x, jumpForce, playerRb.velocity.z);
            // Set player as airborne
            isGrounded = false;
            
            Debug.Log("Player jumped!");
        }
    }

    // Handle launch input
    private void HandleLaunchInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && isGrounded && enemy != null)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < attackRange && enemyRb != null)
            {
                // Scale launch height inversely with weight (lighter = higher)
                float launchHeight = 5f / enemyWeight; // Base 5f, height increases as weight decreases
                enemyRb.velocity = new Vector3(0, launchHeight, 0);
                comboCount += 2;
                comboTimer = comboWindow;
                UpdateStyleRank();
                FlashEnemy();
                ReduceEnemyWeight(); // Reduce weight on launch
                Debug.Log($"Launched! Height: {launchHeight}, Weight: {enemyWeight}, Combo: {comboCount}");
            }
        }

        // Handle cloud launch
        if (Input.GetKeyDown(KeyCode.Q))
        {
            LaunchCloud();
        }
    }

    // Handle attack input
    private void HandleAttackInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && enemy != null)
        {
            // Handle attack input
            float distance = Vector3.Distance(transform.position, enemy .transform.position);
            if (distance < attackRange)
            {
                // Handle attack input
                UpdateCombo(false);
                hitCount++;
                lastHitTimer = weightResetDelay; // Reset timer on hit
                ReduceEnemyWeight(); // Reduce weight on hit
                comboTimer = comboWindow;
                UpdateStyleRank();
                FlashEnemy();
                Debug.Log($"Combo: {comboCount} | Rank: {styleRank} | Weight: {enemyWeight}");

                if (!isGrounded)
                {
                    if (aerialHitCount < maxAerialCombo)
                    {
                        // Scale aerial boost inversely with weight
                        float adjustedBoost = aerialBoost / enemyWeight;
                        playerRb.velocity = new Vector3(playerRb.velocity.x, adjustedBoost, playerRb.velocity.z);
                        enemyRb.velocity = new Vector3(enemyRb.velocity.x, adjustedBoost, enemyRb.velocity.z);
                        aerialHitCount++;
                        comboCount += 1;
                        Debug.Log($"Aerial hit #{aerialHitCount}! Boost: {adjustedBoost}, Weight: {enemyWeight}");
                    }
                    else
                    {
                        playerRb.velocity = new Vector3(playerRb.velocity.x, aerialLift, playerRb.velocity.z);
                        comboCount += 1;
                        Debug.Log("Aerial attack! Extra combo point, max height reached.");
                    }
                }
                if (enemy.transform.position.y > 1.5f)
                {
                    comboCount += 1;
                    Debug.Log("Aerial hit! Extra combo point!");
                }

                if (hitCount == 2 && currentCloud == null)
                {
                    SpawnCloudAboveEnemy();
                }
                if (currentCloud != null)
                {
                    Vector3 currentScale = currentCloud.transform.localScale;
                    currentScale += new Vector3(0.1f, 0.1f, 0.1f);
                    currentCloud.transform.localScale = currentScale;
                }
                if (hitCount > 2)
                {
                    Destroy(enemy);
                    enemy = null;
                    hitCount = 0;
                    aerialHitCount = 0;
                    enemyWeight = 1f; // Reset weight on enemy death
                }
            }
        }
    }

    // Handle slam input
    private void HandleSlamInput()
    {
        if (Input.GetKeyDown(KeyCode.S) && !isGrounded && enemy != null)
        {
            playerRb.velocity = new Vector3(playerRb.velocity.x, -slamForce, playerRb.velocity.z);
            enemyRb.velocity = new Vector3(enemyRb.velocity.x, -slamForce, enemyRb.velocity.z);
            Debug.Log("Slam! Player and enemy forced downward.");
            comboCount += 2;
            UpdateStyleRank();
            aerialHitCount = 0;
        }
    }

    // Handle test cloud input
    private void HandleTestCloudInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && enemy != null)
        {
            Debug.Log("Spawning test Cloud near Player, targeting enemy: " + enemy.name);
            GameObject testCloud = InstantiateCloud(transform.position + Vector3.up * 3f + Vector3.right * 2f);
            testCloud.GetComponent<CloudBehavior>().target = enemy;
            testCloud.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            Debug.Log("Test Cloud spawned!");
        }
    }

    // Validate current cloud
    private void ValidateCurrentCloud()
    {
        if (currentCloud != null && currentCloud.GetComponent<CloudBehavior>() == null)
        {
            // Clear current cloud if CloudBehavior is missing
            Debug.Log("Clearing currentCloud because CloudBehavior is missing!");
            currentCloud = null;
        }
    }

    // Spawn a cloud above the enemy
    private void SpawnCloudAboveEnemy()
    {
        // Log Cloud spawning
        Debug.Log("Spawning Cloud above enemy: " + enemy.name + " (Position: " + enemy.transform.position + ")");
        // Instantiate cloud
        currentCloud = InstantiateCloud(enemy.transform.position + Vector3.up * 3f);
        // Assign target
        currentCloud.GetComponent<CloudBehavior>().target = enemy;
        // Set scale
        currentCloud.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        // Log Cloud spawned
        Debug.Log("Cloud spawned and assigned to currentCloud!");
    }

    // Instantiate a cloud
    private GameObject InstantiateCloud(Vector3 position)
    {
        // Log cloud instantiation
        Debug.Log("Instantiating Cloud at: " + position);
        // Instantiate cloud
        GameObject cloud = Instantiate(cloudPrefab, position, Quaternion.identity);
        // Add Rigidbody
        Rigidbody cloudRb = cloud.AddComponent<Rigidbody>();
        cloudRb.isKinematic = true;
        cloudRb.useGravity = false;
        cloudRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        cloud.tag = "Cloud";
        // Add CloudBehavior
        cloud.AddComponent<CloudBehavior>();
        // Log cloud instantiation
        Debug.Log("Cloud instantiated!");
        return cloud;
    }

    // Handle cursor input
    private void HandleCursorInput()
    {

        if (Input.GetMouseButtonDown(0) && mainCamera != null)
        {
            // Create raycast
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            // Check for collision
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Get the CloudBehavior component
                CloudBehavior cloud = hit.collider.GetComponent<CloudBehavior>();   
                // Check if the component exists
                if (cloud != null)
                {
                    // Log Cloud activation
                    Debug.Log($"Activated cloud: {hit.collider.gameObject.name}");
                    cloud.ActivateCloud(2f);
                }
            }
        }
    }

    // Launch cloud
    private void LaunchCloud()
    {
        // Debug
        Debug.Log("Q pressed!");
        // Check if there is a current cloud
        if (currentCloud == null)
        {
            Debug.Log("No current Cloud!");
            return;
        }
        // Log current Cloud
        Debug.Log("Current Cloud exists: " + currentCloud.name);
        // Get CloudBehavior
        CloudBehavior cloudBehavior = currentCloud.GetComponent<CloudBehavior>();
        // Check if CloudBehavior exists
        if (cloudBehavior == null)
        {
            Debug.LogWarning("No CloudBehavior component on currentCloud!");
            return;
        }
        // Log Cloud target
        Debug.Log("CloudBehavior found, Target: " + (cloudBehavior.target != null ? cloudBehavior.target.name : "None"));
        // Check if Cloud is following Player
        if (cloudBehavior.target != gameObject)
        {
            Debug.Log("Cloud not following Player! Target: " + (cloudBehavior.target != null ? cloudBehavior.target.name : "None"));
            return;
        }
        // Log Cloud is following Player
        Debug.Log("Cloud is following Player!");
        // Get Rigidbody
        Rigidbody cloudRb = currentCloud.GetComponent<Rigidbody>();
        // Check if Rigidbody exists
        if (cloudRb == null)
        {
            Debug.LogWarning("No Rigidbody on Cloud!");
            return;
        }
        // Prepare for launch   
        cloudBehavior.PrepareForLaunch();
        cloudRb.isKinematic = false;
        cloudRb.useGravity = true;
        // Log launch preparation
        Debug.Log("Cloud prepared for launch!");
        // Set launch direction
        Vector3 launchDirection = playerRb.velocity.normalized;
        // Adjust launch direction
        if (launchDirection.magnitude < 0.1f)
        {
            // Use forward direction if no velocity
            launchDirection = transform.forward.normalized;
        }
        launchDirection += Vector3.up * 0.05f;
        launchDirection = launchDirection.normalized;
        // Adjust launch speed
        float adjustedLaunchSpeed = cloudLaunchSpeed * 3f;
        cloudRb.velocity = launchDirection * adjustedLaunchSpeed;
        // Log launch direction and velocity
        Debug.Log("Launch Direction: " + launchDirection + ", Velocity set to: " + cloudRb.velocity);
        // Launch cloud
        cloudBehavior.Launch();
        // Log launch
        StartCoroutine(LogPositionAfterLaunch(currentCloud));
        // Reset current cloud
        currentCloud = null;
        Debug.Log("Cloud launched!");
    }

    // Log cloud position after launch
    private System.Collections.IEnumerator LogPositionAfterLaunch(GameObject cloud)
    {
        // Log cloud position after launch
        for (int i = 0; i < 5; i++)
        {
            // Wait for 0.5 seconds
            yield return new WaitForSeconds(0.5f);
            // Check if cloud still exists
            if (cloud != null)
            {
                // Log cloud position
                Debug.Log("Cloud position after launch: " + cloud.transform.position);
            }
        }
    }

    // Handle superpower
    private void HandleSuperpower()
    {
        // Handle superpower
        if (hasSuperpower)
        {
            // Decrement superpower timer
            superpowerTimer -= Time.deltaTime;
            // Check if superpower duration has expired
            if (superpowerTimer <= 0)
            {
                DeactivateSuperpower();
            }
        }
    }

    // Activate superpower
    private void ActivateSuperpower()
    {
        // Activate superpower flag 
        hasSuperpower = true;
        // Start superpower timer
        superpowerTimer = superpowerDuration;
        // Change player color to superpower color
        if (playerRenderer != null)
        {
            playerRenderer.material.color = superpowerColor;
        }
        // Log superpower activation
        Debug.Log("Superpower activated! Player color changed to red.");
    }

    // Deactivate superpower
    private void DeactivateSuperpower()
    {
        // Deactivate superpower
        hasSuperpower = false;
        // Reset superpower timer
        superpowerTimer = 0f;
        // Reset player color
        if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
        }   
        // Log superpower deactivation
        Debug.Log("Superpower deactivated! Player color reverted.");
    }

    private void UpdateEnemyWeight()
    {
        // Managed in UpdateTimers to reset weight after delay
    }

    // Reduce enemy weight
    private void ReduceEnemyWeight()
{
    // Reduce enemy weight
    enemyWeight = Mathf.Max(minWeight, enemyWeight - weightReductionPerHit);
    
    lastHitTimer = weightResetDelay;

    // Update enemy color; lighter = redder
    if (enemyRenderer != null)
    {
        enemyRenderer.material.color = Color.Lerp(Color.red, originalColor, enemyWeight); // Lighter = redder
    }
    // Log enemy weight reduction
    Debug.Log($"Enemy weight reduced to: {enemyWeight}");
}

    // Reset enemy weight
    private void ResetEnemyWeight()
    {
        enemyWeight = 1f;

        if (enemyRenderer != null)
        {
            // Reset enemy color 
            enemyRenderer.material.color = originalColor;
        }   
        // Log enemy weight reset
        Debug.Log("Enemy weight reset to 1!");
    }

    // Update style rank
    void UpdateStyleRank()
    {
        // Update style rank based on combo count
        if (comboCount >= 15) styleRank = "S";
        else if (comboCount >= 10) styleRank = "A";
        else if (comboCount >= 6) styleRank = "B";
        else if (comboCount >= 3) styleRank = "C";
        else styleRank = "D";
        // Update combo display
        comboText.text = "Combo: " + comboCount + " | " + styleRank;
    }

    // Reset combo
    void ResetCombo()
    {
        // Reset combo count and style rank
        comboCount = 0;
        styleRank = "D";
        // Update combo display
        comboText.text = "Combo: 0 | D";
        // Reset aerial hit count
        aerialHitCount = 0;
        // Log combo being dropped
        Debug.Log("Combo dropped!");
    }

    // Update combo string
    void UpdateCombo(bool isSpace)
    {
        // Check if space was pressed and last button was different attack
        if (isSpace != lastWasSpace) comboCount += 2;
        // If same attack, increment by 1
        else comboCount++;
        // Update entered combo string
        lastWasSpace = isSpace;
    }

    // Start enemy flashing
    void FlashEnemy()
    {
        // Check if enemy renderer exists
        if (enemyRenderer != null)
        {
            // Flash enemy
            enemyRenderer.material.color = Color.white;
            flashTimer = flashDuration;
        }
    }
    // Handle superpower pickup
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SuperpowerPickup"))
        {
            // Activate superpower
            ActivateSuperpower();
            // Destroy the pickup
            Destroy(other.gameObject);
            // Log the event
            Debug.Log("Collected Superpower Pickup!");
        }
    }
}