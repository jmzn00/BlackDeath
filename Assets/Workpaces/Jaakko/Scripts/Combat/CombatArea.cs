using Mono.Cecil;
using System.Collections.Generic;
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
    private bool m_areaCompleted;
    private bool m_spawned;

    [Header("Flags")]
    [SerializeField] private string m_conditionFlag;
    [SerializeField] private string m_setFlag;

    public void Initialize(GameManager game) 
    {
        m_combatManager = game.Resolve<CombatManager>();
        m_actorManager = game.Resolve<ActorManager>();
        m_dialogueManger = game.Resolve<DialogueManager>();
        m_spawned = false;

        CombatEvents.OnCombatEnded += AreaFinished;
    }
    private void AreaFinished(CombatResult result) 
    {
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

        int enemyCount = prefs.m_enemies.Length;
        int spawnPoints = prefs.m_enemySpawnPoints.Length;

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
            if (enemy != null)
            {
                CombatActor ca = enemy.Get<CombatActor>();
                if (ca != null)
                    combatActors.Add(ca);
            }
        }
        m_spawned = true;
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
        if (m_areaCompleted || m_spawned) return;

        // if the area has a condition flag and player does
        // not have the flag then they cannot start the battle
        if (other.CompareTag("Player")) 
        {
            if (!string.IsNullOrEmpty(m_conditionFlag))
            {
                if (!m_dialogueManger.HasFlag(m_conditionFlag))
                    return;
            }

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
        Gizmos.DrawCube(transform.position + m_boxCollider.center, m_boxCollider.size);
    }
}

