using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabSpawner1 : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private int maxSpawnCount = 10;
    [SerializeField] private bool randomizeSpawnPoints = true;

    [Header("Optional")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool limitTotalSpawns = false;

    private int currentSpawnCount = 0;
    private bool isSpawning = false;

    private void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    private IEnumerator SpawnRoutine()
    {
        while (isSpawning && (!limitTotalSpawns || currentSpawnCount < maxSpawnCount))
        {
            SpawnPrefab();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnPrefab()
    {
        if (prefabToSpawn == null || spawnPoints.Length == 0)
        {
            Debug.LogError("PrefabSpawner: Missing prefab or spawn points!");
            return;
        }

        // Select spawn point
        Transform selectedSpawnPoint;

        if (randomizeSpawnPoints)
        {
            selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
        else
        {
            selectedSpawnPoint = spawnPoints[currentSpawnCount % spawnPoints.Length];
        }

        // Spawn the prefab
        GameObject spawnedObject = Instantiate(
            prefabToSpawn,
            selectedSpawnPoint.position,
            selectedSpawnPoint.rotation
        );

        // Increment counter
        currentSpawnCount++;

        // Optional: You can add event notification here
        // OnPrefabSpawned?.Invoke(spawnedObject);
    }

    // Public methods for external control
    public void SpawnSinglePrefab()
    {
        SpawnPrefab();
    }

    public void ResetSpawnCount()
    {
        currentSpawnCount = 0;
    }
}
