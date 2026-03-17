using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(CombatCameraDirector))]
public class CombatCameraDirectorEditor : Editor
{
    private CombatCameraDirector m_director;
    private SerializedProperty m_presetsProp;
    private int m_selectedPresetIndex = 0;

    private GameObject m_actorObject;
    private AnimationClip m_previewClip;
    private float m_previewTime;
    private bool m_isPlaying;
    private double m_lastEditorTime;
    private bool m_useTrackedTarget = true;

    private void OnEnable()
    {
        m_director = (CombatCameraDirector)target;
        m_presetsProp = serializedObject.FindProperty("m_presets");
        RefreshPresetIndex();
        EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        StopPreviewPlay();
        EditorApplication.update -= EditorUpdate;
        AnimationMode.StopAnimationMode();
    }

    public override void OnInspectorGUI()
    {
        // draw default inspector first so user can configure anchor/presets etc.
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Live Preview / Director Tools", EditorStyles.boldLabel);

        // Preset selector
        string[] names = GetPresetNames();
        if (names.Length == 0)
        {
            EditorGUILayout.HelpBox("No presets available on director.", MessageType.Info);
        }
        else
        {
            m_selectedPresetIndex = EditorGUILayout.Popup("Preset", m_selectedPresetIndex, names);
        }

        // Actor / target to preview sampling on
        m_actorObject = (GameObject)EditorGUILayout.ObjectField("Actor (root)", m_actorObject, typeof(GameObject), true);
        m_useTrackedTarget = EditorGUILayout.Toggle("Use Actor CameraTarget", m_useTrackedTarget);

        // Animation clip and scrubber
        m_previewClip = (AnimationClip)EditorGUILayout.ObjectField("Preview Clip", m_previewClip, typeof(AnimationClip), false);

        if (m_previewClip != null)
        {
            float clipLen = m_previewClip.length;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("<<"))
            {
                m_previewTime = 0f;
                SampleClipAtTime();
            }
            if (GUILayout.Button("Preview Now"))
            {
                SampleClipAtTime();
            }
            if (GUILayout.Button("Snap Anchor"))
            {
                ApplyPresetNow();
            }
            if (GUILayout.Button("Stop"))
            {
                StopPreviewPlay();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(m_isPlaying ? "Pause Play" : "Play Preview"))
            {
                if (m_isPlaying) StopPreviewPlay();
                else StartPreviewPlay();
            }
            EditorGUILayout.LabelField($"Time {m_previewTime:0.000}/{clipLen:0.000}", GUILayout.Width(160));
            EditorGUILayout.EndHorizontal();

            float newTime = EditorGUILayout.Slider(m_previewTime, 0f, clipLen);
            if (!Mathf.Approximately(newTime, m_previewTime))
            {
                m_previewTime = newTime;
                SampleClipAtTime();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Assign an AnimationClip to scrub/sample actor animation.", MessageType.None);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply Preset Now (use selected actor)"))
        {
            ApplyPresetNow();
        }

        if (GUILayout.Button("Snap Anchor To Actor"))
        {
            SnapAnchorToActor();
        }
    }

    private string[] GetPresetNames()
    {
        if (m_presetsProp == null) return Array.Empty<string>();
        int count = m_presetsProp.arraySize;
        string[] names = new string[count];
        for (int i = 0; i < count; i++)
        {
            var elem = m_presetsProp.GetArrayElementAtIndex(i).objectReferenceValue as CombatCameraPreset;
            names[i] = elem != null ? elem.presetId : $"(null) {i}";
        }
        return names;
    }

    private void RefreshPresetIndex()
    {
        // keep index safe
        if (m_presetsProp == null) return;
        m_selectedPresetIndex = Mathf.Clamp(m_selectedPresetIndex, 0, Math.Max(0, m_presetsProp.arraySize - 1));
    }

    private void StartPreviewPlay()
    {
        if (m_previewClip == null || m_actorObject == null) return;
        m_isPlaying = true;
        m_lastEditorTime = EditorApplication.timeSinceStartup;
        AnimationMode.StartAnimationMode();
    }

    private void StopPreviewPlay()
    {
        if (!m_isPlaying) return;
        m_isPlaying = false;
        AnimationMode.StopAnimationMode();
    }

    private void EditorUpdate()
    {
        if (!m_isPlaying) return;
        if (m_previewClip == null || m_actorObject == null)
        {
            StopPreviewPlay();
            return;
        }

        double now = EditorApplication.timeSinceStartup;
        double dt = now - m_lastEditorTime;
        m_lastEditorTime = now;

        m_previewTime += (float)dt;
        if (m_previewTime > m_previewClip.length)
            m_previewTime = 0f;

        SampleClipAtTime();
        // repaint so scene/game views update
        SceneView.RepaintAll();
        if (EditorApplication.isPlaying)
            return; // Game view updates automatically while playing
        else
            EditorWindow.focusedWindow?.Repaint();
    }

    private void SampleClipAtTime()
    {
        if (m_previewClip == null || m_actorObject == null) return;

        // Start animation sampling mode and sample
        AnimationMode.StartAnimationMode();
        AnimationMode.SampleAnimationClip(m_actorObject, m_previewClip, m_previewTime);

        // After sampling the actor, apply the preset so the director updates anchor according to sampled CameraTarget.
        ApplyPresetNow();
        // mark scene dirty for editor visual update
        EditorUtility.SetDirty(m_actorObject);
        SceneView.RepaintAll();
    }

    private void ApplyPresetNow()
    {
        if (m_director == null) return;
        if (m_presetsProp == null || m_presetsProp.arraySize == 0) return;

        var presetObj = m_presetsProp.GetArrayElementAtIndex(m_selectedPresetIndex).objectReferenceValue as CombatCameraPreset;
        if (presetObj == null) return;

        Transform actorTransform = null;
        Transform targetTransform = null;
        if (m_actorObject != null)
            actorTransform = m_actorObject.transform;

        // if user enabled useTrackedTarget, director will prefer CameraTarget child
        m_director.ApplyPreset(presetObj, actorTransform, targetTransform);

        // repaint scene/game views
        SceneView.RepaintAll();
        if (EditorApplication.isPlaying)
        {
            // while playing, force a camera manager update next frame by marking director
            // (minimal; the runtime Update will handle anchor)
        }
    }

    private void SnapAnchorToActor()
    {
        if (m_director == null || m_actorObject == null) return;
        // Apply a quick follow preset with no smoothing to snap anchor to actor's camera target
        // Try find a preset with id "CC_FollowActor" first, else use selected preset
        CombatCameraPreset followPreset = null;
        foreach (var p in m_director.GetType().GetField("m_presets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(m_director) as System.Collections.IList)
        {
            if (p is CombatCameraPreset cp && cp.presetId == "CC_FollowActor")
            {
                followPreset = cp;
                break;
            }
        }
        if (followPreset == null)
        {
            // fallback to selected
            if (m_presetsProp.arraySize > 0)
                followPreset = m_presetsProp.GetArrayElementAtIndex(m_selectedPresetIndex).objectReferenceValue as CombatCameraPreset;
        }
        if (followPreset != null)
            m_director.ApplyPreset(followPreset, m_actorObject.transform, null);
    }
}