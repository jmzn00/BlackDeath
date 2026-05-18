using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatusEffects/TickStatusEffect")]
public class TickStatusEffect : ActorStatusEffect
{
    public override void OnApply(StatusEffectInstance instance)
    {        
        base.OnApply(instance);
    }
    public override void OnTurnStart(StatusEffectInstance instance)
    {
        instance.TickDuration();
        base.OnTurnStart(instance);
    }
    public override void OnTurnEnd(StatusEffectInstance instance)
    {
        base.OnTurnEnd(instance);
    }
    public override void OnExpire(StatusEffectInstance instance)
    {
        base.OnExpire(instance);
    }
}
