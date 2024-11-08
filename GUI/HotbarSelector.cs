using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IvyMoon {
    public class HotbarSelector : MonoBehaviour {
        public InventoryDisplay inventoryDisplay; // Reference to the InventoryDisplay
        private int selectedHotbarSlot = 0;

        private void Update() {
            // Check for number key presses (1-8) to select hotbar slots
            if (Input.GetKeyDown(KeyCode.Alpha1)) { SelectHotbarSlot(0); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { SelectHotbarSlot(1); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { SelectHotbarSlot(2); }
            if (Input.GetKeyDown(KeyCode.Alpha4)) { SelectHotbarSlot(3); }
            if (Input.GetKeyDown(KeyCode.Alpha5)) { SelectHotbarSlot(4); }
            if (Input.GetKeyDown(KeyCode.Alpha6)) { SelectHotbarSlot(5); }
            if (Input.GetKeyDown(KeyCode.Alpha7)) { SelectHotbarSlot(6); }
            if (Input.GetKeyDown(KeyCode.Alpha8)) { SelectHotbarSlot(7); }
        }

        public void SelectHotbarSlot(int slotIndex) {
            if (slotIndex >= 0 && slotIndex < inventoryDisplay.hotbarSlots.Count) {
                selectedHotbarSlot = slotIndex;
                inventoryDisplay.SelectHotbarSlot(slotIndex); // Call method in InventoryDisplay to update selection
            }
        }

        public InventoryItem GetSelectedHotbarItem() {
            return inventoryDisplay.GetSelectedHotbarItem(); // Get the currently selected item from the InventoryDisplay
     
        }

    }
}
