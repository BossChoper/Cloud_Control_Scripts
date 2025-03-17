using UnityEngine;

public class CarController : MonoBehaviour
{
    // Car movement variables
    public float speed = 10f;
    public float turnSpeed = 100f;

    void Start()
    {
        enabled = false; // Start disabled; car is not active immediately
    }

    void Update()
    {
        // Car movement
        float move = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;
        
        // Apply movement
        transform.Translate(0, 0, move);
        transform.Rotate(0, turn, 0);
    }
}