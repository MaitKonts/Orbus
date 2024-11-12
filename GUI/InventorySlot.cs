using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.Netcode;
using IvyMoon;

public class InventorySlot : NetworkBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image spriteImage;            // Our image inside the slot
    public InventoryItem item;            // Our item inside the slot
    public InventorySlot selectedItem;    // Reference for our game object "SelectedItem" - so we can change it -
    private TextMeshProUGUI itemNameText; // Ref to our slot text
    public bool dropScreen;
    public GameObject dropSpawner;
    public GameObject selectionHighlight;

    [HideInInspector]
    public bool vendor = false;
    [HideInInspector]
    public bool treasureChest = false;
    [HideInInspector]
    public bool inventory = false;
    [HideInInspector]
    public Inventory player;
    [HideInInspector]
    public Inventory tChest;
    public InventoryEvents InventoryEvents;

    void Awake()
    {
        GameObject chestObj = GameObject.Find("TreasureChest");
        if (chestObj != null)
            tChest = chestObj.GetComponent<Inventory>();

        GameObject playerObj = GameObject.Find("FPSController");
        if (playerObj != null)
            player = playerObj.GetComponent<Inventory>();

        GameObject selectedItemObj = GameObject.Find("SelectedItem");
        if (selectedItemObj != null)
            selectedItem = selectedItemObj.GetComponent<InventorySlot>();

        spriteImage = GetComponent<Image>();
        if (spriteImage == null)
        {
            Debug.LogError("Image component not found on InventorySlot.");
        }
        Setup(null);
        itemNameText = GetComponentInChildren<TextMeshProUGUI>();
        if (itemNameText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found in children.");
        }
    }

    public void Setup(InventoryItem item)
    {
        this.item = item;

        if (this.item != null)
        {
            spriteImage.color = Color.white;
            spriteImage.sprite = this.item.itemIcon;

            if (itemNameText != null)
            {
                itemNameText.text = item.itemName;
            }
        }
        else
        {
            spriteImage.color = Color.clear;
            if (itemNameText != null)
            {
                itemNameText.text = null;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsOwner) return; // Ensure only the owner can interact

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick();
        }
    }

    private void HandleRightClick()
    {
        if (vendor)
        {
            if (this.item != null)
            {
                player.GiveItem(this.item.itemName);
                Debug.Log("Gave to player");
                if (this.item.isUnique)
                {
                    Setup(null);
                    InventoryEvents.OnScrollInfoDeactivated();
                }
            }
        }
        else if (tChest != null && tChest.GetComponent<Inventory>().inventoryDisplay.isActiveAndEnabled)
        {
            if (this.item != null)
            {
                if (inventory)
                {
                    tChest.GiveItem(this.item.itemName);
                    Debug.Log("Gave to chest");
                    Setup(null);
                    InventoryEvents.OnScrollInfoDeactivated();
                }
                else if (treasureChest)
                {
                    player.GiveItem(this.item.itemName);
                    Debug.Log("Gave to player");
                    Setup(null);
                    InventoryEvents.OnScrollInfoDeactivated();
                }
            }
        }
    }

    private void HandleLeftClick()
    {
        if (dropScreen)
        {
            // Handle drop logic here, if applicable
        }
        else
        {
            if (vendor) return; // Exit if vendor logic applies

            if (this.item != null)
            {
                if (selectedItem.item != null)
                {
                    InventoryItem clone = new InventoryItem(selectedItem.item);
                    selectedItem.Setup(this.item);
                    Setup(clone);
                }
                else
                {
                    selectedItem.Setup(this.item);
                    Setup(null);
                }
            }
            else if (selectedItem.item != null)
            {
                Setup(selectedItem.item);
                selectedItem.Setup(null);
            }

            // Call the inventory update event after the item is moved
            InventoryEvents.OnInventoryUpdated();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!dropScreen && item != null && item.itemDescription != null)
        {
            InventoryEvents.OnScrollInfoActivated(item.itemDescription);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!dropScreen)
        {
            InventoryEvents.OnScrollInfoDeactivated();
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(isSelected);
        }
    }
}
