using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Actor))]
public class CombatActor : MonoBehaviour, IActorComponent
{
    [SerializeField] private int m_initiative = 0;
    [SerializeField] private List<CombatAction> m_actions;
    public int Initiative => m_initiative;
    
    public bool IsDead { get; private set; }
    public bool IsPlayer { get; private set; }

    private Actor m_actor;
    public Actor Actor => m_actor;
    private CombatManager m_combatManager;    
    public List<CombatAction> Actions => m_actions;

    private CombatContext m_combatContext;
    public CombatContext Context => m_combatContext;

    private CombatAction m_action;

    public event Action<CombatContext> OnContextChanged;

    private IActionProvider m_actionProvider;
    public IActionProvider ActionProvider => m_actionProvider;

    private HealthComponent m_health;
    public HealthComponent Health => m_health;

    private UIController m_uiController;


    #region IActorComponent
    public bool Initialize(GameManager game) 
    {
        m_actor = GetComponent<Actor>();             

        m_combatManager = game.Resolve<CombatManager>();
        m_uiController = game.Resolve<UIManager>().Controller;

        IsPlayer = m_actor.IsPlayable;

        if (IsPlayer) 
        {
            SetActionProvider(new PlayerActionProvider(m_combatManager
                , m_uiController));
        }
        else 
        {
            SetActionProvider(new AIActionProvider());
        }        
            return true;
    }
    [SerializeField] private GameObject m_visual; // temp  
    void OnHealthChanged(float value) 
    {
        if (value <= 0f) 
        {
            IsDead = true;
            m_combatManager.OnActorDied(this);
            if (m_visual)
                m_visual.SetActive(false);
        }
    }
    public bool Dispose() 
    {
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor) 
    {
        m_health = m_actor.Get<HealthComponent>();
        m_health.OnHealthChanged += OnHealthChanged;
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
    public void OnCombatContextChanged(CombatContext ctx) 
    {
        m_combatContext = ctx;
        OnContextChanged?.Invoke(ctx);
    }
    public void OnCombatStarted() 
    {

    }
    public void OnCombatFinished() 
    {
        if (!IsPlayer)
            Destroy(gameObject); // add pooling?
    }
    
    private ActionContext m_pendingAction;
    public void ExecuteAction(ActionContext ctx) 
    {
                
    }
    public bool SetActionContext(ActionContext ctx) 
    {
        ctx.Source = this;

        m_combatManager.SubmitAction(ctx);
        return false;   
    }
    private void SetActionProvider(IActionProvider provider) 
    {
        m_actionProvider = provider;
    }
}
