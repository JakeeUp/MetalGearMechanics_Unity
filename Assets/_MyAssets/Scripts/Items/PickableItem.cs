using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public Item targetItem;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("GameController"))
            return;

        Controller c = other.GetComponentInParent<Controller>();
        if (c != null && c.inventoryManager != null)
        {
            c.inventoryManager.PickUpItem(targetItem);
            gameObject.SetActive(false);
        }
    }
}
