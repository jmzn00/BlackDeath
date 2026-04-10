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

    public event Action<int> OnActionPointsChanged;

    private ActionSystem m_action;

    public SkipTurnAction SkipAction { get; private set; }
    #region AiPattern
    // this regions content is just for Ai CombatAction PatternBehaviours
    private int m_patternIndex = 0;
    public void UpdatePatternIndex(int index)
    {
        m_patternIndex = index;
    }
    public void IncrementPatternIndex()
    {
        m_patternIndex++;
    }
    public int GetPatternIndex()
    {
        return m_patternIndex;
    }
    #endregion
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

        CombatEvents.OnCombatStarted += CombatStarted;
        CombatEvents.OnTurnStarted += TurnStart;
        CombatEvents.OnTurnEnded += TurnEnd;

        SourceActor = this;
        SourceName = name;

        OnInitliazed(game);

        if (!m_actions.Exists(a => a is SkipTurnAction)) 
        {
            SkipTurnAction skip = Resources
                .Load<SkipTurnAction>
                ("Actions/SkipTurnAction");

            if (skip == null) 
            {
                Debug.LogWarning("SkipTurnAction is null in Resources/Actions");
            }
            else 
            {
                m_actions.Add(skip);
                SkipAction = skip;
            }
        }
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

        CombatEvents.OnCombatStarted -= CombatStarted;
        CombatEvents.OnTurnStarted -= TurnStart;
        CombatEvents.OnTurnEnded -= TurnEnd;

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
    public virtual void CombatStarted()
    {
        if (m_action == null)
            m_action = m_combatManager.Container.Resolve<ActionSystem>();

        AddActionPoints(m_maxActionPoints);
    }
    public virtual void CombatEnded(CombatResult result) 
    {        
        ClearStatusEffects();

        m_combatManager = null;

        m_health.ApplyHealth(m_health.MaxHealth);
        SetDead(false);
    }
    #endregion
    #region ActionPoint
    public void AddActionPoints(int amount) 
    {        
        m_actionPoints += amount;
        m_actionPoints = Mathf.Min(m_actionPoints, m_maxActionPoints);
        OnActionPointsChanged?.Invoke(m_actionPoints);
    }
    public void RemoveActionPoints(int amount) 
    {
        m_actionPoints -= amount;
        m_actionPoints = Mathf.Max(0, m_actionPoints);
        OnActionPointsChanged?.Invoke(m_actionPoints);
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
        m_action.ClosePrompt(this);
    }
    // called by m_animator
    public void OpenWindow(string promptKey)
    {
        m_action.OpenPrompt(this, promptKey);
    }
    // called by event
    public void ActionFinished()
    {
        m_action.NotifyActionFinished(this);
    }
    #endregion
    #region StatusEffect
    private void ClearStatusEffects() 
    {
        foreach (var i in CurrentStatusEffects) 
        {
            i.Expire();
        }            
        m_currentStatusEffects.Clear();
        OnStatusEffectsChanged?.Invoke(m_currentStatusEffects);
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
    public bool HasEffect<T>() where T : ActorStatusEffect
    {
        foreach (var i in CurrentStatusEffects)
        {
            if (i.Template is T)
                return true;
        }
        return false;
    }
    public void RemoveEffect<T>() where T : ActorStatusEffect 
    {
        foreach (var i in new List<StatusEffectInstance>(CurrentStatusEffects))
        {
            if (i.Template is T) 
            {
                i.Expire();
                RemoveEffect(i);
            }                
        }
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