using Mono.Cecil;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
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

    private List<CombatActor> m_enemies;

    private bool m_areaCompleted;
    private bool m_started;

    [Header("Flags")]
    [SerializeField] private string m_conditionFlag;
    [SerializeField] private string m_setFlag;

    [SerializeField] private string m_areaID;
    public string ID => m_areaID;

    private bool m_initialized = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(m_areaID)
            || IsDuplicateID(m_areaID)) 
        {
            m_areaID = Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }        
    }
    private bool IsDuplicateID(string id) 
    {
        CombatArea[] areas = FindObjectsByType<CombatArea>(FindObjectsSortMode.None);
        if (areas == null || areas.Length <= 1)
            return false;
        foreach (CombatArea area in areas) 
        {
            if (area == this) continue;

            if (area.ID == id)
                return true;
        }
        return false;
    }
#endif
    // combat manager will call this if the area was completed in 
    // the current save, this will dictate if the area can be entered
    // and if enemies will be spawned on Initialize
    public void SetCompleted(bool completed) 
    {
        m_areaCompleted = completed;
    }
    public void Initialize(GameManager game) 
    {
        if (m_areaCompleted) return;

        m_combatManager = game.Resolve<CombatManager>();
        m_actorManager = game.Resolve<ActorManager>();
        m_dialogueManger = game.Resolve<DialogueManager>();

        if (m_boxCollider == null)
            m_boxCollider = GetComponent<BoxCollider>();

        m_started = false;  

        SpawnEnemies();

        m_initialized = true;
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
    public void AreaFinished(CombatResult result) 
    {
        if (!m_started) return;        

        switch (result) 
        {
            case CombatResult.Won:
                if (!string.IsNullOrEmpty(m_setFlag))
                    m_dialogueManger.SetFlag(m_setFlag);
                break;
            case CombatResult.Lost:
                
                break;
        }
    }
    public void StartCombat()
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
        foreach (var actor in combatActors)
            actor.CombatStarted();

        m_started = true;
        m_combatManager.StartCombat(combatActors, this);

        m_boxCollider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (m_areaCompleted || m_started
            || !m_initialized) return;

        if (!other.CompareTag("Player")) return;

        if (!string.IsNullOrEmpty(m_conditionFlag))
        {
            if (!m_dialogueManger.HasFlag(m_conditionFlag))
                return;
        }

        StartCombat();
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

