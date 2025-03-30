using UnityEngine;

public class PlayerMovement1 : MonoBehaviour
{
    // Adjustable variables in Inspector
    [SerializeField] private float moveSpeed = 5f;         // Movement speed
    [SerializeField] private float jumpForce = 5f;         // Jump strength
    [SerializeField] private float gravity = -9.81f;       // Gravity strength
    [SerializeField] private LayerMask groundLayer;        // Layer for ground detection
    [SerializeField] private float groundCheckDistance = 0.4f; // Distance to check for ground

    private Rigidbody rb;                                  // Player's rigidbody
    private Vector3 velocity;                              // Velocity for smooth movement
    private bool isGrounded;                               // Ground check

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check if player is grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Get input for movement
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right Arrow
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S or Up/Down Arrow

        // Calculate movement direction
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // Move the player
        Move(moveDirection);
    }

    // PHYSICS
    void FixedUpdate()
    {
        // Apply gravity manually
        if (!isGrounded)
        {
            velocity.y += gravity * Time.fixedDeltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = 0; // Reset vertical velocity when grounded
        }

        // Apply velocity to Rigidbody
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    void Move(Vector3 direction)
    {
        // Calculate horizontal movement
        Vector3 moveVelocity = direction * moveSpeed;
        velocity.x = moveVelocity.x;
        velocity.z = moveVelocity.z;

        // Rotate player to face movement direction (optional)
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void Jump()
    {
        velocity.y = jumpForce;
    }

    // Visualize ground check in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}