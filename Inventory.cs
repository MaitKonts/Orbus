using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace IvyMoon
{
    public class Inventory : NetworkBehaviour
    {
        public List<InventoryItem> characterItems = new List<InventoryItem>();
        public InventoryItemList database;
        public InventoryDisplay inventoryDisplay;

        private void Start()
        {
            if (IsServer)
            {
                // Initialize inventory on the server
                InitializeInventory();
            }
        }

        private void InitializeInventory()
        {
            GiveItem("Axe");
        }

        [ServerRpc(RequireOwnership = false)]
        public void GiveItemServerRpc(string itemName, ServerRpcParams rpcParams = default)
        {
            InventoryItem itemToAdd = database.GetItem(itemName);
            if (itemToAdd != null)
            {
                characterItems.Add(itemToAdd);

                // Notify the client to update their inventory display
                UpdateInventoryClientRpc(itemName);
            }
        }

        [ClientRpc]
        private void UpdateInventoryClientRpc(string itemName)
        {
            if (!IsOwner) return;
            InventoryItem itemToAdd = database.GetItem(itemName);
            if (itemToAdd != null)
            {
                inventoryDisplay.AddNewItem(itemToAdd);
                inventoryDisplay.RefreshHotbar();
            }
        }

        public void GiveItem(string itemName)
        {
            if (IsServer)
            {
                // Directly add item on the server
                GiveItemServerRpc(itemName);
            }
            else
            {
                // Request the server to add the item
                GiveItemServerRpc(itemName);
            }
        }

        public void AddItem(string itemName)
        {
            if (IsServer)
            {
                InventoryItem itemToAdd = database.GetItem(itemName);
                if (itemToAdd != null)
                {
                    characterItems.Add(itemToAdd);
                    inventoryDisplay.AddNewItem(itemToAdd);
                    inventoryDisplay.RefreshHotbar();
                }
            }
        }

        public void AddItems(List<InventoryItem> items)
        {
            foreach (InventoryItem item in items)
            {
                AddItem(item.itemName);
            }
        }

        public InventoryItem CheckThisItem(string itemName)
        {
            return characterItems.Find(InventoryItem => InventoryItem.itemName == itemName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RemoveItemServerRpc(string itemName, ServerRpcParams rpcParams = default)
        {
            InventoryItem item = CheckThisItem(itemName);
            if (item != null)
            {
                characterItems.Remove(item);
                Debug.Log("Item removed: " + item.itemName);

                // Notify the client to update their inventory display
                RemoveItemClientRpc(itemName);
            }
        }

        [ClientRpc]
        private void RemoveItemClientRpc(string itemName)
        {
            InventoryItem item = CheckThisItem(itemName);
            if (item != null)
            {
                characterItems.Remove(item);
                inventoryDisplay.RemoveItem(item);
            }
        }

        public void RemoveItem(string itemName)
        {
            if (IsServer)
            {
                RemoveItemServerRpc(itemName);
            }
            else
            {
                RemoveItemServerRpc(itemName);
            }
        }
    }
}
