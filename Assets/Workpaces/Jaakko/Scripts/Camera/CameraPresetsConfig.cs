using UnityEngine;

[CreateAssetMenu(fileName = "CameraPresets", menuName = "Combat/Camera Presets")]
public class CameraPresetsConfig : ScriptableObject
{
    [Header("Combat Presets")]
    public CameraPresetData actionSelection = new CameraPresetData { presetName = "Action Selection" };
    public CameraPresetData targeting = new CameraPresetData { presetName = "Targeting" };
    public CameraPresetData turnStart = new CameraPresetData { presetName = "Turn Start" };
    public CameraPresetData actionExecution = new CameraPresetData { presetName = "Action Execution" };
    public CameraPresetData reactionWindow = new CameraPresetData { presetName = "Reaction Window" };

    [Header("Exploration Presets")]
    public CameraPresetData exploration = new CameraPresetData { presetName = "Exploration" };

    public CameraPresetData GetPreset(CameraPresetType type)
    {
        switch (type)
        {
            case CameraPresetType.ActionSelection: return actionSelection;
            case CameraPresetType.Targeting: return targeting;
            case CameraPresetType.TurnStart: return turnStart;
            case CameraPresetType.ActionExecution: return actionExecution;
            case CameraPresetType.ReactionWindow: return reactionWindow;
            case CameraPresetType.Exploration: return exploration;
            default: return actionSelection;
        }
    }
}

public enum CameraPresetType
{
    ActionSelection,
    Targeting,
    TurnStart,
    ActionExecution,
    ReactionWindow,
    Exploration
}