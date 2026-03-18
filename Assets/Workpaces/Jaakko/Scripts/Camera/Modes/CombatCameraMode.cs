using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;

public class CombatCameraMode : ICameraMode
{
    private CombatManager m_combatManager;
    private CameraManager m_cameraManager;
    private GameManager m_gameManager;
    private CinemachineCamera m_camera;
    private CameraPresetsConfig m_presets;

    private CameraTarget m_currentTarget;
    private CameraPresetData m_currentPreset;
    private CameraPresetType m_currentPresetType;

    private bool m_isAnimationControlled;
    private Dictionary<CombatActor, CameraTarget> m_actorTargets;

    // Pause state tracking
    private bool m_wasPausedLastFrame = false;

    public CombatCameraMode(CombatManager combatManager, CameraManager cameraManager, GameManager gameManager)
    {
        m_combatManager = combatManager;
        m_cameraManager = cameraManager;
        m_gameManager = gameManager;
        m_camera = cameraManager.Camera;
        m_actorTargets = new Dictionary<CombatActor, CameraTarget>();

        Debug.Log("CombatCameraMode: Constructor called");

        // Load presets from Resources or assign via dependency injection
        m_presets = Resources.Load<CameraPresetsConfig>("CameraPresets");
        if (m_presets == null)
        {
            Debug.LogError("CombatCameraMode: CameraPresetsConfig not found in Resources folder!");
        }
        else
        {
            Debug.Log("CombatCameraMode: Loaded CameraPresetsConfig successfully");
        }
    }

    public bool CanEnter()
    {
        bool canEnter = m_gameManager.State == GameState.Combat;
        Debug.Log($"CombatCameraMode.CanEnter: {canEnter} (GameState: {m_gameManager.State})");
        return canEnter;
    }

    public void Enter()
    {
        Debug.Log("=== Combat Camera Mode: ENTER ===");

        // Subscribe to combat events
        CombatEvents.OnTurnStarted += OnTurnStarted;
        CombatEvents.OnActionSubmitted += OnActionSubmitted;
        CombatEvents.OnActionResolved += OnActionResolved;
        CombatEvents.OnReactionWindowOpened += OnReactionWindowOpened;
        CombatEvents.OnCombatActorsChanged += OnCombatActorsChanged;

        // Subscribe to animation events
        CameraAnimationEvents.OnTargetChanged += OnAnimationTargetChanged;
        CameraAnimationEvents.OnZoomChanged += OnAnimationZoomChanged;

        // Initialize actor targets
        RefreshActorTargets();

        // Find and set initial target
        var initialActor = FindInitialTarget();
        if (initialActor != null)
        {
            SetTarget(initialActor);
        }
        else
        {
            Debug.LogWarning("CombatCameraMode: No initial target found!");
        }

        // Set initial preset
        ApplyPreset(CameraPresetType.TurnStart);
    }

    public void Exit()
    {
        Debug.Log("=== Combat Camera Mode: EXIT ===");

        // Unsubscribe from events
        CombatEvents.OnTurnStarted -= OnTurnStarted;
        CombatEvents.OnActionSubmitted -= OnActionSubmitted;
        CombatEvents.OnActionResolved -= OnActionResolved;
        CombatEvents.OnReactionWindowOpened -= OnReactionWindowOpened;
        CombatEvents.OnCombatActorsChanged -= OnCombatActorsChanged;

        CameraAnimationEvents.OnTargetChanged -= OnAnimationTargetChanged;
        CameraAnimationEvents.OnZoomChanged -= OnAnimationZoomChanged;

        m_currentTarget = null;
        m_actorTargets.Clear();
    }

    public void Update(float dt)
    {
#if UNITY_EDITOR
        // In editor, detect pause state changes
        bool isPaused = UnityEditor.EditorApplication.isPaused;
        
        if (isPaused != m_wasPausedLastFrame)
        {
            if (isPaused)
            {
                Debug.Log("CombatCameraMode: Game paused - Camera manipulation enabled");
            }
            else
            {
                Debug.Log("CombatCameraMode: Game resumed - Reapplying current preset");
                // When unpausing, reapply the current preset to override any manual changes
                if (m_currentPreset != null)
                {
                    UpdateCinemachineFollow();
                    ApplyZoomFromTarget();
                }
            }
            m_wasPausedLastFrame = isPaused;
        }

        // When paused, still update Cinemachine but skip our preset logic
        if (isPaused)
        {
            // Force Cinemachine brain to update so we see changes in Game view
            var brain = UnityEngine.Camera.main?.GetComponent<Unity.Cinemachine.CinemachineBrain>();
            if (brain != null)
            {
                brain.ManualUpdate();
            }
            return;
        }
#endif

        if (m_currentTarget == null || m_currentPreset == null) return;

        // Skip smooth movement if animation is controlling camera
        if (m_isAnimationControlled)
        {
            UpdateAnimationControlled(dt);
        }
        else
        {
            UpdatePresetControlled(dt);
        }
    }

    private void UpdatePresetControlled(float dt)
    {
        // Update follow target
        UpdateCinemachineFollow();

        // Apply zoom from target if available
        ApplyZoomFromTarget();
    }

    private void UpdateAnimationControlled(float dt)
    {
        // Camera follows animated target directly
        UpdateCinemachineFollow();
        ApplyZoomFromTarget();
    }

