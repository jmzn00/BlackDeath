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
        // subscribe to events and drive director via presets
        CombatEvents.OnTurnStarted += OnTurnStarted;
        CombatEvents.OnTargetSelected += OnTargetSelected;
        CombatEvents.OnActionExecuting += OnActionExecuting;
    }

    public void Exit()
    {
        CombatEvents.OnTurnStarted -= OnTurnStarted;
        CombatEvents.OnTargetSelected -= OnTargetSelected;
        CombatEvents.OnActionExecuting -= OnActionExecuting;
    }

    public void Update(float dt)
    {
    }

    private void OnTurnStarted(CombatActor actor)
    {
        if (actor == null) return;

        // Use director presets to position anchor — do NOT change vcam Follow/LookAt directly.
        if (actor.IsPlayer)
            m_cameraManager.TransitionToPreset("CC_PlayerTurn", actor, null);
        else
            m_cameraManager.TransitionToPreset("CC_EnemyPrepare", actor, null);
    }

    private void OnTargetSelected(CombatActor source, CombatActor target)
    {
        // focus between source and target via preset
        m_cameraManager.TransitionToPreset("CC_SelectTarget", source, target);
    }

    private void OnActionExecuting(CombatActor source, CombatActor target, CombatAction action)
    {
        // dramatic preset for execution
        m_cameraManager.TransitionToPreset("CC_ActionExecute", source, target);
    }
}
