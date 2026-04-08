using System.Collections.Generic;

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
}
public class CombatStatSystem : CombatSystemBase
{
    private Dictionary<CombatActor, CombatActorStats> m_stats;    
    public CombatStatSystem(CombatContext ctx) 
    {
        m_stats = new();

        foreach (var actor in ctx.Actors) 
        {
            m_stats[actor] = new CombatActorStats()
            {
                Actor = actor
            };
        }        
    }
    public override void Init()
    {
        CombatEvents.OnDamageApplied += DamageDealt;
        CombatEvents.OnHealthApplied += HealApplied;
    }
    public override void Dispose()
    {
        CombatEvents.OnDamageApplied -= DamageDealt;
        CombatEvents.OnHealthApplied -= HealApplied;
    }
    public IReadOnlyDictionary<CombatActor, CombatActorStats> GetStats()
    {
        return m_stats;
    }
    public void DamageDealt(CombatActor target, IDamageSource source, float amount) 
    {
        var targetStats = Get(target);
        targetStats.DamageTaken += amount;

        var sourceStats = Get(source.SourceActor);
        sourceStats.DamageDealt += amount;
    }
    public void HealApplied(CombatActor target, IDamageSource source, float amount) 
    {
        var targetStats = Get(target);
        targetStats.HealTaken += amount;

        var sourceStats = Get(source.SourceActor);
        sourceStats.HealDealt += amount;
    }
    public void ActionResolved(ActionContext ctx, ActionResult result) 
    {
        var sourceStats = Get(ctx.Source);
        var targetStats = Get(ctx.Target);

        switch (result) 
        {
            case ActionResult.Confirmed:
                sourceStats.ConfirmsPerformed++;
                break;
            case ActionResult.Parried:
                targetStats.ParriesPerformed++;
                break;
            case ActionResult.Dodged:
                targetStats.DodgesPerformed++;
                break;
            case ActionResult.Hit:
                sourceStats.ActionsHit++;
                break;
        }
        
    }
    public CombatActorStats Get(CombatActor actor) 
    {
        return m_stats.TryGetValue(actor, out var stats) ? stats : null;
    }
}