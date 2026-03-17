using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[Serializable]
public class DialogueSaveData
{
    public List<string> flags;
}
public class DialogueContext
{
    public DialogueActor Listner;
    public DialogueActor Speaker;
    public DialogueNode Node;
}
public class DialogueManager : IManager
{
    private GameManager m_game;
    private List<string> m_flags = new List<string>();
    public event Action<DialogueContext> OnDialogueStarted;
    public event Action<DialogueContext> OnDialogueAdvanced;

    private Dictionary<string, DialogueNode> m_nodeLookup = new();

    private DialogueContext m_context;

    public DialogueManager(GameManager game) 
    {
        m_game = game;
    }
    #region IManager
    public bool Init() 
    {
        DialogueNode[] nodes = Resources.LoadAll<DialogueNode>("DialogueNodes");
        m_nodeLookup = nodes.ToDictionary(n => n.id, n => n);
        return true;
    }
    public bool Dispose() 
    {
        return true;
    }
    public void OnManagersInitialzied() 
    {
    
    }
    public void Update(float dt) 
    {
        
    }
    #endregion
    public DialogueNode GetNodeByID(string id) 
    {
        m_nodeLookup.TryGetValue(id, out DialogueNode node);
        return node;
    }
    public void EndDialogue() 
    {
        m_game.SetState(GameState.None);
    }
    public void StartDialogue(DialogueContext ctx) 
    {
        if (m_game.State == GameState.Dialogue) return;

        m_game.SetState(GameState.Dialogue);
        m_context = ctx;
        OnDialogueStarted?.Invoke(ctx);
    }
    public void NextNode(DialogueNode node) 
    {
        if (m_game.State != GameState.Dialogue) return;
        if (m_context == null) return;

        m_context.Node = node;        
        m_context.Speaker.AdvanceDialogue(node);
        OnDialogueAdvanced?.Invoke(m_context);
    }
    public DialogueSaveData Save() 
    {
        DialogueSaveData data = new DialogueSaveData();
        data.flags = m_flags;
        return data;
    }    
    public void Load(DialogueSaveData data) 
    {
        m_flags = data.flags;
    }
    public void SetFlag(string flag) 
    {
        if (m_flags.Contains(flag)) 
        {
            return;
        }
        m_flags.Add(flag);
    }
    public bool HasFlag(string flag) 
    {
        return m_flags.Contains(flag);
    }
}
