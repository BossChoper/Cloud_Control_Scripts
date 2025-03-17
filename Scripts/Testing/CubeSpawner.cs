using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour {
    // Reference to the CombineObjects script
    public GameObject bigCubePrefab; // The larger cube prefab
    public int numberOfCubes = 5;    // How many to spawn
    public float spawnRadius = 5f;   // Area to spawn them in

    void Start() {
        // Spawn small cubes
        for (int i = 0; i < numberOfCubes; i++) {
            SpawnSmallCube();
        }
    }

    // Spawn a small cube
    void SpawnSmallCube() {
        // Random position above ground
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = Mathf.Abs(spawnPos.y) + 0.5f; // Ensure it’s above ground

        // Create the cube
        GameObject smallCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        smallCube.transform.position = spawnPos;
        smallCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add physics
        smallCube.AddComponent<Rigidbody>();
        // Freeze rotation
        smallCube.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | 
                                                         RigidbodyConstraints.FreezeRotationY | 
                                                         RigidbodyConstraints.FreezeRotationZ;

        // Set tag
        smallCube.tag = "SmallCube";

        // Add and configure CombineObjects script
        CombineObjects combineScript = smallCube.AddComponent<CombineObjects>();
        // Assign the big cube prefab
        combineScript.bigCubePrefab = bigCubePrefab;
    }
}