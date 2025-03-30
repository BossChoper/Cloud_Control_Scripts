using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneBehavior : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    public float lifetime = 10f;
    private float nextFireTime = 0f;

    void Start()
    {
        // Disable movement and controller
        Destroy(GetComponent<PlayerMovement>());
        Destroy(GetComponent<CharacterController>());
        Destroy(GetComponent<Shooting>());
        

        firePoint = transform.Find("Camera/ArmCannon");
        if (firePoint == null)
        {
            Debug.LogError("Fire point not found on clone!");
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (Time.time >= nextFireTime && firePoint != null)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void Shoot()
    {
        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}
