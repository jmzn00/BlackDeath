using UnityEngine;

[CreateAssetMenu(fileName = "CameraPresets", menuName = "Combat/Camera Presets")]
public class CameraPresetsConfig : ScriptableObject
{
    [Header("Player Turn Presets")]
    public CameraPresetData playerActionSelecting = new CameraPresetData { presetName = "Player Action Selecting" };
    public CameraPresetData playerTargeting = new CameraPresetData { presetName = "Player Targeting" };
    
    [Header("Enemy Turn Presets")]
    public CameraPresetData enemyTurn = new CameraPresetData { presetName = "Enemy Turn" };
    
    [Header("Shared Combat Presets")]
    public CameraPresetData transition = new CameraPresetData { presetName = "Transition" };
    public CameraPresetData actionExecution = new CameraPresetData { presetName = "Action Execution" };
    
    [Header("Combat Flow Presets")]
    public CameraPresetData turnStart = new CameraPresetData { presetName = "Turn Start" };
    public CameraPresetData combatEnd = new CameraPresetData { presetName = "Combat End" };

    [Header("Exploration Presets")]
    public CameraPresetData exploration = new CameraPresetData { presetName = "Exploration" };

    public CameraPresetData GetPreset(CameraPresetType type)
    {
        switch (type)
        {
            // Player Turn
            case CameraPresetType.PlayerActionSelecting: return playerActionSelecting;
            case CameraPresetType.PlayerTargeting: return playerTargeting;
            
            // Enemy Turn
            case CameraPresetType.EnemyTurn: return enemyTurn;
            
            // Shared Combat
            case CameraPresetType.Transition: return transition;
            case CameraPresetType.ActionExecution: return actionExecution;
            
            // Combat Flow
            case CameraPresetType.TurnStart: return turnStart;
            case CameraPresetType.CombatEnd: return combatEnd;
            
            // Exploration
            case CameraPresetType.Exploration: return exploration;
            
            default: return playerActionSelecting;
        }
    }
}

public enum CameraPresetType
{
    // Player Turn States
    PlayerActionSelecting,
    PlayerTargeting,
    
    // Enemy Turn
    EnemyTurn,
    
    // Shared Combat States
    Transition,
    ActionExecution,
    
    // Combat Flow
    TurnStart,
    CombatEnd,
    
    // Exploration
    Exploration
}