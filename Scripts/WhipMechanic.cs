using UnityEngine;

public class WhipMechanic : MonoBehaviour
{
    public Transform whipPoint;
    public float grabRange = 5f;
    public float tossForce = 10f;
    private GameObject grabbedEnemy = null;
    private bool isGrabbing = false;
    private TimeFreezeMechanic timeFreeze; // Reference to time freeze

    void Start()
    {
        timeFreeze = GetComponent<TimeFreezeMechanic>(); // Get the component
    }

    void Update() // Still works when timeScale > 0
    {
        if (Time.timeScale > 0f)
        {
            HandleInput();
            if (isGrabbing && grabbedEnemy != null)
            {
                HoldEnemy();
            }
        }
    }

    void FixedUpdate() // Works during time freeze (timeScale = 0)
    {
        if (Time.timeScale == 0f)
        {
            HandleInput();
            if (isGrabbing && grabbedEnemy != null)
            {
                HoldEnemy();
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isGrabbing)
            {
                GrabEnemy();
            }
            else
            {
                ReleaseEnemy();
            }
        }

        if (isGrabbing && grabbedEnemy != null)
        {
            CheckTossInput();
        }
    }

    void GrabEnemy()
    {
        RaycastHit hit;
        float adjustedRange = timeFreeze != null && timeFreeze.IsTimeFrozen() ? grabRange * 2f : grabRange; // Double range during freeze
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, adjustedRange))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                grabbedEnemy = hit.collider.gameObject;
                isGrabbing = true;
                Rigidbody rb = grabbedEnemy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true; // Already set by Freeze, but ensures it stays
                }
            }
        }
    }

    void HoldEnemy()
    {
        if (grabbedEnemy != null)
        {
            grabbedEnemy.transform.position = whipPoint.position + whipPoint.forward * 1f;
        }
    }

    void CheckTossInput()
    {
        Vector3 tossDirection = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W)) tossDirection = Vector3.forward;
        else if (Input.GetKeyDown(KeyCode.S)) tossDirection = Vector3.back;
        else if (Input.GetKeyDown(KeyCode.A)) tossDirection = Vector3.left;
        else if (Input.GetKeyDown(KeyCode.D)) tossDirection = Vector3.right;

        if (tossDirection != Vector3.zero)
        {
            TossEnemy(tossDirection);
        }
    }

    void TossEnemy(Vector3 direction)
    {
        if (grabbedEnemy != null)
        {
            Rigidbody rb = grabbedEnemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(direction * tossForce, ForceMode.Impulse);
            }
            isGrabbing = false;
            grabbedEnemy = null;
        }
    }

    void ReleaseEnemy()
    {
        if (grabbedEnemy != null)
        {
            Rigidbody rb = grabbedEnemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
            isGrabbing = false;
            grabbedEnemy = null;
        }
    }
}