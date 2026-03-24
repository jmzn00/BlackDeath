using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatusEffects/HealEffect")]
public class HealStatusEffect : ActorStatusEffect
{
    public override void OnTurnEnd(StatusEffectInstance instance)
    {
        instance.TickDuration();

        base.OnTurnEnd(instance);
    }
}
