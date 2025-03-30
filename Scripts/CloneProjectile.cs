using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneProjectile : MonoBehaviour
{
    public float speed = 20f;
    public GameObject playerPrefab;

    void Start()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Spawn clone
        if (collision.gameObject.CompareTag("Ground"))
        {
            Vector3 spawnPosition = collision.contacts[0].point + Vector3.up * 1f;
            GameObject clone = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            CloneBehavior cloneBehavior = clone.AddComponent<CloneBehavior>();
            Destroy(gameObject);
        }
    }
}
