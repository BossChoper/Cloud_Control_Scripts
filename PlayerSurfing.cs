using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSurfing : MonoBehaviour
{
    [SerializeField] private float surfSpeed = 10f;
    [SerializeField] private float surfTurnSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    
    private Rigidbody playerRb;
    private bool isSurfing;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleSurfingMovement();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SurfJump();
        }
    }

    private void HandleSurfingMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Create movement direction from input
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
        
        if (inputDirection.magnitude >= 0.1f)
        {
            // Gradually rotate the player to face movement direction
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.y;
            float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, surfTurnSpeed * Time.fixedDeltaTime * 90f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            // Move in the input direction
            playerRb.velocity = new Vector3(inputDirection.x * surfSpeed, playerRb.velocity.y, inputDirection.z * surfSpeed);
        }
        else if (isSurfing)
        {
            // Maintain forward momentum when no input is given
            playerRb.velocity = new Vector3(transform.forward.x * surfSpeed, playerRb.velocity.y, transform.forward.z * surfSpeed);
        }
    }

    public void StartSurfing()
    {
        if (!isSurfing && isGrounded)
        {
            isSurfing = true;
            
            // Get current movement input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
            
            // If there's no input, start moving forward
            if (inputDirection.magnitude < 0.1f)
            {
                playerRb.velocity = new Vector3(transform.forward.x * surfSpeed, playerRb.velocity.y, transform.forward.z * surfSpeed);
            }
            else
            {
                // Start moving in input direction
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
                playerRb.velocity = new Vector3(inputDirection.x * surfSpeed, playerRb.velocity.y, inputDirection.z * surfSpeed);
            }
            
            Debug.Log("Started surfing");
        }
    }

    public void SurfJump()
    {
        if (isGrounded && isSurfing)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    public void StopSurfing()
    {
        if (isSurfing)
        {
            isSurfing = false;
            // Optional: Remove visual effects or animations here
            Debug.Log("Stopped surfing");
        }
    }
}
