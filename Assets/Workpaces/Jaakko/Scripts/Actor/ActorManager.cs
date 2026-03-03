using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ActorManager : IManager
{
    private bool m_active;

    private List<IActor> m_actors;

    private Actor m_player = null;
    public Actor Player => m_player;

    private GameManager m_game;

    public ActorManager(GameManager game) 
    {
        m_game = game;
    }
    public List<ActorSaveData> SaveAllActors() 
    {
        // slow
        return m_actors.Select(actor => actor.Save()).ToList();
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
                Debug.LogWarning($"Actor with ID {data.ActorID} not found in scene!");
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
        m_actors = new List<IActor>();
        IActor[] actorsInScene = GameObject.
            FindObjectsByType<Actor>(FindObjectsSortMode.None)
            .OfType<IActor>().ToArray();
        foreach (var actor in actorsInScene)
            Register(actor, m_game);

        m_player = m_actors.OfType<Actor>()
            .FirstOrDefault(a => a.CompareTag("Player"));
        if (m_player == null)
            Debug.LogError("Player Actor not found in scene");

        foreach (var a in m_actors)
            a.Init(m_game);
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
}
