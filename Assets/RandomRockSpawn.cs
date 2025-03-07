using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The prefab to spawn")]
    public GameObject objectToSpawn;

    [Header("Position Settings")]
    [Tooltip("Minimum X position for spawning")]
    public float minX = -10f;

    [Tooltip("Maximum X position for spawning")]
    public float maxX = 10f;

    [Tooltip("Y position for spawning (height)")]
    public float spawnHeight = 5f;

    [Tooltip("Minimum Z position for spawning")]
    public float minZ = -10f;

    [Tooltip("Maximum Z position for spawning")]
    public float maxZ = 10f;

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
            Debug.LogError("No spawn object assigned to RandomSpawner!");
            return;
        }

        // Make sure the object to spawn has a Rigidbody
        if (objectToSpawn.GetComponent<Rigidbody>() == null)
        {
            Debug.LogWarning("The object to spawn doesn't have a Rigidbody component. It won't fall unless affected by gravity.");
        }

        if (continuousSpawning)
        {
            // Start the continuous spawning coroutine
            StartCoroutine(SpawnRoutine());
        }
        else
        {
            // Just spawn once
            SpawnObject();
        }
    }

    // Coroutine for continuous spawning
    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Spawn a single object at random X and Z positions
    private void SpawnObject()
    {
        // Calculate random X and Z positions within the specified ranges
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        // Create the new position vector with random X and Z but fixed Y
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

        // Instantiate the object at the calculated position
        Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
    }

    // Public method to spawn an object (can be called from other scripts or events)
    public void SpawnNow()
    {
        SpawnObject();
    }
}