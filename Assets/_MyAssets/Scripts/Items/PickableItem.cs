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
            Debug.Log($"[Pickup] {targetItem.name} from '{name}' (root '{transform.root.name}', path '{GetPath(transform)}') " +
                      $"at {transform.position} | player '{other.name}' at {other.transform.position} | frame {Time.frameCount}, t={Time.time:F2}s", this);
            c.inventoryManager.PickUpItem(targetItem);
            gameObject.SetActive(false);
        }
    }

    static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
