using UnityEngine;
using Unity.Netcode;
using System;

namespace IvyMoon {
    [Serializable]
    public class InventoryItem : INetworkSerializable {
        public string itemName = "New Item";
        public string itemDescription = "New description";
        public Sprite itemIcon = null;
        public GameObject itemObject;
        public bool isUnique = false;
        public Vector3 handPositionOffset;
        public Vector3 handRotationOffset;
        public int damageValue;
        public int itemAnimationId;

        public InventoryItem() { }

        public InventoryItem(InventoryItem item) {
            itemName = item.itemName;
            itemDescription = item.itemDescription;
            itemIcon = item.itemIcon;
            itemObject = item.itemObject;
            isUnique = item.isUnique;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref itemName);
            serializer.SerializeValue(ref itemDescription);
            serializer.SerializeValue(ref isUnique);

            // We will not serialize itemIcon or itemObject directly
            // Instead, rely on itemName to identify the item
        }
    }
}
