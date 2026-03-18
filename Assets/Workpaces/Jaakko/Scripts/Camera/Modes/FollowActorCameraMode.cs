using UnityEngine;
using Unity.Cinemachine;

public class FollowActorCameraMode : ICameraMode
{
    private CombatManager m_combatManager;
    private ActorManager m_actorManager;
    private CameraManager m_cameraManager;
    private GameManager m_game;
    private CinemachineCamera m_camera;
    private CameraPresetsConfig m_presets;
    private CameraPresetData m_explorationPreset;

    public FollowActorCameraMode(ActorManager actorManager, CombatManager combatManager, 
        CameraManager cameraManager, GameManager game) 
    {
        m_combatManager = combatManager;
        m_actorManager = actorManager;
        m_cameraManager = cameraManager;
        m_game = game;
        m_camera = cameraManager.Camera;

        // Load presets
        m_presets = Resources.Load<CameraPresetsConfig>("CameraPresets");
        if (m_presets == null)
        {
            Debug.LogWarning("CameraPresetsConfig not found in Resources folder!");
        }
        else
        {
            m_explorationPreset = m_presets.exploration;
        }
    }

    public bool CanEnter() 
    {
        bool canEnter = m_game.State == GameState.None;
        //Debug.Log($"FollowActorCameraMode.CanEnter: {canEnter} (GameState: {m_game.State})");
        return canEnter;
    }

    public void Enter() 
    {
        Debug.Log("Follow Actor Camera Mode: Enter");
        
        OnActorChanged(m_actorManager.CurrentControlled);
        m_actorManager.OnActorControlChanged += OnActorChanged;
        
        // Apply exploration preset
        ApplyExplorationPreset();
    }

    public void Exit() 
    {
        Debug.Log("Follow Actor Camera Mode: Exit");
        m_actorManager.OnActorControlChanged -= OnActorChanged;
    }

    public void Update(float dt) 
    {
        // Continuously apply preset settings for smooth following
        if (m_explorationPreset != null && m_actorManager.CurrentControlled != null)
        {
            UpdateCameraSettings();
        }
    }

    private void OnActorChanged(Actor actor) 
    {
        if (actor == null) return;

        // Set follow target
        m_camera.Follow = actor.TrackingTarget;
        
        Debug.Log($"Camera following actor: {actor.name}");
    }

    private void ApplyExplorationPreset()
    {
        if (m_explorationPreset == null) return;

        var follow = m_camera.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            follow.FollowOffset = m_explorationPreset.positionOffset;
            follow.TrackerSettings.PositionDamping = new Vector3(
                m_explorationPreset.followDamping,
                m_explorationPreset.followDamping,
                m_explorationPreset.followDamping
            );
        }

        var followZoom = m_camera.GetComponent<CinemachineFollowZoom>();
        if (followZoom != null)
        {
            followZoom.Width = m_explorationPreset.width;
            followZoom.Damping = m_explorationPreset.zoomDamping;
            followZoom.FovRange = m_explorationPreset.fovRange;
        }

        Debug.Log("Applied exploration camera preset");
    }

    private void UpdateCameraSettings()
    {
        // Ensure settings stay applied (in case they get changed)
        var follow = m_camera.GetComponent<CinemachineFollow>();
        if (follow != null && m_camera.Follow != m_actorManager.CurrentControlled.TrackingTarget)
        {
            m_camera.Follow = m_actorManager.CurrentControlled.TrackingTarget;
        }
    }
}
