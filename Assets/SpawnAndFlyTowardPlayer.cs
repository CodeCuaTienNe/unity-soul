using System.Collections;
using UnityEngine;

public class SpawnAndFlyTowardPlayer : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The prefab to spawn")]
    public GameObject objectToSpawn;

    [Tooltip("The object to spawn from (leave empty to use this GameObject)")]
    public Transform spawnPoint;

    [Tooltip("The player's transform that objects will target")]
    public Transform playerTransform;

    [Header("Movement Settings")]
    [Tooltip("Speed at which objects move toward the player")]
    public float flySpeed = 5f;

    [Tooltip("Whether to use homing behavior (continuously update direction)")]
    public bool homingBehavior = false;

    [Tooltip("Random offset range for spawn position")]
    public Vector3 spawnPositionRandomOffset = new Vector3(1f, 1f, 1f);

    [Header("Timing Settings")]
    [Tooltip("Time between spawns in seconds")]
    public float spawnInterval = 2f;

    [Tooltip("Whether to spawn continuously or just once")]
    public bool continuousSpawning = true;

    private void Start()
    {
        // Validate parameters
        if (objectToSpawn == null)
        {
            Debug.LogError("No spawn object assigned to ObjectSpawnerTargetingPlayer!");
            return;
        }

        if (playerTransform == null)
        {
            // Try to find the player automatically
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("Player found automatically. Using " + player.name + " as target.");
            }
            else
            {
                Debug.LogError("No player transform assigned and no GameObject with tag 'Player' found!");
                return;
            }
        }

        // If no spawn point assigned, use this object's transform
        if (spawnPoint == null)
        {
            spawnPoint = this.transform;
        }

        if (continuousSpawning)
        {
            // Start the continuous spawning coroutine
            StartCoroutine((IEnumerator)SpawnRoutine());
        }
        else
        {
            // Just spawn once
            SpawnObject();
        }
    }

    // Coroutine for continuous spawning
    private IEnumerable SpawnRoutine()
    {
        while (true)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Spawn a single object at the spawn point's position
    private void SpawnObject()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("No player transform to target! Skipping spawn.");
            return;
        }

        // Calculate random offset for spawn position
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnPositionRandomOffset.x, spawnPositionRandomOffset.x),
            Random.Range(-spawnPositionRandomOffset.y, spawnPositionRandomOffset.y),
            Random.Range(-spawnPositionRandomOffset.z, spawnPositionRandomOffset.z)
        );

        // Create the spawn position with random offset
        Vector3 spawnPosition = spawnPoint.position + randomOffset;

        // Calculate initial direction to player
        Vector3 directionToPlayer = (playerTransform.position - spawnPosition).normalized;

        // Instantiate the object at the spawn position
        GameObject newObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.LookRotation(directionToPlayer));

        // Add the flying behavior component to the spawned object
        FlyTowardsPlayer flyScript = newObject.AddComponent<FlyTowardsPlayer>();
        flyScript.playerTransform = playerTransform;
        flyScript.flySpeed = flySpeed;
        flyScript.homingBehavior = homingBehavior;
    }

    // Public method to spawn an object (can be called from other scripts or events)
    public void SpawnNow()
    {
        SpawnObject();
    }
}

public class FlyTowardsPlayer : MonoBehaviour
{
    [HideInInspector]
    public Transform playerTransform;

    [HideInInspector]
    public float flySpeed = 5f;

    [HideInInspector]
    public bool homingBehavior = false;

    private Vector3 flyDirection;

    private void Start()
    {
        if (playerTransform == null)
        {
            // Try to find the player if not set
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("No player transform set for FlyTowardsPlayer script!");
                Destroy(this);
                return;
            }
        }

        // Set initial direction if not using homing behavior
        if (!homingBehavior)
        {
            flyDirection = (playerTransform.position - transform.position).normalized;
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        // Update direction if using homing behavior
        if (homingBehavior)
        {
            flyDirection = (playerTransform.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(flyDirection);
        }

        // Move towards the target
        transform.position += flyDirection * flySpeed * Time.deltaTime;
    }
}