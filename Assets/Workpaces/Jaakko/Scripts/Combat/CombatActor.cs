using System;
using System.Collections;
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

[RequireComponent(typeof(Actor))]
public class CombatActor : MonoBehaviour, IActorComponent
{    
    public bool IsDead { get; private set; }
    public bool IsPlayer { get; private set; }
    private Actor m_actor;
    public Actor Actor => m_actor;

    private CombatManager m_combatManager;

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


    private UIController m_uiController; // temp
    private AnimationController m_animationController; // temp
    [SerializeField] private GameObject m_visual; // temp 

    [Header("AI")]
    [SerializeField] private AiReactionSettings m_reactionSettings;


    #region IActorComponent
    public bool Initialize(GameManager game)
    {
        m_actor = GetComponent<Actor>();

        m_combatManager = game.Resolve<CombatManager>();
        m_uiController = game.Resolve<UIManager>().Controller;

        IsPlayer = m_actor.IsPlayable;

        if (IsPlayer)
        {
            SetActionProvider(new PlayerActionProvider(m_combatManager,
                m_uiController));
            SetReactionProvider(new PlayerReactionProvider());            
        }
        else
        {
            SetActionProvider(new AIActionProvider());
            SetReactionProvider(new AIReactionProvider(m_reactionSettings));
        }
        return true;
    }
    void OnHealthChanged(float value)
    {
        if (value <= 0f)
        {
            IsDead = true;
            m_combatManager.OnActorDied(this);

            if (m_visual) // temp
                m_visual.SetActive(false); // temp
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

        m_animationController = GetComponent<AnimationController>();
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
    public void OnTurnStart()
    {
        if (m_statusEffects == null || m_statusEffects.Count == 0) return;

        List<ActorStatusEffect> effects = new List<ActorStatusEffect> (m_statusEffects);
        foreach (var e in effects) 
        {
            if (e == null) return;
                e.TurnStart();
        }
            
    }
    public void OnTurnEnd()
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
        OnStatusEffectsChanged?.Invoke(m_statusEffects);
    }
    public void OnCombatContextChanged(CombatContext ctx)
    {
        OnContextChanged?.Invoke(ctx);
    }
    public void OnCombatStarted()
    {

    }
    public void OnCombatFinished()
    {
        foreach (var effect in m_statusEffects)
            Destroy(effect);

        m_statusEffects.Clear();

        if (m_visual)
            m_visual.SetActive(true);

        // have combatmanager handle obj destruction
        if (!IsPlayer)
            Destroy(gameObject); // add pooling?
        if (IsDead)
            IsDead = false;
        Health.ApplyHealth(Health.MaxHealth);
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
    private void SetReactionProvider(IReactionProvider provider) 
    {
        m_reactionProvider = provider;
    }

    // called by CombatManager
    public void PlayAction(ActionContext ctx, Action onComplete)
    {
        m_currentContext = ctx;
        m_onActionComplete = onComplete;

        if (m_animationController == null || ctx.Action?.animationClip == null)
        {
            Debug.LogWarning($"PlayAction: On {name} Animator or Clip is NULL");
            InvokeAndClearOnComplete();
            return;
        }
        m_animationController?.PlayActionAnimation(ctx.Action.animationClip);

        // fallback if the animator never calls Anim_AttackFinished
        if (m_actionTimeout != null)
            StopCoroutine(m_actionTimeout);
        float clipLength = ctx.Action.animationClip.length;
        m_actionTimeout = StartCoroutine(ActionTimeout(clipLength + 0.75f));
    }
    // called by animation clip
    public void Anim_CloseWindow()
    {
        m_combatManager.ReactiveWindow.Close(m_currentContext);
    }
    // called by animator
    public void Anim_OpenWindow(string promptKey)
    {
        m_currentContext.PromptKey = promptKey;
        m_combatManager.ReactiveWindow.Open(m_currentContext);
    }
    // called by animation clip
    public void Anim_AttackFinished()
    {
        InvokeAndClearOnComplete();
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