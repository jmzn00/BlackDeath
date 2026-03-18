#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;
using System.Linq;

public class CameraPresetEditorWindow : EditorWindow
{
    private CameraPresetsConfig m_config;
    private CameraPresetType m_selectedPreset = CameraPresetType.ActionSelection;
    private CinemachineCamera m_camera;
    private bool m_isPlayMode = false;
    
    // Track active preset
    private CameraPresetType? m_activePresetType = null;
    private string m_activeCameraMode = "Unknown";
    
    // Pause state
    private bool m_isPaused = false;
    private bool m_autoRefreshGameView = true;
    
    // Brain state tracking
    private CinemachineBrain m_brain;
    private CinemachineBrain.UpdateMethods m_originalUpdateMethod;

    [MenuItem("Tools/Camera/Preset Editor")]
    public static void ShowWindow()
    {
        GetWindow<CameraPresetEditorWindow>("Camera Preset Editor");
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.pauseStateChanged += OnPauseStateChanged;
        RefreshCamera();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.pauseStateChanged -= OnPauseStateChanged;
        
        // Restore brain update method if changed
        RestoreBrainUpdateMethod();
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        m_isPlayMode = state == PlayModeStateChange.EnteredPlayMode;
        RefreshCamera();
        
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            RestoreBrainUpdateMethod();
        }
    }

    private void OnPauseStateChanged(PauseState state)
    {
        m_isPaused = state == PauseState.Paused;
        
        if (m_isPaused)
        {
            Debug.Log("Camera Preset Editor: Game paused - You can now manipulate the camera");
            SetupBrainForManualUpdate();
        }
        else
        {
            Debug.Log("Camera Preset Editor: Game resumed");
            RestoreBrainUpdateMethod();
        }
        
        Repaint();
    }

    private void SetupBrainForManualUpdate()
    {
        m_brain = Camera.main?.GetComponent<CinemachineBrain>();
        if (m_brain != null)
        {
            // Store original update method
            m_originalUpdateMethod = m_brain.UpdateMethod;
            
            // Set to manual update
            m_brain.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
            
            Debug.Log($"CinemachineBrain set to ManualUpdate mode (was {m_originalUpdateMethod})");
        }
    }

    private void RestoreBrainUpdateMethod()
    {
        if (m_brain != null && m_brain.UpdateMethod == CinemachineBrain.UpdateMethods.ManualUpdate)
        {
            m_brain.UpdateMethod = m_originalUpdateMethod;
            Debug.Log($"CinemachineBrain restored to {m_originalUpdateMethod} mode");
            m_brain = null;
        }
    }

    private void OnEditorUpdate()
    {
        if (Application.isPlaying)
        {
            UpdateActivePresetInfo();
            
            // Auto-refresh game view when paused
            if (m_isPaused && m_autoRefreshGameView)
            {
                if (m_brain != null && m_brain.UpdateMethod == CinemachineBrain.UpdateMethods.ManualUpdate)
                {
                    // Update the virtual camera state first
                    if (m_camera != null)
                    {
                        // Force the virtual camera to recalculate
                        m_camera.InternalUpdateCameraState(Vector3.up, 0.01f);
                    }
                    
                    // Then update the brain to apply it to the actual camera
                    m_brain.ManualUpdate();
                }
            }
            
            Repaint(); // Refresh the window
        }
    }

    private void UpdateActivePresetInfo()
    {
        // Try to get the camera manager from Services
        var cameraManager = Services.Get<CameraManager>();
        if (cameraManager != null)
        {
            // Use reflection to get the current mode
            var modeField = typeof(CameraManager).GetField("m_mode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (modeField != null)
            {
                var currentMode = modeField.GetValue(cameraManager);
                if (currentMode != null)
                {
                    m_activeCameraMode = currentMode.GetType().Name;

                    // If it's combat camera mode, try to get the current preset type
                    if (currentMode is CombatCameraMode combatMode)
                    {
                        var presetTypeField = typeof(CombatCameraMode).GetField("m_currentPresetType",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (presetTypeField != null)
                        {
                            m_activePresetType = (CameraPresetType)presetTypeField.GetValue(combatMode);
                        }
                    }
                    else
                    {
                        // In exploration mode
                        m_activePresetType = CameraPresetType.Exploration;
                    }
                }
            }
        }
    }

    private void RefreshCamera()
    {
        m_camera = FindFirstObjectByType<CinemachineCamera>();
        if (m_camera != null)
        {
            Debug.Log($"Camera Preset Editor: Found camera '{m_camera.name}'");
        }
        else
        {
            Debug.LogWarning("Camera Preset Editor: No CinemachineCamera found in scene");
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Camera Preset Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Show active preset info in play mode
        if (Application.isPlaying)
        {
            DrawActivePresetInfo();
            EditorGUILayout.Space();
            
            // Pause controls
            DrawPauseControls();
            EditorGUILayout.Space();
        }

        // Config field
        EditorGUI.BeginChangeCheck();
        m_config = (CameraPresetsConfig)EditorGUILayout.ObjectField(
            "Preset Config",
            m_config,
            typeof(CameraPresetsConfig),
            false
        );
        if (EditorGUI.EndChangeCheck() && m_config != null)
        {
            Debug.Log($"Loaded preset config: {m_config.name}");
        }

        if (m_config == null)
        {
            EditorGUILayout.HelpBox("Please assign a CameraPresetsConfig asset", MessageType.Warning);
            if (GUILayout.Button("Find CameraPresets in Resources"))
            {
                m_config = Resources.Load<CameraPresetsConfig>("CameraPresets");
                if (m_config == null)
                {
                    Debug.LogError("Could not find 'CameraPresets' in Resources folder!");
                }
            }
            return;
        }

        EditorGUILayout.Space();

        // Preset selection
        EditorGUI.BeginChangeCheck();
        m_selectedPreset = (CameraPresetType)EditorGUILayout.EnumPopup("Preset Type", m_selectedPreset);
        
        // Show indicator if this is the active preset
        if (Application.isPlaying && m_activePresetType.HasValue && m_activePresetType.Value == m_selectedPreset)
        {
            var rect = GUILayoutUtility.GetLastRect();
            rect.x = rect.width - 80;
            rect.width = 80;
            GUI.Label(rect, "⚡ ACTIVE", EditorStyles.boldLabel);
        }
        
        if (EditorGUI.EndChangeCheck() && Application.isPlaying && m_activePresetType.HasValue)
        {
            // Quick switch button appears when selecting different preset
            if (m_selectedPreset != m_activePresetType.Value)
            {
                EditorGUILayout.HelpBox($"Currently viewing: {m_selectedPreset}\nActive preset: {m_activePresetType.Value}", MessageType.Info);
            }
        }

        EditorGUILayout.Space();

        // Camera status
        if (m_camera == null)
        {
            EditorGUILayout.HelpBox("No CinemachineCamera found in scene", MessageType.Error);
            if (GUILayout.Button("Refresh Camera"))
            {
                RefreshCamera();
            }
            return;
        }

        EditorGUILayout.HelpBox($"Camera: {m_camera.name}", MessageType.Info);

        // Play mode controls
        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            
            if (m_isPaused)
            {
                EditorGUILayout.HelpBox(
                    "⏸ PAUSED: Manipulate the Cinemachine Camera freely!\n" +
                    "• Adjust components in Inspector\n" +
                    "• Changes update automatically in Game view\n" +
                    "• Capture when satisfied", 
                    MessageType.Info
                );
                
                // Manual refresh button
                if (!m_autoRefreshGameView && GUILayout.Button("🔄 Refresh Game View", GUILayout.Height(30)))
                {
                    ForceRefreshCamera();
                }
                
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "▶ PLAYING: Camera is controlled by the system.\n" +
                    "Pause the game to manipulate camera settings.", 
                    MessageType.Warning
                );
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📷 CAPTURE Current Camera State", GUILayout.Height(40)))
            {
                CaptureCurrentCameraState();
            }
            
            // Quick button to switch to active preset for editing
            if (m_activePresetType.HasValue && m_activePresetType.Value != m_selectedPreset)
            {
                if (GUILayout.Button($"Switch to\n{m_activePresetType.Value}", GUILayout.Height(40), GUILayout.Width(100)))
                {
                    m_selectedPreset = m_activePresetType.Value;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (GUILayout.Button("✓ Apply Preset to Camera", GUILayout.Height(30)))
            {
                ApplyPresetToCamera();
            }
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Enter Play Mode to manipulate and capture camera settings in real-time.", 
                MessageType.Warning
            );
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // Preset data editor
        DrawPresetData();
    }

    private void ForceRefreshCamera()
    {
        if (m_camera == null)
        {
            Debug.LogWarning("Cannot refresh: No camera found");
            return;
        }

        // Ensure brain is in manual update mode
        if (m_brain == null || m_brain.UpdateMethod != CinemachineBrain.UpdateMethods.ManualUpdate)
        {
            SetupBrainForManualUpdate();
        }

        if (m_brain != null)
        {
            // CRITICAL: Update virtual camera first
            m_camera.InternalUpdateCameraState(Vector3.up, 0.01f);
            
            // Then update the brain
            m_brain.ManualUpdate();
            
            Debug.Log("✓ Force updated Cinemachine Camera and Brain");
        }

        // Force repaint all views
        SceneView.RepaintAll();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    private void DrawPauseControls()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Pause/Resume button
        EditorGUILayout.BeginHorizontal();
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.fontSize = 12;

        if (m_isPaused)
        {
            buttonStyle.normal.textColor = Color.green;
            if (GUILayout.Button("▶ Resume Game", buttonStyle, GUILayout.Height(30)))
            {
                EditorApplication.isPaused = false;
            }
        }
        else
        {
            buttonStyle.normal.textColor = Color.yellow;
            if (GUILayout.Button("⏸ Pause Game", buttonStyle, GUILayout.Height(30)))
            {
                EditorApplication.isPaused = true;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Auto-refresh toggle
        if (m_isPaused)
        {
            EditorGUILayout.Space(5);
            m_autoRefreshGameView = EditorGUILayout.ToggleLeft(
                "🔄 Auto-refresh Game View (recommended)", 
                m_autoRefreshGameView
            );
            
            if (!m_autoRefreshGameView)
            {
                EditorGUILayout.HelpBox("Manual refresh: Use the 'Refresh Game View' button", MessageType.Info);
            }
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawActivePresetInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 12;
        titleStyle.normal.textColor = m_isPaused ? Color.yellow : new Color(0.3f, 0.8f, 0.3f);
        
        string statusIcon = m_isPaused ? "⏸" : "▶";
        EditorGUILayout.LabelField($"{statusIcon} LIVE STATUS", titleStyle);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Camera Mode:", GUILayout.Width(100));
        EditorGUILayout.LabelField(m_activeCameraMode, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        if (m_activePresetType.HasValue)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active Preset:", GUILayout.Width(100));
            
            GUIStyle presetStyle = new GUIStyle(EditorStyles.boldLabel);
            presetStyle.normal.textColor = m_isPaused ? Color.yellow : Color.cyan;
            EditorGUILayout.LabelField(m_activePresetType.Value.ToString(), presetStyle);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawPresetData()
    {
        CameraPresetData preset = m_config.GetPreset(m_selectedPreset);
        if (preset == null) return;

        EditorGUILayout.LabelField("Preset Data", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();

        preset.presetName = EditorGUILayout.TextField("Preset Name", preset.presetName);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Follow Settings", EditorStyles.boldLabel);
        preset.positionOffset = EditorGUILayout.Vector3Field("Position Offset", preset.positionOffset);
        preset.followDamping = EditorGUILayout.Slider("Follow Damping", preset.followDamping, 0f, 5f);
        preset.transitionSpeed = EditorGUILayout.Slider("Transition Speed", preset.transitionSpeed, 0.1f, 10f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
        preset.rotation = EditorGUILayout.Vector3Field("Rotation (Euler)", preset.rotation);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Zoom Settings", EditorStyles.boldLabel);
        preset.width = EditorGUILayout.FloatField("Width", preset.width);
        preset.zoomDamping = EditorGUILayout.Slider("Zoom Damping", preset.zoomDamping, 0f, 2f);
        preset.fovRange = EditorGUILayout.Vector2Field("FOV Range (Min, Max)", preset.fovRange);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Camera Settings", EditorStyles.boldLabel);
        preset.fieldOfView = EditorGUILayout.FloatField("Field of View", preset.fieldOfView);
        preset.orthographicSize = EditorGUILayout.FloatField("Orthographic Size", preset.orthographicSize);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(m_config);
            Debug.Log($"Modified preset: {m_selectedPreset}");
        }
    }

    private void CaptureCurrentCameraState()
    {
        if (m_camera == null)
        {
            Debug.LogError("Cannot capture: No camera found");
            EditorUtility.DisplayDialog("Error", "No CinemachineCamera found in scene", "OK");
            return;
        }

        CameraPresetData preset = m_config.GetPreset(m_selectedPreset);
        if (preset == null)
        {
            Debug.LogError($"Cannot capture: Preset {m_selectedPreset} is null");
            return;
        }

        // Capture Follow component settings
        var follow = m_camera.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            preset.positionOffset = follow.FollowOffset;
            preset.followDamping = follow.TrackerSettings.PositionDamping.x;
            Debug.Log($"  Captured Follow Offset: {preset.positionOffset}");
            Debug.Log($"  Captured Follow Damping: {preset.followDamping}");
        }
        else
        {
            Debug.LogWarning("  No CinemachineFollow component found");
        }

        // Capture rotation
        preset.rotation = m_camera.transform.rotation.eulerAngles;
        Debug.Log($"  Captured Rotation: {preset.rotation}");

        // Capture Follow Zoom settings
        var followZoom = m_camera.GetComponent<CinemachineFollowZoom>();
        if (followZoom != null)
        {
            preset.width = followZoom.Width;
            preset.zoomDamping = followZoom.Damping;
            preset.fovRange = followZoom.FovRange;
            Debug.Log($"  Captured Width: {preset.width}");
            Debug.Log($"  Captured Zoom Damping: {preset.zoomDamping}");
            Debug.Log($"  Captured FOV Range: {preset.fovRange}");
        }
        else
        {
            Debug.LogWarning("  No CinemachineFollowZoom component found");
        }

        EditorUtility.SetDirty(m_config);
        Debug.Log($"✓ Successfully captured camera state for preset: {m_selectedPreset}");
        
        string message = $"Captured camera settings to preset:\n{m_selectedPreset}";
        if (m_activePresetType.HasValue)
        {
            message += $"\n\nCurrent active preset: {m_activePresetType.Value}";
            if (m_selectedPreset != m_activePresetType.Value)
            {
                message += $"\n\n⚠ Note: You captured to {m_selectedPreset}, but {m_activePresetType.Value} is currently active.";
            }
        }
        
        EditorUtility.DisplayDialog("Capture Complete", message + "\n\nDon't forget to save your changes!", "OK");
    }

    private void ApplyPresetToCamera()
    {
        if (m_camera == null)
        {
            Debug.LogError("Cannot apply: No camera found");
            return;
        }

        CameraPresetData preset = m_config.GetPreset(m_selectedPreset);
        if (preset == null)
        {
            Debug.LogError($"Cannot apply: Preset {m_selectedPreset} is null");
            return;
        }

        // Apply Follow settings
        var follow = m_camera.GetComponent<CinemachineFollow>();
        if (follow != null)
        {
            follow.FollowOffset = preset.positionOffset;
            follow.TrackerSettings.PositionDamping = new Vector3(
                preset.followDamping, 
                preset.followDamping, 
                preset.followDamping
            );
            Debug.Log($"  Applied Follow Offset: {preset.positionOffset}");
        }

        // Apply rotation
        m_camera.transform.rotation = Quaternion.Euler(preset.rotation);
        Debug.Log($"  Applied Rotation: {preset.rotation}");

        // Apply Follow Zoom settings
        var followZoom = m_camera.GetComponent<CinemachineFollowZoom>();
        if (followZoom != null)
        {
            followZoom.Width = preset.width;
            followZoom.Damping = preset.zoomDamping;
            followZoom.FovRange = preset.fovRange;
            Debug.Log($"  Applied Width: {preset.width}");
            Debug.Log($"  Applied Zoom Damping: {preset.zoomDamping}");
            Debug.Log($"  Applied FOV Range: {preset.fovRange}");
        }

        // Force refresh to show changes
        if (m_isPaused)
        {
            ForceRefreshCamera();
        }

        Debug.Log($"✓ Applied preset: {m_selectedPreset} to camera");
    }
}
#endif