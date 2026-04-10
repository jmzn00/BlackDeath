using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatuEffects/BurnEffect")]
public class BurnStatusEffect : ActorStatusEffect
{
    public override void OnTurnEnd(StatusEffectInstance instance)
    {
        instance.TickDuration();

        base.OnTurnEnd(instance);
    }
}
