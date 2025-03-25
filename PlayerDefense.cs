using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDefense : MonoBehaviour
{
    // Block
    private int blockHitsRemaining = 3;
    private bool isBlocking = false;
    public GameObject blockEffectPrefab;
    private float blockResetTimer = 5f;

    // Parry variables
    private bool isParrying = false;
    private float parryWindow = 0.5f;
    private float parryTimer = 0f;
    public float parryForce = 10f;
    public GameObject parryEffectPrefab;

    public KeyCode blockKey = KeyCode.J;
    public KeyCode parryKey = KeyCode.K;

    // Update is called once per frame
    void Update()
    {
        // Handle block input
        if (Input.GetKeyDown(blockKey) && blockHitsRemaining > 0)
        {
            StartBlocking();
        }
        if (Input.GetKeyUp(blockKey))
        {
            StopBlocking();
        }

        // Handle parry input
        if (Input.GetKeyDown(parryKey) && !isBlocking)
        {
            StartParry();
        }
        if (Input.GetKeyUp(parryKey))
        {
            StopParry();
        }

        // Update parry timer
        if (isParrying)
        {
            parryTimer -= Time.deltaTime;
            if (parryTimer <= 0)
            {
                StopParry();
            }
        }

        if(blockHitsRemaining <= 0)
        {
            UpdateBlockTimer();
        }

    }

    void StartBlocking()
    {
        isBlocking = true;
        Debug.Log("Blocking started");
    }

    void StopBlocking()
    {
        isBlocking = false;
        Debug.Log("Blocking stoppped");
    }

    void StartParry()
    {
        isParrying = true;
        parryTimer = parryWindow;
        Debug.Log("Parry started");
    }

    void StopParry()
    {
        isParrying = false;
        Debug.Log("Parry stopped");
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Projectile"))
        {
            if(isParrying)
            {
                ReflectProjectile(collision);
            }
            else if (isBlocking)
            {
                BlockProjectile(collision);
            }
            else
            {
                // Normal hit logic here if neither blocking nor parrying
                Debug.Log("Player hit by projectile!");
            }
        }
    }

    void UpdateBlockTimer()
    {
        if (blockResetTimer > 0)
        {
            blockResetTimer -= Time.deltaTime;
            Debug.Log($"Block reset timer: {blockResetTimer}");
            if (blockResetTimer <= 0)
            {
                ResetBlock();
            }
        }
    }

    void BlockProjectile(Collision collision)
    {
        blockHitsRemaining--;

        // Spawn block prefab
        if (blockEffectPrefab != null)
        {
            GameObject block = Instantiate(blockEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            Destroy(block, 1f);
        }
        // Destroy projectile
        Destroy(collision.gameObject);
        Debug.Log($"Block successful! Hits remaining: {blockHitsRemaining}");
        if(blockHitsRemaining <= 0)
        {
            StopBlocking();
            Debug.Log("Block broken!");
        }
        
    }

    void ReflectProjectile(Collision collision)
    {
        // Get projectile rigidbody
        Rigidbody projectileRb = collision.gameObject.GetComponent<Rigidbody>();
        if (projectileRb != null)
        {
            // Calculate reflection direction
            Vector3 reflectionDirection = -collision.relativeVelocity.normalized;

            // Apply force
            projectileRb.velocity = reflectionDirection * parryForce;

            // Spawn parry effect if assigned
            if (parryEffectPrefab != null)
            {
                GameObject parry = Instantiate(parryEffectPrefab, collision.contacts[0].point, Quaternion.identity);
                Destroy(parry, 1f);
            }

            Debug.Log("Projectile parried!");
        }
        StopParry();
    }

    // Call this to reset block (e.g., after a cooldown or level reset)
    public void ResetBlock()
    {
        blockHitsRemaining = 3;
        Debug.Log("Block reset!");
    }

}
