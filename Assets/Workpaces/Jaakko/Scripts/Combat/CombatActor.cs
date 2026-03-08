using System;
using System.Collections;
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

    public event Action<CombatContext> OnContextChanged;

    private IActionProvider m_actionProvider;
    public IActionProvider ActionProvider => m_actionProvider;

    private HealthComponent m_health;
    public HealthComponent Health => m_health;

    private UIController m_uiController;

    private Animator m_animator;


    #region IActorComponent
    public bool Initialize(GameManager game)
    {
        m_actor = GetComponent<Actor>();
        m_animator = GetComponent<Animator>();

        m_combatManager = game.Resolve<CombatManager>();
        m_uiController = game.Resolve<UIManager>().Controller;

        IsPlayer = m_actor.IsPlayable;

        if (IsPlayer)
        {
            SetActionProvider(new PlayerActionProvider(m_combatManager,
                m_uiController));
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
    public void OnCombatContextChanged(CombatContext ctx)
    {
        OnContextChanged?.Invoke(ctx);
    }
    public void OnCombatStarted()
    {

    }
    public void OnCombatFinished()
    {
        // have combatmanager handle obj destruction
        if (!IsPlayer)
            Destroy(gameObject); // add pooling?
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

    private ActionContext m_currentContext;
    private Action m_onActionComplete;
    private AnimationController m_animationController;
    private Coroutine m_actionTimeout;
    public void PlayAction(ActionContext ctx, Action onComplete)
    {
        m_currentContext = ctx;
        m_onActionComplete = onComplete;
        
        if (m_animator == null || ctx.Action?.animationClip == null)
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
        m_actionTimeout = StartCoroutine(ActionTimeout(clipLength + 0.25f));
    }
    // called by animation clip
    public void Anim_CloseWindow()
    {
        m_combatManager.CloseReactiveWindow(m_currentContext);
    }
    // called by animator
    public void Anim_OpenWindow() 
    {
        m_combatManager.OpenReactiveWindow(m_currentContext);
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
}