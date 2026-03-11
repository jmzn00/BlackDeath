using System;
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
    [SerializeField] private AiReactionSettings m_reactionSettings;
    protected override void OnInitliazed(GameManager game)
    {
        SetActionProvider(new AIActionProvider());
        SetReactionProvider(new AIReactionProvider(m_reactionSettings, this));
    }
    public override void OnCombatFinished()
    {
        base.OnCombatFinished();

        if (!IsPlayer) // TEMP
            Destroy(gameObject);
    }
}
