using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CombatPreferences 
{
    public Actor[] m_enemies;
    public Transform[] m_enemySpawnPoints;

    public Transform[] m_partySpawnPoints;    
}
[RequireComponent(typeof(BoxCollider))]
public class CombatArea : MonoBehaviour
{
    [SerializeField] private CombatPreferences m_combatPreferences;
    private CombatManager m_combatManager;
    private BoxCollider m_boxCollider;

    public void Initialize(CombatManager combatManager) 
    {
        m_combatManager = combatManager;        
    }
    public void StartBattle() 
    {
        if (m_combatPreferences.m_enemies[0] == null) 
        {
            Debug.LogWarning("CombatArea Enemy Prefab NULL");
            return;
        }
        m_combatManager.StartBattle(m_combatPreferences);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            StartBattle();
        }
    }
    private void OnDrawGizmos()
    {
        if (m_boxCollider == null) 
        {
            m_boxCollider = GetComponent<BoxCollider>();
        }
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, m_boxCollider.size);
    }
}

