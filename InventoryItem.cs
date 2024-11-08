using UnityEngine;
using Unity.Netcode;
using System;
namespace IvyMoon{
[Serializable]
public class InventoryItem : INetworkSerializable
{
    public string itemName = "New Item";
    public string itemDescription = "New description";
    public Sprite itemIcon = null; // This will need a different approach for serialization
    public GameObject itemObject;  // This will need a different approach for serialization
    public bool isUnique = false;

    public int damageValue; // Damage value for attack actions
    public int itemAnimationId; // ID for selecting specific animations

    // Constructor to create a copy of an InventoryItem
    public InventoryItem(InventoryItem item)
    {
        itemName = item.itemName;
        itemDescription = item.itemDescription;
        itemIcon = item.itemIcon;
        itemObject = item.itemObject;
        isUnique = item.isUnique;
    }

    // Default constructor
    public InventoryItem() { }

    // Implement the INetworkSerializable interface
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemName);
        serializer.SerializeValue(ref itemDescription);
        serializer.SerializeValue(ref isUnique);

        // For itemIcon and itemObject, you might need to serialize a string identifier or path
        // Example: Serialize a string path or ID instead of the actual object
        string itemIconPath = itemIcon != null ? itemIcon.name : string.Empty;
        serializer.SerializeValue(ref itemIconPath);

        string itemObjectPath = itemObject != null ? itemObject.name : string.Empty;
        serializer.SerializeValue(ref itemObjectPath);

        // Note: You will need to handle the conversion from path/ID back to the actual Sprite/GameObject on the client side
    }
}
}