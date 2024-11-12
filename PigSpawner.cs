using Unity.Netcode;
using UnityEngine;

public class PigSpawner : NetworkBehaviour
{
    public GameObject pigPrefab;          // Reference to the pig prefab
    public Vector3 spawnLocation;         // Coordinates where the pig will spawn
    public float spawnInterval = 60f;     // Time interval between spawns, in seconds

    private float spawnTimer = 0f;        // Tracks time since last spawn

    private void Start()
    {
        // Only the server should handle spawning logic
        if (IsServer)
        {
            spawnTimer = spawnInterval; // Set initial timer to interval
        }
    }

    private void Update()
    {
        // Only run spawn logic on the server
        if (!IsServer) return;

        // Countdown the spawn timer
        spawnTimer -= Time.deltaTime;

        // Check if the timer has expired
        if (spawnTimer <= 0f)
        {
            SpawnPig();
            spawnTimer = spawnInterval; // Reset timer to the interval after spawn
        }
    }

    private void SpawnPig()
    {
        Debug.Log("Spawning pig at: " + spawnLocation); // Log spawn event

        // Instantiate and spawn the pig at the specified location
        GameObject pigInstance = Instantiate(pigPrefab, spawnLocation, Quaternion.identity);
        pigInstance.GetComponent<NetworkObject>().Spawn();
    }
}
