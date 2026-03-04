using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A named collection of InputPrompts. Assign one to BattleManager.
/// Animation events reference prompts by their key string.
/// Create via: right-click → Create → Combat/InputPromptLibrary
/// </summary>
[CreateAssetMenu(menuName = "Combat/InputPromptLibrary")]
public class InputPromptLibrary : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        [Tooltip("Key used in animation events e.g. 'fire_tap', 'heavy_charge'")]
        public string key;
        public InputPrompt prompt;
    }

    public Entry[] entries;

    private Dictionary<string, InputPrompt> m_lookup;

    /// <summary>Returns the prompt for the given key, or null if not found.</summary>
    public InputPrompt Get(string key)
    {
        if (m_lookup == null) BuildLookup();
        m_lookup.TryGetValue(key, out var prompt);
        return prompt;
    }

    private void BuildLookup()
    {
        m_lookup = new Dictionary<string, InputPrompt>();
        if (entries == null) return;
        foreach (var e in entries)
            if (!string.IsNullOrEmpty(e.key) && e.prompt != null)
                m_lookup[e.key] = e.prompt;
    }

    // Rebuild if entries change in editor
    private void OnValidate() => m_lookup = null;
}
