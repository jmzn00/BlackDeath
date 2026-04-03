using System.Collections.Generic;
using UnityEngine;
using static InputSystem_Actions;

[System.Serializable]
public class CombatPreferences 
{
    [HideInInspector] public CombatArea m_area;
    public Actor[] m_enemies;

    public Transform[] m_enemySpawnPoints;
    public Transform[] m_partySpawnPoints;

    public Transform m_partyActionPoint;
    public Transform m_enemyActionPoint;
}
[RequireComponent(typeof(BoxCollider))]
public class CombatArea : MonoBehaviour
{
    [SerializeField] private CombatPreferences m_combatPreferences;
    public CombatPreferences Preferences => m_combatPreferences;
    private CombatManager m_combatManager;
    private DialogueManager m_dialogueManger;
    private ActorManager m_actorManager;
    private BoxCollider m_boxCollider;
    [SerializeField] private bool m_areaCompleted;
    private bool m_started;

    [Header("Flags")]
    [SerializeField] private string m_conditionFlag;
    [SerializeField] private string m_setFlag;

    private List<CombatActor> m_enemies;

    public void Initialize(GameManager game) 
    {
        m_combatManager = game.Resolve<CombatManager>();
        m_actorManager = game.Resolve<ActorManager>();
        m_dialogueManger = game.Resolve<DialogueManager>();

        m_started = false;  

        CombatEvents.OnCombatEnded += AreaFinished;

        if (!m_areaCompleted)
            SpawnEnemies();
    }
    private void SpawnEnemies() 
    {
        m_enemies = new();

        CombatPreferences prefs = m_combatPreferences;

        int enemyCount = prefs.m_enemies.Length;
        int spawnPoints = prefs.m_enemySpawnPoints.Length;

        int spawned = 0;
        for (int i = 0; i < spawnPoints; i++)
        {
            int index = i % enemyCount;

            ActorSpawnPreferences asp = new ActorSpawnPreferences
            {
                prefab = prefs.m_enemies[index],
                position = prefs.m_enemySpawnPoints[i].position,
                rotation = prefs.m_enemySpawnPoints[i].rotation
            };
            Actor enemy = m_actorManager.Spawn(asp);
            spawned++;
            if (enemy != null)
            {
                CombatActor ca = enemy.Get<CombatActor>();
                if (ca != null)
                    m_enemies.Add(ca);
            }
        }
    }
    private void AreaFinished(CombatResult result) 
    {
        if (!m_started) return;

        switch (result) 
        {
            case CombatResult.Won:
                m_areaCompleted = true;
                break;
            case CombatResult.Lost:

                break;
        }
    }
    public void StartBattle()
    {        
        CombatEvents.CombatStarted();

        if (m_combatPreferences.m_enemies[0] == null)
        {
            Debug.LogWarning("CombatArea Enemy Prefab NULL");
            return;
        }

        CombatPreferences prefs = m_combatPreferences;
        prefs.m_area = this;

        List<CombatActor> combatActors = new List<CombatActor>();
        int setIndex = 0;
        foreach (Actor a in m_actorManager.Party)
        {
            CombatActor ca = a.Get<CombatActor>();
            if (ca != null)
            {
                combatActors.Add(ca);                
            }
            a.Get<MovementController>().
                MoveTo(prefs.m_partySpawnPoints[setIndex]);

            setIndex++;
        }
        
        foreach (var a in m_enemies) 
        {
            combatActors.Add(a);
        }
       
        m_started = true;
        m_combatManager.StartCombat(combatActors, this);
        
    }
    public void EndBattle(CombatResult result) 
    {
        if (result == CombatResult.Won) 
        {
            m_dialogueManger.SetFlag(m_setFlag);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (m_areaCompleted || m_started) return;

        if (!other.CompareTag("Player")) return;

        if (!string.IsNullOrEmpty(m_conditionFlag))
        {
            if (!m_dialogueManger.HasFlag(m_conditionFlag))
                return;
        }

        StartBattle();
    }
    private void OnDrawGizmos()
    {
        if (m_boxCollider == null) 
        {
            m_boxCollider = GetComponent<BoxCollider>();
        }
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + m_boxCollider.center, m_boxCollider.size);
    }
}

