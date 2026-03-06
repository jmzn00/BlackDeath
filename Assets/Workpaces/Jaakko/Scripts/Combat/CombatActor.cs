using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Actor))]
public class CombatActor : MonoBehaviour, IActorComponent
{
    [SerializeField] private int m_initiative = 0;
    public int Initiative => m_initiative;
    public bool IsDead;
    public bool IsPlayer;

    private Actor m_actor;
    public Actor Actor => m_actor;
    private CombatManager m_combatManager;

    [SerializeField] private List<CombatAction> m_actions;
    public List<CombatAction> Actions => m_actions;

    private CombatContext m_combatContext;
    public CombatContext Context => m_combatContext;

    private CombatAction m_action;

    public event Action<CombatContext> OnContextChanged;


    #region IActorComponent
    public bool Initialize(GameManager game) 
    {
        m_actor = GetComponent<Actor>();
        m_combatManager = game.Resolve<CombatManager>();

        IsPlayer = m_actor.IsPlayable;
        return true;
    }
    public bool Dispose() 
    {
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor) 
    {
    
    }
    public void SetInputSource(IInputSource source) 
    {
        
    }
    public void LoadData(ActorSaveData data) 
    {
    
    }
    public void SaveData(ActorSaveData data) 
    {
    
    }
    #endregion
    public void ReceiveAction(CombatContext ctx) 
    {
    
    }
    public void OnCombatContextChanged(CombatContext ctx) 
    {
        m_combatContext = ctx;
        OnContextChanged?.Invoke(ctx);
    }
    public void OnCombatStarted() 
    {
        if (IsPlayer) 
        {
            m_actor.UI.ShowComponent<CombatUI>(true);
        }
    }
    public void OnCombatFinished() 
    {
    
    }
    public void OnTurn() 
    {
        
    }
    public ActionContext RequestAction(List<CombatActor> participants) 
    {
        return null;
    }
    private CombatAction m_selectedAction;
    public bool SetActionContext(ActionContext ctx) 
    {
        
        return false;   
    }
}
