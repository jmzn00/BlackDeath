using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusView : UIViewBase
{
    [Header("Prefabs")]
    [SerializeField] private CombatPortrait m_portraitPrefab;

    private List<CombatPortrait> m_portraits;
    private Dictionary<CombatActor, CombatPortrait> m_portraitLookup;
    public override void Init() 
    { 
        if (m_portraits != null) 
        {
            foreach (var p in m_portraits) 
            {
                Destroy(p.gameObject);
            }
            m_portraits.Clear();
        }
        m_portraits = new();
        m_portraitLookup = new();

        for (int i = 0; i < 4; i++) 
        {
            CreatePortrait();
        }
    }
    private void CreatePortrait() 
    {
        CombatPortrait p = Instantiate(m_portraitPrefab, transform);
        m_portraits.Add(p);
        TogglePortrait(p, false);
    }
    private void TogglePortrait(CombatPortrait p, bool value) 
    {
        p.gameObject.SetActive(value);
    }

    public void ActorsChanged(List<CombatActor> actors) 
    {
        List<CombatActor> allies = new List<CombatActor>(actors
            .Where(a => a.Team == Team.Player));

        if (allies.Count > m_portraits.Count) 
        {
            int diff = allies.Count - m_portraits.Count;

            for (int i = 0; i < diff; i++) 
            {
                CreatePortrait();
            }
        }

        for (int i = 0; i < allies.Count; i++) 
        {
            CombatPortrait p = m_portraits[i];
            CombatActor a = allies[i];

            p.Bind(a);
            m_portraitLookup[a] = p;
            TogglePortrait(p, true);
        }
    }
    public void ActorDied(CombatActor actor) 
    {
        if (m_portraitLookup.TryGetValue(actor, out CombatPortrait p)) 
        {
            p.Dispose();
            TogglePortrait(p, false);
        }
    }
}