using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatusEffects/HealEffect")]
public class HealStatusEffect : ActorStatusEffect
{
    public override void OnTurnStart(StatusEffectInstance instance)
    {
        instance.TickDuration();
        base.OnTurnStart(instance);
    }
}
