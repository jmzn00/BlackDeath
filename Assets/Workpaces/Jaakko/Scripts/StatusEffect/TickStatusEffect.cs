using UnityEngine;

[CreateAssetMenu(menuName = "Actor/StatusEffects/TickStatusEffect")]
public class TickStatusEffect : ActorStatusEffect
{
    public float tickDamage = 2f;
    protected override void OnApply()
    {        
        base.OnApply();
    }
    protected override void OnTurnStart()
    {
        Owner.Health.ApplyDamage(tickDamage);
        base.OnTurnStart();
    }
    protected override void OnTurnEnd()
    {
        base.OnTurnEnd();
    }
    protected override void OnExpire()
    {
        base.OnExpire();
    }
}
