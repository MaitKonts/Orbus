using System;
using UnityEngine;
using IvyMoon;

public class InventoryEvents : MonoBehaviour
{
    // Define static events for inventory actions
    public static event Action<InventoryItem> ItemAddedToInventory;
    public static event Action<string> ScrollInfoActivated;
    public static event Action ScrollInfoDeactivated;
    public static event Action<InventorySlot> ClickActivated;
    public static event Action ClickDeactivated;
    public static event Action<InventoryItem> ItemRemovedFromInventory;
    public static event Action InventoryChangedEvent = delegate { };
    public InventoryDisplay inventoryDisplay;
    public static event Action InventoryUpdated = delegate { };

    // Method to trigger InventoryUpdated
    public static void OnInventoryUpdated()
    {
        Debug.Log("Inventory updated event triggered.");
        InventoryUpdated?.Invoke();
    }

    public static void OnItemAddedToInventory(InventoryItem item)
    {
        ItemAddedToInventory?.Invoke(item);
    }

    public static void OnScrollInfoActivated(string text)
    {
        ScrollInfoActivated?.Invoke(text);
    }

    public static void OnScrollInfoDeactivated()
    {
        ScrollInfoDeactivated?.Invoke();
    }

    public static void OnClickActivated(InventorySlot slot)
    {
        ClickActivated?.Invoke(slot);
    }

    public static void OnClickDeactivated()
    {
        ClickDeactivated?.Invoke();
    }

    public static void OnItemRemovedFromInventory(InventoryItem item)
    {
        ItemRemovedFromInventory?.Invoke(item);
    }

    public static void OnInventoryChanged()
    {
        InventoryChangedEvent?.Invoke();
    }
}
