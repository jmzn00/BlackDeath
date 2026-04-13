using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatusEffects/CureEffect")]
public class CureStatusEffect : ActorStatusEffect 
{
    public override void OnTurnStart(StatusEffectInstance instance)
    {
        instance.SourceActor.ClearStatusEffects();

        base.OnTurnStart(instance);
    }
}