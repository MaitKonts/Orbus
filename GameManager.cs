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
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Spawn a player object for the new client
        var spawnPosition = new Vector3(Random.Range(-5, 5), 1, Random.Range(-5, 5));
        var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        // Attach the network identity to the new player object and assign it to the client
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
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
