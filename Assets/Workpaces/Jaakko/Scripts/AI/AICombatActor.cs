using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class AiReactionSettings
{
    [Range(0, 100)]
    public int dodgePercentage;
    [Range(0, 100)]
    public int parryPercentage;
    [Range(0, 100)]
    public int confirmPercentage;
}
public class AICombatActor : CombatActor
{
    [Header("Reaction")]
    [SerializeField] private AiReactionSettings m_reactionSettings;
    [Space]
    [Header("Behaviours")]
    [SerializeField] private List<AIActionBehaviour> m_actionBehaviours;
    [SerializeField] private List<AITargetingBehaviour> m_targetingBehaviours;
    public List<AIActionBehaviour> ActionBehaviours => m_actionBehaviours;
    public List<AITargetingBehaviour> TargetingBehaviours => m_targetingBehaviours;
    protected override void OnInitliazed(GameManager game)
    {
        SetActionProvider(new AIActionProvider(this));
        SetReactionProvider(new AIReactionProvider(m_reactionSettings, this));
    }
    public override void OnCombatFinished()
    {
        base.OnCombatFinished();

        if (!IsPlayer) // TEMP
            Destroy(gameObject);
    }
}
