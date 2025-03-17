using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Player movement variables
    public float speed = 5f;
    public GameObject car; // Assign in Inspector
    private bool isInCar = false;
    private CharacterController controller;
    private Renderer playerRenderer; // To hide/show player visuals

    void Start()
    {
        // Initialize components
        controller = GetComponent<CharacterController>();
        // If no controller is found, add one
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }
        
        // Get the Capsule's renderer
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer == null)
        {
            Debug.LogError("No Renderer found on Player! Add a visible mesh.");
        }
        if (car == null)
        {
            Debug.LogError("Car reference is not assigned in PlayerController!");
        }
    }

    void Update()
    {
        // Always check for Q to exit car, regardless of state
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q key pressed");
            // Check if player is in car
            if (isInCar)
            {
                // Exit car
                ExitCar();
            }
        }

        // Only allow movement when not in car
        if (!isInCar)
        {
            // Player movement
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            // Move player
            Vector3 move = new Vector3(moveX, 0, moveZ).normalized * speed * Time.deltaTime;
            controller.Move(move);

            // Enter car when near and pressing E
            if (car != null && Vector3.Distance(transform.position, car.transform.position) < 2f && Input.GetKeyDown(KeyCode.E))
            {
                EnterCar();
            }
        }
    }

    // Enter car
    void EnterCar()
    {
        Debug.Log("Entering car");
        isInCar = true;
        // Snap player to car position
        transform.position = car.transform.position;
        // Disable player movement
        controller.enabled = false; 
        // Hide player visuals
        if (playerRenderer != null) playerRenderer.enabled = false; 
        // Get car controller
        CarController carController = car.GetComponent<CarController>();
        // Enable car control
        if (carController != null)
        {
            carController.enabled = true; 
        }
        else
        {
            Debug.LogError("CarController script not found on Car object!");
        }
    }

    // Exit car
    void ExitCar()
    {
        Debug.Log("Exiting car");
        isInCar = false;
        // Spawn beside car
        transform.position = car.transform.position + new Vector3(3, 1, 0); 
        // Re-enable movement
        controller.enabled = true; 
        // Show player
        if (playerRenderer != null) playerRenderer.enabled = true; 
        // Get car controller
        CarController carController = car.GetComponent<CarController>();
        // Disable car control
        if (carController != null)
        {
            carController.enabled = false; 
        }
    }
}