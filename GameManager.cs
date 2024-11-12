using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        // Only the server should handle player spawning
        if (IsServer)
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
            }
            else
            {
                Debug.LogError("NetworkManager.Singleton is null. Cannot subscribe to OnClientConnectedCallback.");
            }
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned.");
            return;
        }

        // Spawn a player object for the new client
        var spawnPosition = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
        var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        // Attach the network identity to the new player object and assign it to the client
        var networkObject = playerInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId);
        }
        else
        {
            Debug.LogError("Player prefab does not have a NetworkObject component.");
        }
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe to avoid memory leaks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
        }
    }
}
