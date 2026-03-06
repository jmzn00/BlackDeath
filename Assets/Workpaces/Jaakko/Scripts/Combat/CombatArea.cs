using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CombatPreferences 
{
    [HideInInspector] public CombatArea m_area;
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
    private bool m_areaCompleted;

    public void SetCompleted(bool value) 
    {
        m_areaCompleted = value;
    }
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
        CombatPreferences prefs = m_combatPreferences;
        prefs.m_area = this;
        m_combatManager.StartBattle(prefs);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (m_areaCompleted) return;

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

