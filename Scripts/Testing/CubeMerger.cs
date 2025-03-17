using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeMerger : MonoBehaviour
{
    // Cube prefabs
    public GameObject bigCubePrefab; // Assign the BigCube prefab in the Inspector
    public GameObject smallCubePrefab; // Assign the SmallCube prefab in the Inspector
    // Cube parameters
    public float proximityDistance = 2f; // Initial range to consider cubes
    public float explosionRadius = 3f; // Radius to pull nearby objects for explosion
    public float explosionPullForce = 15f; // Strength of the pull during explosion
    public float mergeSpeed = 2f; // Speed at which cubes move together
    public float breakForce = 10f; // Force applied when breaking cube apart
    public int smallCubesPerSide = 3; // Number of small cubes per side when breaking
    private bool isMerging = false; // Prevent multiple merges at once
    private bool isBreaking = false; // Prevent multiple breaks at once

    void Update()
    {
        // Check for merge button press (e.g., "E" key)
        if (Input.GetKeyDown(KeyCode.E) && !isMerging)
        {
            StartCoroutine(MergeCubes());
        }

        // Check for break button press (e.g., "Q" key)
        if (Input.GetKeyDown(KeyCode.Q) && !isBreaking)
        {
            StartCoroutine(BreakCube());
        }
    }

    // Merge cubes
    IEnumerator MergeCubes()
    {
        isMerging = true;

        // Find all cubes in the scene tagged with "MergeCube"
        GameObject[] allCubes = GameObject.FindGameObjectsWithTag("MergeCube");

        // Need at least 3 cubes
        if (allCubes.Length < 3)
        {
            Debug.Log("Need at least 3 cubes to merge!");
            isMerging = false;
            yield break;
        }

        // Step 1: Find the three closest cubes
        List<GameObject> closestCubes = FindClosestThreeCubes(allCubes);
        if (closestCubes.Count < 3)
        {
            Debug.Log("Couldn't find 3 cubes close enough!");
            isMerging = false;
            yield break;
        }

        // Step 2: Disable physics on the selected cubes
        List<Rigidbody> rigidbodies = new List<Rigidbody>();
        foreach (GameObject cube in closestCubes)
        {
            Rigidbody rb = cube.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rigidbodies.Add(rb);
                rb.isKinematic = true; // Disable physics, make it kinematic
            }
        }

        // Step 3: Calculate the center point
        Vector3 centerPoint = CalculateCenter(closestCubes);

        // Step 4: Move cubes toward the center slowly and pull nearby objects
        // Need to move everything towards the center point
        float elapsedTime = 0f;
        while (Vector3.Distance(closestCubes[0].transform.position, centerPoint) > 0.1f) // Stop when close enough
        {
            // Move merging cubes
            foreach (GameObject cube in closestCubes)
            {
                cube.transform.position = Vector3.MoveTowards(
                    cube.transform.position,
                    centerPoint,
                    mergeSpeed * Time.deltaTime
                );
            }

            // Pull nearby objects continuously
            Collider[] nearbyObjects = Physics.OverlapSphere(centerPoint, explosionRadius);
            foreach (Collider obj in nearbyObjects)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null && obj.tag != "MergeCube" && obj.tag != "BigCube")
                {
                    Vector3 direction = (centerPoint - obj.transform.position).normalized;
                    // Use a smoother force over time
                    float pullStrength = explosionPullForce * Time.deltaTime;
                    rb.AddForce(direction * pullStrength, ForceMode.Impulse);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Step 5: Destroy all affected objects and spawn big cube
        // First destroy all nearby objects that were being pulled
        Collider[] nearObjects = Physics.OverlapSphere(centerPoint, explosionRadius);
        foreach (Collider obj in nearObjects)
        {
            if (obj.tag != "BigCube") // Don't destroy any existing big cubes
            {
                Destroy(obj.gameObject);
            }
        }

        // Then destroy the merging cubes
        foreach (GameObject cube in closestCubes)
        {
            Destroy(cube);
        }
        
        // Spawn the new big cube
        Instantiate(bigCubePrefab, centerPoint, Quaternion.identity);

        isMerging = false;
    }

    IEnumerator BreakCube()
    {
        isBreaking = true;

        // Find all big cubes in the scene
        GameObject[] bigCubes = GameObject.FindGameObjectsWithTag("BigCube");
        
        if (bigCubes.Length == 0)
        {
            Debug.Log("No big cubes to break!");
            isBreaking = false;
            yield break;
        }

        // Get the first big cube (you could modify this to break specific cubes)
        GameObject bigCube = bigCubes[0];
        Vector3 bigCubePos = bigCube.transform.position;
        Vector3 bigCubeScale = bigCube.transform.localScale;

        // Calculate the size of each small cube
        float smallCubeSize = bigCubeScale.x / smallCubesPerSide;
        Vector3 smallCubeScale = new Vector3(smallCubeSize, smallCubeSize, smallCubeSize);

        // Calculate the offset to start spawning from
        float offset = (smallCubesPerSide - 1) * smallCubeSize * 0.5f;

        // Create small cubes in a grid pattern
        for (int x = 0; x < smallCubesPerSide; x++)
        {
            for (int y = 0; y < smallCubesPerSide; y++)
            {
                for (int z = 0; z < smallCubesPerSide; z++)
                {
                    Vector3 position = bigCubePos + new Vector3(
                        (x * smallCubeSize) - offset,
                        (y * smallCubeSize) - offset,
                        (z * smallCubeSize) - offset
                    );

                    GameObject smallCube = Instantiate(smallCubePrefab, position, Quaternion.identity);
                    smallCube.transform.localScale = smallCubeScale;

                    // Add explosion force
                    Rigidbody rb = smallCube.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 explosionDir = (position - bigCubePos).normalized;
                        rb.AddForce(explosionDir * breakForce, ForceMode.Impulse);
                    }
                }
            }
        }

        // Destroy the big cube
        Destroy(bigCube);
        
        yield return new WaitForSeconds(0.5f);
        isBreaking = false;
    }

    // Find the three closest cubes
    List<GameObject> FindClosestThreeCubes(GameObject[] cubes)
    {
        if (cubes.Length < 3) return new List<GameObject>();

        // Store all possible triplets and their total distance
        List<(List<GameObject> triplet, float totalDistance)> triplets = new List<(List<GameObject>, float)>();

        // Check every combination of 3 cubes
        for (int i = 0; i < cubes.Length - 2; i++)
        {
            for (int j = i + 1; j < cubes.Length - 1; j++)
            {
                for (int k = j + 1; k < cubes.Length; k++)
                {
                    GameObject cube1 = cubes[i];
                    GameObject cube2 = cubes[j];
                    GameObject cube3 = cubes[k];

                    // Calculate total distance between the three cubes
                    float dist12 = Vector3.Distance(cube1.transform.position, cube2.transform.position);
                    float dist13 = Vector3.Distance(cube1.transform.position, cube3.transform.position);
                    float dist23 = Vector3.Distance(cube2.transform.position, cube3.transform.position);
                    float totalDistance = dist12 + dist13 + dist23;

                    triplets.Add((new List<GameObject> { cube1, cube2, cube3 }, totalDistance));
                }
            }
        }

        // Sort by total distance and pick the closest triplet
        triplets.Sort((a, b) => a.totalDistance.CompareTo(b.totalDistance));
        return triplets.Count > 0 ? triplets[0].triplet : new List<GameObject>();
    }

    // Calculate the center of a list of cubes
    Vector3 CalculateCenter(List<GameObject> cubes)
    {
        Vector3 sum = Vector3.zero;
        foreach (GameObject cube in cubes)
        {
            sum += cube.transform.position;
        }
        return sum / cubes.Count;
    }
}