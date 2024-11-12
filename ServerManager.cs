using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace IvyMoon{
public class ServerManager : MonoBehaviour
{
    public Button startServerButton;
    public Button stopServerButton;
    public GameObject playerPrefab; // Assign your player prefab here if not using LocalClient.PlayerObject

    private void Start()
    {
        startServerButton.onClick.AddListener(StartServer);
        stopServerButton.onClick.AddListener(StopServer);

        // Register event for client connections
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    private void OnDestroy()
    {
        // Unregister the event when the object is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    private void StartServer()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log("Server started");

            // If this instance is also a host, spawn the local player
            if (NetworkManager.Singleton.IsHost)
            {
                SpawnPlayer(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    private void StopServer()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Server stopped");
        }
    }

    private void HandleClientConnected(ulong clientId) {
        foreach (var player in FindObjectsOfType<PlayerController>()) {
            if (player.IsOwner && player.IsHost) {
                // Trigger a ClientRpc for all players to synchronize the host’s item
                player.ForceItemDisplayClientRpc(player.networkSelectedItem.Value.ToString());
            }
        }
    }


    private void SpawnPlayer(ulong clientId)
    {
        // Spawn the player using the prefab in the Spawnable Prefabs list or the assigned playerPrefab
        GameObject playerInstance;

        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab);
        }
        else
        {
            Debug.LogError("Player prefab is not assigned in ServerManager or NetworkManager!");
            return;
        }

        var networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        Debug.Log($"Spawned player for client {clientId}");
    }
}
}