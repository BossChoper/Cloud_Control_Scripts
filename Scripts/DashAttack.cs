using UnityEngine;
// FUNCTIONAL
public class DashAttack : MonoBehaviour
{
    // Adjustable variables in Inspector
    [SerializeField] private float phaseDistance = 2f;     // Distance to appear behind enemy
    [SerializeField] private float phaseRange = 1.5f;      // Range to detect enemy for phasing
    [SerializeField] private float phaseCooldown = 1f;     // Cooldown between phase attacks
    [SerializeField] private LayerMask enemyLayer;         // Layer for enemies
    
    private float lastPhaseTime;                           // Track last phase attack time
    private Animator animator;                             // Optional: for attack animation

    void Start()
    {
        animator = GetComponent<Animator>();               // Get animator if you have one
    }

    void Update()
    {
        // Check for phase attack input (e.g., "F" key) and cooldown
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= lastPhaseTime + phaseCooldown)
        {
            TryPhaseAttack();
        }
    }

    void TryPhaseAttack()
    {
        // Detect enemies in range
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, phaseRange, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            // Get the closest enemy
            Collider targetEnemy = GetClosestEnemy(hitEnemies);
            if (targetEnemy != null)
            {
                ExecutePhaseAttack(targetEnemy);
            }
        }
    }

    Collider GetClosestEnemy(Collider[] enemies)
    {
        Collider closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(enemy.transform.position, currentPos);
            if (distance < minDistance)
            {
                closest = enemy;
                minDistance = distance;
            }
        }
        return closest;
    }

    void ExecutePhaseAttack(Collider targetEnemy)
    {
        // Optional: Trigger animation
        if (animator != null)
            animator.SetTrigger("PhaseAttack");

        // Calculate position behind enemy
        Vector3 enemyPos = targetEnemy.transform.position;
        Vector3 directionFromEnemy = (transform.position - enemyPos).normalized;
        Vector3 newPosition = enemyPos - directionFromEnemy * phaseDistance;

        // Ensure new position is at same height as enemy (optional)
        newPosition.y = transform.position.y;

        // Phase through (instant movement)
        transform.position = newPosition;

        // Face the enemy after phasing
        transform.LookAt(enemyPos);

        // Destroy the enemy
        Destroy(targetEnemy.gameObject);

        // Update cooldown
        lastPhaseTime = Time.time;
    }

    // Visualize the phase range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, phaseRange);
    }
}