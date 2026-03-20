using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Defines a single input prompt: which button to watch for, what label and icon to show.
/// Create via: right-click → Create → Combat/InputPrompt
/// </summary>
[CreateAssetMenu(menuName = "Combat/InputPrompt")]
public class InputPrompt : ScriptableObject
{
    [Header("Display")]
    [Tooltip("Short label shown in the UI e.g. 'PRESS!' or 'SHIFT!'")]
    public string label = "PRESS!";
    [Tooltip("Button icon shown alongside the label.")]
    public Sprite icon;

    public Sprite psIcon;
    public Sprite xboxIcon;

    [Header("Input")]
    [Tooltip("Which input to listen for during the window.")]
    public PromptInputType inputType = PromptInputType.Confirm;
    public InputAction action;
}
public enum PromptInputType
{
    Confirm,  // Space / Enter / Confirm action
    Parry,    // Left Shift / Parry action
    Dodge,    // Left Alt / Dodge action
}
