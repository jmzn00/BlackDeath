using UnityEngine;

public class HealthBarSpawner : MonoBehaviour
{
    [SerializeField] private Transform m_anchor;
    [SerializeField] private ActorHealthBar m_prefab;

    private void Start()
    {
        if (m_anchor == null || m_prefab == null) return;
        Instantiate(m_prefab, m_anchor.position, m_anchor.rotation, m_anchor);
    }
}
