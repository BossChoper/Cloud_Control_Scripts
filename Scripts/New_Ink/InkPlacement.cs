using UnityEngine;

public class InkPlacement : MonoBehaviour
{
    // Ink Puddle Prefab    
    public GameObject inkPuddlePrefab;
    // Merge Distance
    public float mergeDistance = 2f;
    // Ground Layer
    public LayerMask groundLayer;
    // Main Camera
    public Camera mainCamera;

    void Update()
    {
        // Left mouse button to place ink
        if (Input.GetMouseButtonDown(0))
        {
            PlaceInk();
        }
    }

    void PlaceInk()
    {
        // Raycast from camera to mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        // RaycastHit
        RaycastHit hit;

        //Check if ray hits the ground
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            // Placement position
            Vector3 placementPosition = hit.point;

            //Check if there is already ink
            Collider[] nearbyObjects = Physics.OverlapSphere(placementPosition, inkPuddlePrefab.transform.localScale.x / 2);
            // Can place
            bool canPlace = true;

            // Check if there is already ink
            foreach(Collider col in nearbyObjects)
            {
                if(col.CompareTag("Ink"))
                {
                    canPlace = false;
                    break;
                }
            }

            // If can place
            if (canPlace)
            {
                // Instantiate new puddle
                GameObject newPuddle = Instantiate(inkPuddlePrefab, placementPosition, Quaternion.identity);
                newPuddle.tag = "Ink";
                //Align the puddle with uneven terrain
                newPuddle.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                MergeNearbyPuddles(newPuddle);
            }
        }
    }

    // Merge nearby puddles
    void MergeNearbyPuddles(GameObject newPuddle)
    {
        // Get nearby puddles
        Collider[] nearbyPuddles = Physics.OverlapSphere(newPuddle.transform.position, mergeDistance);
        // Merge
        foreach(Collider col in nearbyPuddles)
        {
            // Check if it is another ink puddle
            if(col.CompareTag("Ink") && col.gameObject != newPuddle)
            {
                // Simple merge: scale up the new puddle and destroy old one
                Vector3 newScale = newPuddle.transform.localScale;
                newScale.x += col.transform.localScale.x * 0.5f; // Increase size based on nearby puddle
                // newScale.y += col.transform.localScale.y * 0.5f; // Increases size of new puddle height; not needed
                newScale.z += col.transform.localScale.z * 0.5f;
                // Align the new puddle with uneven terrain
                // newPuddle.transform.rotation = Quaternion.FromToRotation(Vector3.up, col.transform.up);
                newPuddle.transform.localScale = newScale;
                // Destroy the old puddle
                // Destroy(col.gameObject);
            }
        }
    }
}