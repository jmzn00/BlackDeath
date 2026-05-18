using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatuEffects/BurnEffect")]
public class BurnStatusEffect : ActorStatusEffect
{
    public override void OnTurnStart(StatusEffectInstance instance)
    {
        instance.TickDuration();
        base.OnTurnStart(instance);
    }
}
