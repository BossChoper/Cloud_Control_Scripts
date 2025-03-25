using UnityEngine;
using System.Collections.Generic;

// Script organizes minions in queue for following
public class CloudieManager : MonoBehaviour
{
    [Header("Minion Settings")] 
    public Transform player; // Reference to the Player
    public List<CloudieBehavior> minionQueue = new List<CloudieBehavior>(); // Ordered list of free Minions
    public float spacing = 2f; // Distance between Minions in the queue

    void Start()
    {
        // Find the Player
        if (player == null)
            player = GameObject.Find("Player").transform;

        // Initialize the queue with all Minions in the scene
        GameObject[] minions = GameObject.FindGameObjectsWithTag("Cloudie");
        foreach (GameObject minionObj in minions)
        {
            CloudieBehavior minion = minionObj.GetComponent<CloudieBehavior>();
            if (minion != null && !minion.IsAssignedToCarry)
            {
                minionQueue.Add(minion);
                //minion.SetManager(this);
            }
        }
    }

    public void AddMinionToQueue(CloudieBehavior minion)
    {
        if (!minionQueue.Contains(minion) && !minion.IsAssignedToCarry)
        {
            minionQueue.Add(minion); // Add to the back of the queue
        }
    }

    public CloudieBehavior GetFirstAvailableMinion()
    {
        if (minionQueue.Count > 0)
        {
            CloudieBehavior minion = minionQueue[0];
            minionQueue.RemoveAt(0); // Remove from the front
            return minion;
        }
        return null;
    }

    public Vector3 GetQueuePosition(int index)
    {
        if (index == 0)
        {
            // First Minion follows directly behind the Player
            return player.position - player.forward * spacing;
        }
        else
        {
            // Subsequent Minions follow the previous Minion
            CloudieBehavior previousMinion = minionQueue[index - 1];
            return previousMinion.transform.position - previousMinion.transform.forward * spacing;
        }
    }

    public int GetQueueCount()
    {
        return minionQueue.Count;
    }
}