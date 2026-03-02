using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ActorManager : IManager
{
    private bool m_active;

    private List<IActor> m_actors;

    public List<ActorSaveData> SaveAllActors() 
    {
        return m_actors.Select(actor => actor.Save()).ToList();
    }
    public void LoadAllActors(List<ActorSaveData> actorDataList) 
    {
        foreach (var data in actorDataList) 
        {
            IActor actor = m_actors.FirstOrDefault(a => a.ActorID == data.ActorID);
            if (actor != null) 
            {
                actor.Load(data);
            }
            else 
            {
                Debug.LogWarning($"Actor with ID {data.ActorID} not found in scene!");
            }
        }
    }
    public bool Init(GameManager game) 
    {        
        m_active = true;
        m_actors = new List<IActor>();
        IActor[] actorsInScene = GameObject.
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IActor>().ToArray();
        foreach (var actor in actorsInScene)
            Register(actor);

        return m_active;
    }
    public bool Dispose(GameManager game) 
    {
        m_active = false;

        foreach (var a in new List<IActor>(m_actors)) 
        {
            Unregister(a);
        }
        return m_active;
    }
    public void Update(float dt) 
    {
        if (!m_active) return;
    }
    public bool Register(IActor actor) 
    {
        if (m_actors.Contains(actor)) 
            return false;

        actor.EnsureID();
        actor.Init();
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
