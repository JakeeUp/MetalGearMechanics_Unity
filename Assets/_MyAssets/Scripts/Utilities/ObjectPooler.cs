using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectPooler : ScriptableObject
{
    public Pool[] pools;
    Dictionary<string, Pool> poolDict = new Dictionary<string, Pool>();

    [System.NonSerialized]
    GameObject parentObject;

    public void Init()
    {
        poolDict.Clear();

        foreach (Pool p in pools)
        {
            p.Clear();
            poolDict.Add(p.poolId, p);
        }

        parentObject = new GameObject("pool parent");
    }

    public GameObject GetObject(string id)
    {
        // The pool parent is destroyed on scene load while this asset survives, so rebuild
        if (parentObject == null)
            Init();

        if (!poolDict.TryGetValue(id, out Pool value))
            return null;

        GameObject go = value.GetObject(parentObject.transform);
        go.SetActive(false);
        return go;
    }
}

[System.Serializable]
public class Pool
{
    public string poolId;
    public GameObject prefab;
    public int budget = 5;

    [System.NonSerialized] List<GameObject> createdObjects = new List<GameObject>();
    [System.NonSerialized] int index;

    public void Clear()
    {
        createdObjects.Clear();
        index = 0;
    }

    public GameObject GetObject(Transform parent)
    {
        if (createdObjects.Count < budget)
        {
            GameObject go = GameObject.Instantiate(prefab, parent);
            createdObjects.Add(go);
            return go;
        }

        index = (index + 1) % createdObjects.Count;
        return createdObjects[index];
    }
}
