using UnityEngine;

public class FollowActorCameraMode : ICameraMode
{
    private CombatManager m_combatManager;
    private ActorManager m_actorManager;
    private CameraManager m_cameraManager;
    private GameManager m_game;
    public FollowActorCameraMode(ActorManager actorManager, CombatManager combatManager
        , CameraManager cameraManager, GameManager game) 
    {
        m_combatManager = combatManager;
        m_actorManager = actorManager;
        m_cameraManager = cameraManager;
        m_game = game;
    }
    public bool CanEnter() 
    {
        return m_game.State == GameState.None;
    }
    public void Enter() 
    {
        OnActorChanged(m_actorManager.CurrentControlled);
        m_actorManager.OnActorControlChanged += OnActorChanged;    
    }
    public void Exit() 
    {
        m_actorManager.OnActorControlChanged -= OnActorChanged;
    }
    public void Update(float dt) 
    {
        
    }
    private void OnActorChanged(Actor actor) 
    {
        if (actor == null) return;

        m_cameraManager.Camera.Target.LookAtTarget = actor.TrackingTarget;
        m_cameraManager.Camera.Target.TrackingTarget = actor.TrackingTarget;
    }
}
