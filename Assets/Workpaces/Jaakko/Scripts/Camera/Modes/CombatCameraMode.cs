using UnityEngine;

public class CombatCameraMode : ICameraMode
{
    private CombatManager m_combatManager;
    private CameraManager m_cameraManager;
    private GameManager m_game;


    public CombatCameraMode(CombatManager combatManager, CameraManager cameraManager, GameManager game) 
    {
        m_combatManager = combatManager;
        m_cameraManager = cameraManager;
        m_game = game;
    }
    public bool CanEnter() 
    {
        return m_game.State == GameState.Combat;
    }
    public void Enter() 
    {
        //OnActorChanged(m_combatManager.Actor);
        //m_combatManager.OnCurrentActorChanged += OnActorChanged;        
    }
    public void Exit() 
    {
        //m_combatManager.OnCurrentActorChanged -= OnActorChanged;
    }
    public void Update(float dt) 
    {

    }
    private void OnActorChanged(Actor actor) 
    {
        if (actor == null) 
        {
            Debug.Log("Actor is NULL");
            return;
        }
            
        if (actor.TrackingTarget == null) 
        {
            Debug.Log("Tracking Target is NULL");
            return;
        }
        m_cameraManager.Camera.Target.TrackingTarget = actor.TrackingTarget;
        m_cameraManager.Camera.Target.LookAtTarget = actor.TrackingTarget;
    }
}
