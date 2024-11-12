using System.Collections;
using UnityEngine;
using IvyMoon;
using Unity.Netcode;

public class ItemsToLoad : NetworkBehaviour
{
    private Inventory charInventory; // Our local character's inventory
    public string[] startingItems; // The names of all the items we start with

    void Awake()
    {
        charInventory = GetComponent<Inventory>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializeStartingItems();
        }
    }

    private void InitializeStartingItems()
    {
        if (charInventory == null)
        {
            throw new System.ArgumentException("An Inventory script is required for this to run");
        }

        for (int i = 0; i < startingItems.Length; i++)
        {
            charInventory.GiveItem(startingItems[i]);
        }

        // Notify the client to update their inventory display
        UpdateClientInventoryClientRpc();
    }

    [ClientRpc]
    private void UpdateClientInventoryClientRpc()
    {
        if (charInventory != null)
        {
            for (int i = 0; i < startingItems.Length; i++)
            {
                charInventory.inventoryDisplay.AddNewItem(charInventory.database.GetItem(startingItems[i]));
            }
        }
    }
}
