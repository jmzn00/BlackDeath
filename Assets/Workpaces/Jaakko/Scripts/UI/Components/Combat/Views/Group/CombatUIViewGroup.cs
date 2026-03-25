using System;
using UnityEngine;

public class CombatUIViewGroup : UIViewGroup
{
    [Header("Views")]
    public ActionView ActionView;
    public TargetView TargetView;
    public DamageView DamageView;

    public override Type ComponentType => typeof(CombatUI);
}
