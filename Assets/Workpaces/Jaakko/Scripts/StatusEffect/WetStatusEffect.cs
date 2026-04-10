using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatusEffects/WetEffect")]
public class WetStatusEffect : ActorStatusEffect 
{
    public override void OnTurnStart(StatusEffectInstance instance)
    {
        if (instance.SourceActor.HasEffect<BurnStatusEffect>())
        {
            instance.SourceActor.RemoveEffect<BurnStatusEffect>();
        }

        instance.TickDuration();
        base.OnTurnStart(instance);
    }
}