    private void UpdateCinemachineFollow()
    {
        if (m_currentTarget == null || m_currentPreset == null) return;

        var follow = m_camera.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            m_camera.Follow = m_currentTarget.transform;
            follow.FollowOffset = m_currentPreset.positionOffset;
            
            // Set damping for each axis individually
            follow.TrackerSettings.PositionDamping = new Vector3(
                m_currentPreset.followDamping,
                m_currentPreset.followDamping,
                m_currentPreset.followDamping
            );
        }
    }

    private void ApplyZoomFromTarget()
    {
        if (m_currentTarget == null || m_currentPreset == null) return;

        var followZoom = m_camera.GetComponent<CinemachineFollowZoom>();
        if (followZoom != null)
        {
            // Apply zoom settings from preset
            followZoom.Width = m_currentPreset.width * m_currentTarget.zoomMultiplier;
            followZoom.Damping = m_currentPreset.zoomDamping;
            
            // Set FOV range
            followZoom.FovRange = m_currentPreset.fovRange;
        }
    }

    private void ApplyPreset(CameraPresetType presetType)
    {
        if (m_presets == null)
        {
            Debug.LogError("CombatCameraMode: Cannot apply preset - m_presets is null");
            return;
        }

        m_currentPresetType = presetType;
        m_currentPreset = m_presets.GetPreset(presetType);
        m_isAnimationControlled = false;

        if (m_currentPreset == null)
        {
            Debug.LogError($"CombatCameraMode: Preset {presetType} is null!");
            return;
        }

        Debug.Log($"Applied camera preset: {presetType} - {m_currentPreset.presetName}");
        
        // Immediately apply the preset
        UpdateCinemachineFollow();
        ApplyZoomFromTarget();
    }

    private void SetTarget(CombatActor actor)
    {
        if (actor == null)
        {
            Debug.LogWarning("CombatCameraMode: Attempted to set null actor as target");
            return;
        }

        if (m_actorTargets.TryGetValue(actor, out CameraTarget target))
        {
            m_currentTarget = target;
            Debug.Log($"Camera target set to: {actor.name}");
        }
        else
        {
            Debug.LogWarning($"No camera target found for actor: {actor.name}");
        }
    }

    private CombatActor FindInitialTarget()
    {
        if (m_actorTargets.Count == 0)
        {
            Debug.LogWarning("CombatCameraMode: No actor targets available");
            return null;
        }

        // Prefer player actors
        foreach (var kvp in m_actorTargets)
        {
            if (kvp.Key.IsPlayer && !kvp.Key.IsDead)
            {
                Debug.Log($"Found initial target (player): {kvp.Key.name}");
                return kvp.Key;
            }
        }

        // Fallback to any actor
        var firstActor = m_actorTargets.Keys.First();
        Debug.Log($"Found initial target (fallback): {firstActor.name}");
        return firstActor;
    }

    private void RefreshActorTargets()
    {
        m_actorTargets.Clear();
        var targets = GameObject.FindObjectsByType<CameraTarget>(FindObjectsSortMode.None);

        Debug.Log($"CombatCameraMode: Searching for CameraTarget components...");
        
        foreach (var target in targets)
        {
            if (target.Actor != null)
            {
                m_actorTargets[target.Actor] = target;
                Debug.Log($"  - Found target for: {target.Actor.name}");
            }
            else
            {
                Debug.LogWarning($"  - CameraTarget on {target.gameObject.name} has no CombatActor parent!");
            }
        }

        Debug.Log($"CombatCameraMode: Found {m_actorTargets.Count} camera targets total");
    }

    // Make current preset publicly accessible for editor tools
    public CameraPresetType CurrentPresetType => m_currentPresetType;

    #region Event Handlers

    private void OnTurnStarted(CombatActor actor)
    {
        Debug.Log($"CombatCameraMode: OnTurnStarted - {actor.name}");
        SetTarget(actor);
        ApplyPreset(CameraPresetType.TurnStart);
    }

    private void OnActionSubmitted(ActionContext ctx)
    {
        Debug.Log($"CombatCameraMode: OnActionSubmitted - {ctx.Source.name}");
        SetTarget(ctx.Source);
        ApplyPreset(CameraPresetType.ActionExecution);
        m_isAnimationControlled = true; // Allow animations to take control
    }

    private void OnActionResolved(ActionContext ctx, ActionResult result)
    {
        Debug.Log("CombatCameraMode: OnActionResolved");
        m_isAnimationControlled = false;
        ApplyPreset(CameraPresetType.ActionSelection);
    }

    private void OnReactionWindowOpened(ActionContext ctx)
    {
        Debug.Log($"CombatCameraMode: OnReactionWindowOpened");
        // Focus on the reacting target
        if (ctx.Target != null)
        {
            SetTarget(ctx.Target);
            ApplyPreset(CameraPresetType.ReactionWindow);
        }
    }

    private void OnCombatActorsChanged(System.Collections.Generic.List<CombatActor> actors)
    {
        Debug.Log($"CombatCameraMode: OnCombatActorsChanged - {actors.Count} actors");
        RefreshActorTargets();
    }

    private void OnAnimationTargetChanged(CameraTarget target)
    {
        m_currentTarget = target;
        Debug.Log($"CombatCameraMode: Animation target changed to {target.Actor.name}");
    }

    private void OnAnimationZoomChanged(float zoom)
    {
        if (m_currentTarget != null)
        {
            m_currentTarget.zoomMultiplier = zoom;
            Debug.Log($"CombatCameraMode: Zoom changed to {zoom}");
        }
    }

    #endregion
}