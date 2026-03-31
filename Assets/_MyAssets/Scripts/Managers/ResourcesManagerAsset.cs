using UnityEngine;

namespace Jacob.ManagerAssets
{
    [CreateAssetMenu]
    public class ResourcesManagerAsset : ScriptableObject
    {
        public Item[] allItems;

        public Item[] GetAllItems()
        {
            return allItems;
        }
    }
}
