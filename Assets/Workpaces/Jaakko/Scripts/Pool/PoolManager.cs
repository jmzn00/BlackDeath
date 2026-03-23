using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class SpawnPreferences 
{
    public Vector3 position;
    public Quaternion rotation;
    public Transform parent;
}
public class PoolManager : IManager
{
    private GameManager m_game;

    private Dictionary<GameObject, Queue<GameObject>> m_pools;
    public PoolManager(GameManager game) 
    {
        m_game = game;
    }
    public void OnManagersInitialzied() { }
    public bool Init() 
    {
        m_pools = new();
        return true;
    }
    public bool Dispose() 
    {
        m_pools.Clear();
        return true;
    }
    public void Update(float dt) { }
    public void Release(GameObject obj, GameObject prefab) 
    {
        if (obj == null || prefab == null) 
        {
            Debug.LogWarning($"Trying to release null obj or prefab");
            return;
        }
        if (!obj.activeSelf) 
        {
            Debug.LogWarning($"Object {obj.name} already released.");
            return;
        }
        if (!m_pools.TryGetValue(prefab, out var pool)) 
        {
            pool = new Queue<GameObject>();
            m_pools[prefab] = pool;
        }

        obj.SetActive(false);

        pool.Enqueue(obj);
    }
    public GameObject Spawn(GameObject prefab, SpawnPreferences prefs) 
    {
        if (!m_pools.TryGetValue(prefab, out var pool)) 
        {
            pool = new Queue<GameObject>();
            m_pools[prefab] = pool;
        }
        GameObject obj;

        if (pool.Count > 0) 
        {
            obj = pool.Dequeue();
            obj.SetActive(true);
        }
        else 
        {
            obj = GameObject.Instantiate(prefab);
            var pooled = obj.GetComponent<PooledObject>();
            if (pooled == null) 
            {
                obj.AddComponent<PooledObject>().Init(this, prefab);
            }
            pooled.Init(this, prefab);
        }
        obj.transform.SetPositionAndRotation(prefs.position
            , prefs.rotation);
        obj.transform.parent = prefs.parent;

        return obj;
    }
    public T Spawn<T>(T prefab, SpawnPreferences prefs)
        where T : Component 
    {
        var go = Spawn(prefab.gameObject, prefs);
        return go.GetComponent<T>();    
    }
}
