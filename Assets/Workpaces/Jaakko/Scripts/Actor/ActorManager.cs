using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ActorSpawnPreferences 
{
    public Actor prefab;
    public Vector3 position;
    public Quaternion rotation;
}
public class ActorManager : IManager
{
    private bool m_active;
    
    private GameManager m_game;

    private List<IActor> m_actors;
    private List<Actor> m_party = new List<Actor>();
    public IReadOnlyList<Actor> Party => m_party;

    private Actor m_currentControlled;
    public Actor CurrentControlled => m_currentControlled;    

    public event Action<Actor> OnActorControlChanged;
    public event Action OnReady;
    public bool IsReady { get; private set; }
    private void SetReady(bool value)
    {
        if (IsReady) return;

        IsReady = true;
        OnReady?.Invoke();
    }
    public void SetControlledActor(Actor actor) 
    {
        if (actor == null || !m_party.Contains(actor)) 
        {
            Debug.LogWarning("Trying to set controlled actor that is not in party");
            return;
        }
        m_currentControlled = actor;
        OnActorControlChanged?.Invoke(actor);
    }
    public void SwitchToNextActor() 
    {
        if (m_game.State == GameState.Combat) return;

        if (m_party.Count <= 1) 
        {
            Debug.Log($"Cannot switch actors. Party count: {m_party.Count}");
            return;
        }
        int currentIndex = m_party.IndexOf(m_currentControlled);
        int nextIndex = (currentIndex + 1) % m_party.Count;

        SetControlledActor(m_party[nextIndex]);
    }

    public ActorManager(GameManager game) 
    {
        m_game = game;
    }
    public void OnSceneLoaded(SceneData data) 
    {
        IsReady = false;
        if (data.IsGameplay) 
        {
            GatherActorsInScene();
        }
        SetReady(true);
    }
    public List<ActorSaveData> SaveAllActors() 
    {
        List<ActorSaveData> save = new List<ActorSaveData>();
        for (int i = 0; i < m_actors.Count; i++) 
        {
            if (m_actors[i] == null)
                continue;
            save.Add(m_actors[i].Save());
        }
        return save;
    }
    public void LoadAllActors(List<ActorSaveData> actorDataList) 
    {
        foreach (var data in actorDataList) 
        {
            IActor actor = m_actors.FirstOrDefault(a => a.ActorID == data.ActorID);
            if (actor != null) 
            {
                actor.LoadData(data);
            }
            else 
            {
                Debug.LogWarning($"Actor {data.ActorName} with ID {data.ActorID} not found in scene!");
            }
        }
    }
    public bool Init() 
    {        
        m_active = true;
        return true;
    }
    public void OnManagersInitialzied() 
    {        

    }
    private void GatherActorsInScene() 
    {
        m_actors = new List<IActor>();
        IActor[] actorsInScene = GameObject.
            FindObjectsByType<Actor>(FindObjectsSortMode.None)
            .OfType<IActor>().ToArray();

        foreach (var actor in actorsInScene)
            Register(actor, m_game);

        foreach (var a in m_actors)
            a.Init(m_game);

        m_party = m_actors.OfType<Actor>()
            .Where(a => a.IsPlayable)
            .ToList();
        if (m_party.Count > 0)
        {
            foreach (var a in m_party)
            {
                if (a.CompareTag("Player"))
                    SetControlledActor(a);
            }
            if (m_currentControlled == null)
                SetControlledActor(m_party[0]);
        }
        else
            Debug.LogWarning("No playable actors found in scene!");
    }
    public bool Dispose() 
    {
        m_active = false;

        foreach (var a in new List<IActor>(m_actors)) 
        {
            Unregister(a);
        }
        return true;
    }
    public void Update(float dt) 
    {
        if (!m_active) return;
    }
    public bool Register(IActor actor, GameManager game) 
    {
        if (m_actors.Contains(actor)) 
            return false;
        
        actor.EnsureID();
        m_actors.Add(actor);
        return true;
    }
    public bool Unregister(IActor actor) 
    {
        if (!m_actors.Contains(actor)) 
        {
            return false;
        }
        actor.Dispose();
        m_actors.Remove(actor);
        return true;    
    }
    public Actor Spawn(ActorSpawnPreferences prefs) 
    {
        if (prefs.prefab == null) 
        {
            Debug.LogWarning("AM: Trying To Spawn Actor With Null Prefab");
            return null;
        }

        Actor a = GameObject.Instantiate(prefs.prefab, prefs.position
            , prefs.rotation);
        a.Init(m_game);
        return a;
        // dont register runtime spawned actors, they dont need save data
        /*
        IActor ia = a.GetComponent<IActor>();
        
        if (ia != null) 
        {
            Register(ia, m_game);
            return a;
        }
        else
            return null;
        */
    }
}
