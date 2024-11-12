using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IvyMoon
{
    public class InventoryDisplay : MonoBehaviour
    {
        [SerializeField]
        public List<InventorySlot> uIItems = new List<InventorySlot>();

        public List<InventorySlot> hotbarSlots = new List<InventorySlot>(); // List to hold hotbar items
        public GameObject slotPrefab;
        public GameObject hotbarSlotPrefab;    // Prefab for hotbar slots
        public Transform slotGrid;
        public Transform hotbarGrid; // Reference for the hotbar container in UI
        public int numberOfSlots = 24;
        private int selectedHotbarSlot = 0; // Default to the first slot
        public int maxHotbarSlots = 11;
        public bool vendor;
        public bool TreasureChest;
        public bool player;
        public Inventory playerInventory; // Reference to the player's inventory

        private void Awake()
        {
            if (slotPrefab == null || slotGrid == null || hotbarSlotPrefab == null || hotbarGrid == null || playerInventory == null)
            {
                Debug.LogError("InventoryDisplay: One or more required components are not assigned.");
                return;
            }

            for (int i = 0; i < numberOfSlots; i++)
            {
                GameObject instance = Instantiate(slotPrefab, slotGrid);
                InventorySlot slot = instance.GetComponentInChildren<InventorySlot>();
                if (slot == null)
                {
                    Debug.LogError("InventorySlot component not found in slotPrefab.");
                    continue;
                }

                if (vendor)
                {
                    slot.vendor = true;
                }
                if (TreasureChest)
                {
                    slot.treasureChest = true;
                }
                if (player)
                {
                    slot.inventory = true;
                }
                uIItems.Add(slot);
            }
            InitializeHotbar();
            RefreshHotbar();
        }

        public void SetupSlot(int slot, InventoryItem item)
        {
            if (slot < 0 || slot >= uIItems.Count)
            {
                Debug.LogWarning("SetupSlot: Slot index out of range.");
                return;
            }
            uIItems[slot].Setup(item);
        }

        public void AddNewItem(InventoryItem item)
        {
            int slotIndex = uIItems.FindIndex(i => i.item == null);
            if (slotIndex != -1)
            {
                SetupSlot(slotIndex, item);
            }
        }

        public void RemoveItem(InventoryItem item)
        {
            int slotIndex = uIItems.FindIndex(i => i.item == item);
            if (slotIndex != -1)
            {
                SetupSlot(slotIndex, null);
            }
        }

        // Instantiate hotbar slots based on maxHotbarSlots
        public void InitializeHotbar()
        {
            // Clear existing hotbar slots to avoid duplicates
            foreach (var slot in hotbarSlots)
            {
                Destroy(slot.gameObject);
            }
            hotbarSlots.Clear();

            // Create exactly maxHotbarSlots slots
            for (int i = 0; i < maxHotbarSlots; i++)
            {
                GameObject instance = Instantiate(hotbarSlotPrefab, hotbarGrid);
                InventorySlot slot = instance.GetComponent<InventorySlot>();
                if (slot == null)
                {
                    Debug.LogError("InventorySlot component not found in hotbarSlotPrefab.");
                    continue;
                }

                if (i < playerInventory.characterItems.Count)
                {
                    // Assign an item to the slot if available
                    slot.Setup(playerInventory.characterItems[i]);
                }
                else
                {
                    // Leave the slot empty if no item is available
                    slot.Setup(null);
                }

                hotbarSlots.Add(slot);
            }
        }

        public void AddToHotbar(InventoryItem item, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= hotbarSlots.Count)
            {
                Debug.LogWarning("AddToHotbar: Slot index out of range.");
                return;
            }
            hotbarSlots[slotIndex].Setup(item); // Assign item to specific hotbar slot
        }

        public void RefreshHotbar()
        {
            // Iterate over existing hotbar slots and update each slot with the corresponding inventory item
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                // Check if there is a corresponding inventory item to display in this slot
                if (i < uIItems.Count)
                {
                    InventoryItem inventoryItem = uIItems[i].item;
                    hotbarSlots[i].Setup(inventoryItem); // Update the hotbar slot with the current item
                }
                else
                {
                    hotbarSlots[i].Setup(null); // Clear the slot if no item is present
                }
            }

            // Update selection highlighting to maintain only one selected slot
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                hotbarSlots[i].SetSelected(i == selectedHotbarSlot);
            }
        }

        public void SelectHotbarSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < hotbarSlots.Count)
            {
                selectedHotbarSlot = slotIndex;

                // Update selection highlighting
                for (int i = 0; i < hotbarSlots.Count; i++)
                {
                    hotbarSlots[i].SetSelected(i == selectedHotbarSlot);
                }
            }
        }

        public InventoryItem GetSelectedHotbarItem()
        {
            if (selectedHotbarSlot >= 0 && selectedHotbarSlot < hotbarSlots.Count)
            {
                return hotbarSlots[selectedHotbarSlot].item;
            }
            return null; // No item selected or slot is empty
        }

        private void OnEnable()
        {
            InventoryEvents.InventoryUpdated += RefreshHotbar;
        }

        private void OnDisable()
        {
            InventoryEvents.InventoryUpdated -= RefreshHotbar;
        }
    }
}
