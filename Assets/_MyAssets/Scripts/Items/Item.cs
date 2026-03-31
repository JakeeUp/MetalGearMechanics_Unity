using UnityEngine;

public abstract class Item : ScriptableObject, Jacob.Utilities.IIcon
{
    public GameObject prefab;
    public Sprite inventoryIcon;
    public Vector2 iconPivotPosition;

    public GameObject GetObjectForIcon()
    {
        return prefab;
    }

    public Vector2 GetPivotPosition()
    {
        return iconPivotPosition;
    }

    public void IconCreatedCallback(Sprite sprite)
    {
        inventoryIcon = sprite;
    }
}
