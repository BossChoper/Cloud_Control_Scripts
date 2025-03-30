using UnityEngine;
using System.Collections;

public class TimeFreezeMechanic : MonoBehaviour
{
    public float freezeDuration = 5f;
    public float freezeCooldown = 10f;
    private float nextFreezeTime = 0f;
    private bool isTimeFrozen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && Time.time >= nextFreezeTime && !isTimeFrozen)
        {
            StartCoroutine(FreezeTime());
        }
    }

    IEnumerator FreezeTime()
    {
        isTimeFrozen = true;
        // Do not do Time.timeScale = 0f;
        FreezeAllEnemies();
        Debug.Log("Time frozen for " + freezeDuration + " seconds");

        float elapsedTime = 0f;
        while (elapsedTime < freezeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        EndTimeFreeze();
    }

    void EndTimeFreeze()
    {
        isTimeFrozen = false;
        // Do not reset Time.timeScale = 1f;
        UnfreezeAllEnemies();
        nextFreezeTime = Time.time + freezeCooldown;
        Debug.Log("Time resumed");
    }

    void FreezeAllEnemies()
    {
        EnemyBehavior[] enemies = FindObjectsOfType<EnemyBehavior>();
        foreach (EnemyBehavior enemy in enemies)
        {
            enemy.Freeze();
        }
    }

    void UnfreezeAllEnemies()
    {
        EnemyBehavior[] enemies = FindObjectsOfType<EnemyBehavior>();
        foreach (EnemyBehavior enemy in enemies)
        {
            enemy.Unfreeze();
        }
    }

    public bool IsTimeFrozen()
    {
        return isTimeFrozen;
    }
}