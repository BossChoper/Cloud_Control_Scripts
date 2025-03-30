using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public float speed = 5f;
    public float gravity = -9.81f;
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private Vector3 velocity;
    private TimeFreezeMechanic timeFreeze; // Reference to check freeze state

    void Start()
    {
        controller = GetComponent<CharacterController>();
        timeFreeze = GetComponent<TimeFreezeMechanic>(); // Get the time freeze component
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float deltaTime = (timeFreeze != null && timeFreeze.IsTimeFrozen()) ? Time.unscaledDeltaTime : Time.deltaTime;

        // Movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        move.Normalize();
        controller.Move(move * speed * deltaTime);

        // Gravity
        velocity.y += gravity * deltaTime;
        controller.Move(velocity * deltaTime);
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}