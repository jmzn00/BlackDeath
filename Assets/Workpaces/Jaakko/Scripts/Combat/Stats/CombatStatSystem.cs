using System.Collections.Generic;
using System.Linq;

public class CombatActorStats 
{
    public CombatActor Actor;

    public float DamageDealt = 0;
    public float DamageTaken = 0;
    
    public float HealDealt = 0;
    public float HealTaken = 0;

    public int ActionsHit = 0;

    public int ParriesPerformed = 0;
    public int DodgesPerformed = 0;
    public int ConfirmsPerformed = 0;

    public float score = 0;
}
public class CombatStatSystem : CombatSystemBase
{
    private Dictionary<CombatActor, CombatActorStats> m_stats;    
    public CombatStatSystem() 
    {        
    }
    public override void Init(CombatContext context)
    {
        CombatEvents.OnDamageApplied += DamageDealt;
        CombatEvents.OnHealthApplied += HealApplied;

        CombatEvents.OnActionResolved += ActionResolved;

        m_stats = new();

        foreach (var actor in context.Actors)
        {
            m_stats[actor] = new CombatActorStats()
            {
                Actor = actor
            };
        }
    }
    public override void Reset()
    {
        CombatEvents.OnDamageApplied -= DamageDealt;
        CombatEvents.OnHealthApplied -= HealApplied;

        CombatEvents.OnActionResolved -= ActionResolved;

        m_stats.Clear();
    }
    public List<CombatActorStats> GetStatsOrdered()
    {
        return m_stats.Values
        .OrderByDescending(stats => stats.score)
        .ToList();
    }
    public void DamageDealt(CombatActor target, IDamageSource source, float amount) 
    {
        var targetStats = Get(target);
        targetStats.DamageTaken += amount;
        targetStats.score += amount / 2;

        var sourceStats = Get(source.SourceActor);
        sourceStats.DamageDealt += amount;
        sourceStats.score += amount;

    }
    public void HealApplied(CombatActor target, IDamageSource source, float amount) 
    {
        var targetStats = Get(target);
        targetStats.HealTaken += amount;

        var sourceStats = Get(source.SourceActor);
        sourceStats.HealDealt += amount;
        sourceStats.score += amount;
    }
    public void ActionResolved(ActionContext ctx, ActionResult result) 
    {
        var sourceStats = Get(ctx.Source);
        var targetStats = Get(ctx.Target);

        switch (result) 
        {
            case ActionResult.Confirmed:
                sourceStats.ConfirmsPerformed++;
                sourceStats.score += 1;
                break;
            case ActionResult.Parried:
                targetStats.ParriesPerformed++;
                targetStats.score += 3;
                break;
            case ActionResult.Dodged:
                targetStats.DodgesPerformed++;
                targetStats.score += 2;
                break;
            case ActionResult.Hit:
                sourceStats.ActionsHit++;
                break;
        }
        
    }
    public CombatActorStats Get(CombatActor actor) 
    {
        if (actor == null)
            return null;

        return m_stats.TryGetValue(actor, out var stats) ? stats : null;
    }
}