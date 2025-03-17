using UnityEngine;

// This script should be added to the hook prefab or added dynamically
// This script should be added to the hook prefab or added dynamically
public class HookProjectile : MonoBehaviour
{
    [HideInInspector] public HookController parentController;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Kart")) // Tag your karts as "Kart"
        {
            if (parentController != null)
            {
                parentController.LatchOntoKart(collision.gameObject);
            }
            else
            {
                Debug.LogError("Hook projectile has no parent controller reference!");
            }
            
            Destroy(gameObject); // Destroy the hook projectile
        }
    }
}