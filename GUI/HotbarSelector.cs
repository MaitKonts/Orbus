using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IvyMoon
{
    public class HotbarSelector : MonoBehaviour
    {
        public InventoryDisplay inventoryDisplay; // Reference to the InventoryDisplay
        private int selectedHotbarSlot = 0;
        private PlayerController playerController;

        private void Awake()
        {
            // Cache the PlayerController reference
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController not found in the scene.");
            }

            if (inventoryDisplay == null)
            {
                Debug.LogError("InventoryDisplay is not assigned.");
            }
        }

        private void Update()
        {
            // Check for number key presses (1-8) to select hotbar slots
            for (int i = 0; i < 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectHotbarSlot(i);
                }
            }
        }

        public void SelectHotbarSlot(int slotIndex)
        {
            if (inventoryDisplay == null)
            {
                Debug.LogWarning("InventoryDisplay is not assigned.");
                return;
            }

            if (slotIndex >= 0 && slotIndex < inventoryDisplay.hotbarSlots.Count)
            {
                selectedHotbarSlot = slotIndex;
                inventoryDisplay.SelectHotbarSlot(slotIndex);

                // Get the selected item and pass the itemName to PlayerController
                InventoryItem selectedItem = GetSelectedHotbarItem();
                if (playerController != null)
                {
                    playerController.SetSelectedHotbarItem(selectedItem != null ? selectedItem.itemName : string.Empty);
                }
            }
        }

        public InventoryItem GetSelectedHotbarItem()
        {
            if (inventoryDisplay == null)
            {
                Debug.LogWarning("InventoryDisplay is not assigned.");
                return null;
            }

            return inventoryDisplay.GetSelectedHotbarItem(); // Get the currently selected item from the InventoryDisplay
        }
    }
}
