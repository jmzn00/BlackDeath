using System;
using System.Collections.Generic;
using UnityEngine;
public enum CombatActorState 
{
    ActionSelecting,
    Targeting
}
[RequireComponent(typeof(Actor))]
public class CombatActor : MonoBehaviour, IActorComponent
{
    private CombatManager m_combatManager;
    public bool IsDead { get; protected set; }
    public bool IsPlayer { get; private set; }

    private Actor m_actor;
    public Actor Actor => m_actor;

    private AnimatorComponent m_animator;
    public AnimatorComponent Animator => m_animator;


    private IActionProvider m_actionProvider;
    public IActionProvider ActionProvider => m_actionProvider;

    private IReactionProvider m_reactionProvider;
    public IReactionProvider ReactionProvider => m_reactionProvider;

    private List<ActorStatusEffect> m_statusEffects = new List<ActorStatusEffect>();
    public List<ActorStatusEffect> StatusEffects => m_statusEffects;
    public event Action<List<ActorStatusEffect>> OnStatusEffectsChanged;

    private HealthComponent m_health;
    public HealthComponent Health => m_health;

    [SerializeField] protected GameObject m_visual; // TEMP

    [SerializeField] private List<CombatAction> m_actions;
    public List<CombatAction> Actions => m_actions;
    
    // event that m_animator listens to
    public event Action<AnimationClip> OnPlayRequested;

    public event Action<CombatActorState> OnCombatActorStateChanged;
    public event Action<CombatActor> OnCurrentTargetChanged;

    #region IActionProvider
    protected void SetActionProvider(IActionProvider provider)
    {
        m_actionProvider = provider;
    }
    protected void SetReactionProvider(IReactionProvider provider)
    {
        m_reactionProvider = provider;
    }
    #endregion
    #region IActorComponent
    public bool Initialize(GameManager game)
    {
        m_actor = GetComponent<Actor>();
        IsPlayer = m_actor.IsPlayable;
        m_combatManager = game.Resolve<CombatManager>();

        m_combatManager.OnCombatStarted += CombatStarted;
        m_combatManager.OnCombatEnded += CombatEnded;

        m_combatManager.OnTurnStart += TurnStart;
        m_combatManager.OnTurnEnd += TurnEnd;
        OnInitliazed(game);
        return true;
    }
    protected virtual void OnInitliazed(GameManager game) 
    {

    }   
    protected virtual void OnDispose() 
    {
        
    }
    public bool Dispose()
    {
        m_health.OnHealthChanged -= OnHealthChanged;
        m_animator.OnActionAnimationFinished -= ActionFinished;
        OnDispose();
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor)
    {
        m_health = m_actor.Get<HealthComponent>();
        m_health.OnHealthChanged += OnHealthChanged;

        m_animator = actor.Get<AnimatorComponent>();
        m_animator.OnActionAnimationFinished += ActionFinished;
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
    public void Load(object data)
    {

    }
    public object Save()
    {
        return null;
    }
    #endregion
    #region UiSelection

    // ui calls
    public void ChangeState(CombatActorState state) 
    {
        if (m_state == state) return;

        m_state = state;
        OnCombatActorStateChanged?.Invoke(state);
        
        // Fire global event for camera system
        CombatEvents.ActorStateChanged(this, state);
    }
    // ui calls
    public void ChangeTarget(CombatActor actor) 
    {
        Debug.Log($"Selected Target: {actor.name}");

        OnCurrentTargetChanged?.Invoke(actor);
    }
    #endregion
    #region Combat  
    public AnimationClip TransitionClip => m_animator.TransitionClip;
    public bool HasTransition() 
    {
        return m_animator.TransitionClip != null;
    }
    public void PlayTransition() 
    {
        m_animator.PlayTransition();
    }
    void OnHealthChanged(float value)
    {
        if (IsDead) return;

        if (value <= 0f)
        {            
            IsDead = true;
            CombatEvents.ActorDied(this); // CHANGE
            if (m_visual) // temp
                m_visual.SetActive(false); // temp
        }
    }
    public void TurnStart(CombatActor actor)
    {
        if (actor != this) 
        {
            return;
        }
        HandleStatusEffectsTurnStart();
    }
    public void TurnEnd(CombatActor actor) 
    {
        if (actor != this) 
        {
            return;
        }
        HandleStatusEffectsTurnEnd();
    }
    private void CombatStarted()
    {
        MovementController c = Actor.Get<MovementController>();
        if (c != null)
            c.enabled = false;
    }
    protected virtual void CombatEnded(CombatResult result) 
    {        
        ClearStatusEffects();

        MovementController c = Actor.Get<MovementController>();
        if (c != null) 
        {
            c.enabled = true;
        }
        
    }
    public void SubmitAction(CombatActor source,
        CombatActor target, CombatAction action)
    {
        m_combatManager.Action.SubmitAction(this,
            target, action);
    }
    private CombatActorState m_state;
    public void PlayAction(ActionContext ctx, Action onComplete)
    {
        if (ctx.Source == ctx.Target) 
        {
            ActionFinished();
            return;
        }
        if (ctx.Action.animationClip == null) 
        {
            Debug.LogWarning("AnimationClip is NULL");
            ActionFinished();
            return;
        }        
        OnPlayRequested?.Invoke(ctx.Action.animationClip);
    }
    // called by m_animator
    public void CloseWindow()
    {
        m_combatManager.Action.ClosePrompt(this);
    }
    // called by m_animator
    public void OpenWindow(string promptKey)
    {
        m_combatManager.Action.OpenPrompt(this, promptKey);
        
    }
    // called by event
    public void ActionFinished()
    {
        m_combatManager.Action.NotifyActionFinished(this);
    }
    #endregion
    #region StatusEffect
    private void HandleStatusEffectsTurnStart()
    {
        if (m_statusEffects == null || m_statusEffects.Count == 0) return;

        List<ActorStatusEffect> effects = new List<ActorStatusEffect>(m_statusEffects);
        foreach (var e in effects)
        {
            if (e == null) return;
            e.TurnStart();
        }
        if (IsDead)
        {
            CombatEvents.ActorDied(this);
        }
    }
    private void HandleStatusEffectsTurnEnd()
    {
        for (int i = m_statusEffects.Count - 1; i >= 0; i--)
        {
            var effect = m_statusEffects[i];
            effect.TurnEnd();

            if (effect.TickDuration())
            {
                effect.Expire();
                m_statusEffects.RemoveAt(i);
                Destroy(effect);
            }
        }
        if (IsDead)
        {
            CombatEvents.ActorDied(this);
        }
        OnStatusEffectsChanged?.Invoke(m_statusEffects);
    }
    private void ClearStatusEffects() 
    {
        foreach (var effect in m_statusEffects)
            Destroy(effect);

        m_statusEffects.Clear();
    }
    public void ApplyStatus(ActorStatusEffect effect)
    {
        ActorStatusEffect instance = Instantiate(effect);
        instance.Initialize(this);

        var existing = m_statusEffects.Find(e =>
            e.GetType() == instance.GetType() &&
            e.displayName == instance.displayName);

        if (existing != null)
        {
            if (effect.IsStackable)
            {
                existing.AddDuration(instance.duration);
                OnStatusEffectsChanged?.Invoke(m_statusEffects);
            }
            return;
        }

        m_statusEffects.Add(instance);
        OnStatusEffectsChanged?.Invoke(m_statusEffects);
    }
    #endregion
}