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
            poolDict.Add(p.poolId, p);

        parentObject = new GameObject("pool parent");
    }

    public GameObject GetObject(string id)
    {
        if (!poolDict.TryGetValue(id, out Pool value))
            return null;

        GameObject go = value.GetObject();
        go.SetActive(false);
        go.transform.parent = parentObject.transform;
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

    public GameObject GetObject()
    {
        if (createdObjects.Count < budget)
        {
            GameObject go = GameObject.Instantiate(prefab);
            createdObjects.Add(go);
            return go;
        }

        index = (index + 1) % createdObjects.Count;
        return createdObjects[index];
    }
}
