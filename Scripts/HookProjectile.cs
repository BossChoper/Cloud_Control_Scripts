using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookProjectile : MonoBehaviour
{  
    [HideInInspector] public HookController parentController;

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            if(parentController != null)
            {
                parentController.LatchOnToKart(collision.gameObject);
            }
            else{
                Debug.LogError("Hook projectile has no parent controller reference!");
            }
            Destroy(gameObject);
        }
    }
}
