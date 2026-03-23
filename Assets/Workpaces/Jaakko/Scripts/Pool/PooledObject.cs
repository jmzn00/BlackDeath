using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private PoolManager m_pool;
    private GameObject m_prefab;
    private bool m_isInitialized;

    public void Init(PoolManager pool, GameObject prefab) 
    {
        m_pool = pool;
        m_prefab = prefab;
        m_isInitialized = true;
    }
    public void Release() 
    {
        if (!m_isInitialized) 
        {
            Debug.LogWarning($"PooledObject on {gameObject.name} not Initialized");
            return;
        }
        m_pool.Release(gameObject, m_prefab);
    }
}
