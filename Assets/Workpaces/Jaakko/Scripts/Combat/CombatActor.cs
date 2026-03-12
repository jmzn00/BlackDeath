using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Actor))]
public class CombatActor : MonoBehaviour, IActorComponent
{    
    public bool IsDead { get; protected set; }
    public bool IsPlayer { get; private set; }
    private Actor m_actor;
    public Actor Actor => m_actor;

    protected CombatManager m_combatManager;

    private IActionProvider m_actionProvider;
    public IActionProvider ActionProvider => m_actionProvider;

    private IReactionProvider m_reactionProvider;
    public IReactionProvider ReactionProvider => m_reactionProvider;

    [SerializeField] private List<CombatAction> m_actions;
    public List<CombatAction> Actions => m_actions;
    private Action m_onActionComplete;
    private Coroutine m_actionTimeout;
    public event Action OnActionFinished;

    private List<ActorStatusEffect> m_statusEffects = new List<ActorStatusEffect>();
    public List<ActorStatusEffect> StatusEffects => m_statusEffects;
    public event Action<List<ActorStatusEffect>> OnStatusEffectsChanged;

    private ActionContext m_currentContext;
    public event Action<CombatContext> OnContextChanged;

    private HealthComponent m_health;
    public HealthComponent Health => m_health;

    private AnimationController m_animationController; // temp
    [SerializeField] protected GameObject m_visual; // temp 

    protected bool m_defensiveAnimationPlaying;
    public bool DefensiveAnimationPlaying => m_defensiveAnimationPlaying;

    protected bool m_currentlyTargeted;
    public event Action<CombatActor, CombatActor, CombatAction> OnTargeted;
    public event Action<CombatActor, CombatActor, CombatAction> OnNoLongerTargeted;
    public void NotifyTargeted(CombatActor source, CombatAction action) 
    {
        OnTargeted?.Invoke(source, this, action);
    }
    public void NotifyNoLongerTargeted(CombatActor source, CombatAction action) 
    {
        OnNoLongerTargeted?.Invoke(source, this, action);
    }
    #region IActorComponent
    public bool Initialize(GameManager game)
    {
        m_actor = GetComponent<Actor>();
        IsPlayer = m_actor.IsPlayable;
        m_combatManager = game.Resolve<CombatManager>();

        CombatEvents.OnCombatStarted += CombatStarted;
        CombatEvents.OnCombatEnded += CombatEnded;
        CombatEvents.OnTurnStarted += TurnStart;
        CombatEvents.OnTurnEnded += TurnEnd;
        OnInitliazed(game);
        return true;
    }
    protected virtual void OnInitliazed(GameManager game) 
    {

    }   
    protected virtual void OnDispose() 
    {
        
    }
    void OnHealthChanged(float value)
    {        
        if (value <= 0f)
        {
            IsDead = true;
            //m_combatManager.OnActorDied(this);
            CombatEvents.ActorDied(this);
            if (m_visual) // temp
                m_visual.SetActive(false); // temp
        }
    }
    public bool Dispose()
    {
        m_animationController.OnDefensiveAnimationPlaying -= OnDefensiveAnimationPlaying;
        OnDispose();
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor)
    {
        m_health = m_actor.Get<HealthComponent>();
        m_health.OnHealthChanged += OnHealthChanged;

        m_animationController = GetComponent<AnimationController>();
        m_animationController.OnDefensiveAnimationPlaying += OnDefensiveAnimationPlaying;
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
    #region Combat
    public void OnParryPerformed()
    {
        m_defensiveAnimationPlaying = true;
        m_animationController?.PlayDefensiveAnimation(AnimationType.Parry);
    }
    public void OnDodgePerformed() 
    {
        m_defensiveAnimationPlaying = true;
        m_animationController?.PlayDefensiveAnimation(AnimationType.Dodge);
    }
    void OnDefensiveAnimationPlaying(bool value) 
    {
        m_defensiveAnimationPlaying = value;
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
    public void OnCombatContextChanged(CombatContext ctx)
    {
        OnContextChanged?.Invoke(ctx);
    }
    private void CombatStarted()
    {

    }
    protected virtual void CombatEnded(CombatResult result) 
    {
        ClearStatusEffects();
    }
    public void SubmitAction(CombatActor source,
        CombatActor target, CombatAction action)
    {
        m_combatManager.Action.SubmitAction(source,
            target, action);
    }
    public bool SetActionContext(ActionContext ctx)
    {
        ctx.Source = this;
        m_combatManager.
            Action.
            SubmitAction(ctx.Source,
            ctx.Target, ctx.Action);
        
        return false;
    }
    protected void SetActionProvider(IActionProvider provider)
    {
        m_actionProvider = provider;
    }
    protected void SetReactionProvider(IReactionProvider provider) 
    {
        m_reactionProvider = provider;
    }

    // called by CombatManager
    public void PlayAction(ActionContext ctx, Action onComplete)
    {
        m_currentContext = ctx;
        m_onActionComplete = onComplete;

        if (ctx.Source == ctx.Target) 
        {
            Anim_ActionFinished();
            return;
        }
        if (ctx.Action.animationClip == null) 
        {
            Debug.LogWarning("AnimationClip is NULL");
            Anim_CloseWindow();
            InvokeAndClearOnComplete();
            return;
        }


        m_animationController.PlayActionAnimation(ctx.Action.animationClip);

        // fallback if the animator never calls Anim_AttackFinished
        if (m_actionTimeout != null)
            StopCoroutine(m_actionTimeout);

        float clipLength = ctx.Action.animationClip.length;
        m_actionTimeout = StartCoroutine(ActionTimeout(clipLength + 0.75f));
    }
    // called by animation clip
    public void Anim_CloseWindow()
    {
        //m_combatManager.ReactiveWindow.Close(m_currentContext);
        m_combatManager.Action.ClosePrompt();
    }
    // called by animator
    public void Anim_OpenWindow(string promptKey)
    {
        //m_currentContext.PromptKey = promptKey;
        //m_combatManager.ReactiveWindow.Open(m_currentContext);
        m_combatManager.Action.OpenPrompt(promptKey);
        
    }
    // called by animation clip
    public void Anim_ActionFinished()
    {
        //InvokeAndClearOnComplete();
        m_combatManager.Action.NotifyActionFinished(this);
    }
    private void InvokeAndClearOnComplete()
    {
        try
        {
            m_onActionComplete?.Invoke();
            OnActionFinished?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"{name} PlayAction: Exception while invoking completion: {ex}");
        }
        finally
        {
            m_onActionComplete = null;
            m_currentContext = null;
        }
    }
    private IEnumerator ActionTimeout(float time)
    {
        yield return new WaitForSeconds(time);

        if (m_onActionComplete != null)
        {
            Debug.LogWarning($"{name} Action timeout fallback triggered");
            InvokeAndClearOnComplete();
        }
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
        // Always instantiate a new instance to avoid shared ScriptableObject state
        ActorStatusEffect instance = Instantiate(effect);
        instance.Initialize(this);

        // Try to find an existing effect of the same type AND same display name.
        // This allows different assets that share the same concrete class
        // (e.g. TickStatusEffect) but represent different effects (Burn vs Poison)
        // to coexist separately.
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
            // if not stackable, do nothing
            return;
        }

        // If no existing effect, add the new instance
        m_statusEffects.Add(instance);
        OnStatusEffectsChanged?.Invoke(m_statusEffects);
    }
    #endregion

}