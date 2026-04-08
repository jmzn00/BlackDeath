using System;
using UnityEngine;

public class CombatUIViewGroup : UIViewGroup
{
    [field: Header("Views")]
    [field: Space]
    [field: SerializeField] public ActionView ActionView{ get; private set; }
    [field: SerializeField] public TargetView TargetView{ get; private set; }
    [field: SerializeField] public DamageView DamageView{ get; private set; }
    [field: SerializeField] public ReactionView ReactionView{ get; private set; }
    [field: SerializeField] public StatusView StatusView{ get; private set; }
    [field:SerializeField] public ResultView ResultView { get; private set; }

    public override Type ComponentType => typeof(CombatUI);
}
