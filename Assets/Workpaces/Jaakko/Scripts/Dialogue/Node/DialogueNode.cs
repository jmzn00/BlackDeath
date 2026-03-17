using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    public string id;

    [TextArea(4, 10)]
    public string text;

    public DialogueChoice[] choices;

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (Application.isPlaying) return;

        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (string.IsNullOrEmpty(id) || IsDuplicateID())
        {
            id = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
    }

    private bool IsDuplicateID()
    {
        string[] guids = AssetDatabase.FindAssets("t:DialogueNode");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueNode node = AssetDatabase.LoadAssetAtPath<DialogueNode>(path);

            if (node == null || node == this) continue;

            if (node.id == id)
                return true;
        }

        return false;
    }
#endif
}
[System.Serializable]
public class DialogueChoice 
{
    [TextArea(4, 10)]
    public string choiceText;

    public DialogueNode nextNode;

    public string conditionFlag;
    public string setFlag;
}
