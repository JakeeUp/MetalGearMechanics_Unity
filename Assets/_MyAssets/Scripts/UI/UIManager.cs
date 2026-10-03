using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // ============================
    // Serialized Fields
    // ============================

    public InventoryUI leftUI;
    public InventoryUI rightUI;
    public GameObject inventorySlotPrefab;

    // ============================
    // Singleton
    // ============================

    public static UIManager singleton;

    // ============================
    // State
    // ============================

    InventoryManager inventoryManager;

    // ============================
    // Lifecycle
    // ============================

    private void Awake()
    {
        singleton = this;
    }

    // ============================
    // Public API
    // ============================

    public void Init(InventoryManager inv)
    {
        if (inv == null)
        {
            Debug.LogError("InventoryManager is null during UIManager initialization.");
            return;
        }

        inventoryManager = inv;
    }

    public void CreateSlotsForItemList(List<Item> l)
    {
        if (l.Count == 0)
            return;

        leftUI.CreateSlotsForList(l, inventorySlotPrefab);
    }

    public bool isInInventory(Item item)
    {
        if (inventoryManager == null || item == null)
            return false;

        return inventoryManager.pickedUpItems.Contains(item);
    }

    public bool Tick(float vertical, float delta, bool isLeftActive, bool isRightActive)
    {
        if (isLeftActive)
        {
            if (!leftUI.invGameObject.activeInHierarchy)
            {
                leftUI.OpenOwnedSlots(this);
                leftUI.invGameObject.SetActive(true);
            }

            leftUI.Tick(vertical, delta, inventoryManager);
            return true;
        }
        else
        {
            if (leftUI.invGameObject.activeInHierarchy)
                leftUI.invGameObject.SetActive(false);
        }

        if (isRightActive)
        {
            if (!rightUI.invGameObject.activeInHierarchy)
            {
                rightUI.OpenOwnedSlots(this);
                rightUI.invGameObject.SetActive(true);
            }

            rightUI.Tick(vertical, delta, inventoryManager);
            return true;
        }
        else
        {
            if (rightUI.invGameObject.activeInHierarchy)
                rightUI.invGameObject.SetActive(false);
        }

        return false;
    }
}

[System.Serializable]
public class InventoryUI
{
    public RectTransform invGrid;
    public GameObject invGameObject;

    [System.NonSerialized]
    List<ItemSlot> createdItems = new List<ItemSlot>();
    [System.NonSerialized]
    List<ItemSlot> ownedItems = new List<ItemSlot>();
    ItemSlot currentObject;
    Vector2 targetYPosition;
    Vector2 startPosition;

    [System.NonSerialized] float t;
    [System.NonSerialized] float lastChange;
    [System.NonSerialized] GridLayoutGroup gridLayout;

    // Show only the slots for items the player has picked up; navigation cycles through these
    public void OpenOwnedSlots(UIManager uiManager)
    {
        ownedItems.Clear();

        for (int i = 0; i < createdItems.Count; i++)
        {
            bool owned = uiManager.isInInventory(createdItems[i].targetItem);
            createdItems[i].gameObject.SetActive(owned);
            if (owned)
                ownedItems.Add(createdItems[i]);
        }
    }

    public void CreateSlotsForList(List<Item> items, GameObject slotPrefab)
    {
        for (int i = 0; i < items.Count; i++)
            CreateInventorySlotForItem(items[i], slotPrefab);
    }

    public void CreateInventorySlotForItem(Item item, GameObject inventorySlotPrefab)
    {
        GameObject go = GameObject.Instantiate(inventorySlotPrefab);
        go.transform.SetParent(invGrid);
        go.SetActive(true);

        ItemSlot slot = go.GetComponentInChildren<ItemSlot>();
        slot.LoadItem(item);
        createdItems.Add(slot);
        t = 1;
    }

    bool notPressed;

    public void Tick(float vertical, float delta, InventoryManager inv)
    {
        if (ownedItems.Count == 0)
            return;

        if (Mathf.Abs(vertical) > 0.5f)
        {
            if (Time.realtimeSinceStartup - lastChange > 1 || !notPressed)
            {
                notPressed = true;
                lastChange = Time.realtimeSinceStartup;

                bool isDown = vertical < 0;
                int curIndex = ownedItems.IndexOf(currentObject);
                curIndex = isDown ? curIndex - 1 : curIndex + 1;

                if (curIndex < 0)
                    curIndex = ownedItems.Count - 1;
                if (curIndex > ownedItems.Count - 1)
                    curIndex = 0;

                currentObject = ownedItems[curIndex];
                Vector2 position = invGrid.localPosition;
                startPosition = position;
                if (gridLayout == null)
                    gridLayout = invGrid.GetComponent<GridLayoutGroup>();
                position.y = curIndex * gridLayout.cellSize.y;
                targetYPosition = position;
                t = 0;

                inv.LoadItem(currentObject.targetItem);
            }
        }
        else
        {
            notPressed = false;
        }

        t += delta * 2;
        if (t > 1)
            t = 1;

        Vector2 targetPosition = Vector2.Lerp(startPosition, targetYPosition, t);
        invGrid.localPosition = targetPosition;
    }
}
