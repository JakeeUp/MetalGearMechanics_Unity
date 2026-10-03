using UnityEngine;
using UnityEngine.UI;
using Jacob.Utilities;

public class ItemSlot : MonoBehaviour
{
    public Image img;
    public Item targetItem;

    public void LoadItem(Item item)
    {
        targetItem = item;

        if (targetItem.inventoryIcon == null)
            IconMaker.RequestIcon(targetItem, LoadIcon);
        else
            LoadIcon();
    }

    void LoadIcon()
    {
        img.sprite = targetItem.inventoryIcon;
    }
}
