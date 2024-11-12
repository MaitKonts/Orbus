using System.Collections;
using UnityEngine;
using IvyMoon;
using Unity.Netcode;

public class SceneItem : NetworkBehaviour
{
    [SerializeField]
    private string itemName;  // CASE SENSITIVE - write in the name of the item that matches the name in the InventoryList so we find the right item.
    private bool hasRun;
    private GameObject inventoryFullText;

    private void Awake()
    {
        inventoryFullText = GameObject.Find("InventoryFull"); // Find the game object and make a local reference to it
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasRun)
        {
            // Request the server to handle the item pickup
            HandleItemPickupServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId);
            hasRun = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleItemPickupServerRpc(ulong playerNetworkObjectId, ServerRpcParams rpcParams = default)
    {
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        Inventory playerInventory = playerNetworkObject.GetComponent<Inventory>();

        if (playerInventory != null)
        {
            if (playerInventory.characterItems.Count < playerInventory.inventoryDisplay.numberOfSlots)
            {
                playerInventory.GiveItem(itemName);
                Destroy(gameObject); // Destroy the item in the game world on the server
            }
            else
            {
                // Notify the client that the inventory is full
                NotifyInventoryFullClientRpc(playerNetworkObjectId);
            }
        }
    }

    [ClientRpc]
    private void NotifyInventoryFullClientRpc(ulong playerNetworkObjectId)
    {
        NetworkObject playerNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        if (playerNetworkObject.IsLocalPlayer)
        {
            inventoryFullText.SetActive(true);
        }
    }
}
