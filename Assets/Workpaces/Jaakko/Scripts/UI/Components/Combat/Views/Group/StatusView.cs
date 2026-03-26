using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusView : MonoBehaviour, IUIComponentView 
{
    [Header("Prefabs")]
    [SerializeField] private CombatPortrait m_portraitPrefab;

    private Dictionary<CombatActor, CombatPortrait> m_portraits;

    public void Init() 
    { 
        if (m_portraits != null) 
        {
            foreach (var kvp in m_portraits) 
            {
                Destroy(kvp.Value);
            }
            m_portraits.Clear();
        }
        m_portraits = new();
    }
    public void View() { gameObject.SetActive(true); }
    public void Hide() { gameObject.SetActive(false); }

    public void ActorsChanged(List<CombatActor> actors) 
    {
        List<CombatActor> allies = new List<CombatActor>(actors
            .Where(a => a.Team == Team.Player));

        foreach (var a in allies) 
        {
            CombatPortrait p = Instantiate(m_portraitPrefab, transform);
            p.Bind(a);

            m_portraits[a] = p;
        }
    }
    public void ActorDied(CombatActor actor) 
    {
        if (m_portraits.TryGetValue(actor, out CombatPortrait portrait))
        {
            Destroy(portrait.gameObject);
            m_portraits.Remove(actor);
        }
    }
}