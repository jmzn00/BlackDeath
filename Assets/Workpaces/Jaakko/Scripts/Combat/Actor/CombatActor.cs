using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CombatActorState 
{
    ActionSelecting,
    Targeting
}
public enum Team 
{
    Player,
    Enemy,
    Neutral
}
public enum ControlType
{
    User,
    Ai
}
[RequireComponent(typeof(Actor))]
public class CombatActor : MonoBehaviour, IActorComponent, IDamageSource
{
    private CombatManager m_combatManager;
    public bool IsDead { get; protected set; }
    public bool IsPlayer { get; private set; }

    [SerializeField] private Team m_team;
    [SerializeField] private ControlType m_controlType;

    public Team Team => m_team;
    public ControlType ControlType => m_controlType;

    private Actor m_actor;
    public Actor Actor => m_actor;

    private AnimatorComponent m_animator;


    private IActionProvider m_actionProvider;
    public IActionProvider ActionProvider => m_actionProvider;

    private IReactionProvider m_reactionProvider;
    public IReactionProvider ReactionProvider => m_reactionProvider;    

    private List<StatusEffectInstance> m_currentStatusEffects = new();
    public List<StatusEffectInstance> CurrentStatusEffects => m_currentStatusEffects;
    public event Action<List<StatusEffectInstance>> OnStatusEffectsChanged;

    private HealthComponent m_health;
    public HealthComponent Health => m_health;

    [SerializeField] protected GameObject m_visual; // TEMP, Play Death Animation

    [SerializeField] private List<CombatAction> m_actions;
    public List<CombatAction> Actions => m_actions;
    
    // event that m_animator listens to
    public event Action<AnimationClip> OnPlayRequested;

    public AnimationClip TransitionClip => m_animator.TransitionClip;

    public CombatActor SourceActor { get; private set; }
    public string SourceName { get; private set; }

    private int m_actionPoints;
    [SerializeField] private int m_maxActionPoints;
    public int ActionPoints => m_actionPoints;
    public int MaxActionPoints => m_maxActionPoints;

    [SerializeField] public int CAP;


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

        m_combatManager = game.Resolve<CombatManager>();

        m_combatManager.OnCombatStarted += CombatStarted;
        m_combatManager.OnCombatEnded += CombatEnded;

        m_combatManager.OnTurnStart += TurnStart;
        m_combatManager.OnTurnEnd += TurnEnd;

        SourceActor = this;
        SourceName = name;

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
        m_animator.OnActionAnimationFinished -= ActionFinished;
        OnDispose();
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor)
    {
        m_health = m_actor.Get<HealthComponent>();

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
    #region Combat  
    public void SetDead(bool value) 
    {
        IsDead = value;

        if (m_visual)
            m_visual.SetActive(!value);
    }
    public void TurnStart(CombatActor actor)
    {
        if (actor != this) 
        {
            return;
        }
    }
    public void TurnEnd(CombatActor actor) 
    {
        if (actor != this) 
        {
            return;
        }
    }
    private void CombatStarted()
    {
        MovementController c = Actor.Get<MovementController>();
        if (c != null)
            c.enabled = false;

        AddActionPoints(m_maxActionPoints);
    }
    protected virtual void CombatEnded(CombatResult result) 
    {        
        ClearStatusEffects();

        m_combatManager.OnCombatStarted -= CombatStarted;
        m_combatManager.OnCombatEnded -= CombatEnded;

        m_combatManager.OnTurnStart -= TurnStart;
        m_combatManager.OnTurnEnd -= TurnEnd;

        m_combatManager = null;

        MovementController c = Actor.Get<MovementController>();
        if (c != null) 
        {
            c.enabled = true;
        }
        m_health.ApplyHealth(m_health.MaxHealth);
        SetDead(false);
    }
    #endregion
    #region ActionPoint
    public void AddActionPoints(int amount) 
    {        
        m_actionPoints += amount;
        m_actionPoints = Mathf.Min(m_actionPoints, m_maxActionPoints);
        CAP = m_actionPoints;
    }
    public void RemoveActionPoints(int amount) 
    {
        m_actionPoints -= amount;
        m_actionPoints = Mathf.Max(0, m_actionPoints);
        CAP = m_actionPoints;
    }
    #endregion
    #region Animation
    public bool HasTransition()
    {
        return m_animator.TransitionClip != null;
    }
    public void PlayTransition()
    {
        m_animator.PlayTransition();
    }
    public void PlayAction(ActionContext ctx)
    {
        if (ctx.Action.animationClip == null)
        {
            Debug.LogWarning("AnimationClip is NULL");
            ActionFinished();
            return;
        }
        OnPlayRequested?.Invoke(ctx.Action.animationClip);

        if (m_animationTimeout == null)
            StartCoroutine(AnimationTimeout(ctx.Action.animationClip.length * 1.25f));
    }
    private Coroutine m_animationTimeout;
    private IEnumerator AnimationTimeout(float clipLength) 
    {
        yield return new WaitForSeconds(clipLength);

        ActionFinished();
        m_animationTimeout = null;
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
    private void ClearStatusEffects() 
    {
        foreach (var i in CurrentStatusEffects)
            i.Expire();

        m_currentStatusEffects.Clear();
    }
    public StatusEffectInstance GetInstance(ActorStatusEffect e) 
    {
        foreach (var i in CurrentStatusEffects) 
        {
            if (i.Template == e) 
            {
                return i;
            }
        }
        return null;
    }
    public bool HasEffect(ActorStatusEffect effect) 
    {
        foreach (var i in CurrentStatusEffects) 
        {
            if (i.Template == effect) 
            {
                return true;
            }
        }
        return false;
    }
    public void ApplyEffect(StatusEffectInstance instance) 
    {
        m_currentStatusEffects.Add(instance);
        OnStatusEffectsChanged?.Invoke(m_currentStatusEffects);
    }
    public void RemoveEffect(StatusEffectInstance instance) 
    {
        m_currentStatusEffects.Remove(instance);
        OnStatusEffectsChanged?.Invoke(m_currentStatusEffects);
    }
    #endregion
}