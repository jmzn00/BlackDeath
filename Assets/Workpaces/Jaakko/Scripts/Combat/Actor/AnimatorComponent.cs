using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorComponent : MonoBehaviour, IActorComponent
{
    public bool Initialize(GameManager game) {  return true; }    
    public bool Dispose() 
    {
        m_combatActor.OnPlayRequested -= PlayCombatAction;
        return true;
    }

    public void LoadData(ActorSaveData data) { }
    public void SaveData(ActorSaveData data) { }
    public void SetInputSource(IInputSource source) { }

    [Header("Animations")]
    [Header("Out Of Combat")]
    [SerializeField] private AnimationClip m_idle;
    [SerializeField] private AnimationClip m_walk;
    [Header("Combat")]
    [SerializeField] private AnimationClip m_parry;
    [SerializeField] private AnimationClip m_dodge;
    [SerializeField] private AnimationClip m_transition;

    public AnimationClip TransitionClip => m_transition;

    public event Action OnActionAnimationFinished;

    private CombatActor m_combatActor;
    private Animator m_animator;
    public void OnActorComponentsInitialized(Actor actor) 
    {
        m_combatActor = actor.Get<CombatActor>();
        m_animator = GetComponent<Animator>();
        m_combatActor.OnPlayRequested += PlayCombatAction;
    }
    // from m_combatActor.OnPlayRequested
    public void PlayCombatAction(AnimationClip clip) 
    {
        m_animator.Play(clip.name, 0, 0f);
    }
    public void PlayTransition() 
    {
        m_animator.Play(m_transition.name, 0, 0f);
    }
    // called by animation
    public void Anim_OpenWindow(string promptKey) 
    {
        m_combatActor.OpenWindow(promptKey);
    }
    // called by animation
    public void Anim_CloseWindow() 
    {
        if (m_combatActor == null)
        {
            Debug.LogWarning("Combat Actor is NULL on AnimatiorComponent");
            return;
        }

        m_combatActor.CloseWindow();
    }
    // called by animation
    public void Anim_Finished() 
    {
        if (m_combatActor == null) 
        {
            Debug.LogWarning("Combat Actor is NULL on AnimatiorComponent");
            return;
        }
        OnActionAnimationFinished?.Invoke();
        m_animator.Play(m_idle.name, 0, 0f);
    }    
}
