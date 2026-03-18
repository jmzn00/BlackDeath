using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueActor : MonoBehaviour, IActorComponent
{
    [SerializeField] private DialogueNode m_startNode;

    private Actor m_actor;
    public Actor Actor => m_actor;
    private DialogueManager m_dialogue;

    private DialogueNode m_currentNode = null;

    private Stack<DialogueNode> m_history = new();
    public void SetInputSource(IInputSource source) 
    {
        
    }
    public bool Initialize(GameManager game) 
    {
        m_dialogue = game.Resolve<DialogueManager>();

        m_currentNode = m_startNode; // TEMP
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor) 
    {
        m_actor = actor;
    }
    public bool Dispose() 
    {
        return true;
    }
    public void LoadData(ActorSaveData data) 
    {
        if (string.IsNullOrEmpty(data.DialogueNodeID)) return;

        m_currentNode = m_dialogue.GetNodeByID(data.DialogueNodeID);
    }
    public void SaveData(ActorSaveData data) 
    {
        if (m_currentNode == null) return;

        data.DialogueNodeID = m_currentNode.id;
    }
    public void Load(object data) 
    {
        
    }
    public object Save() 
    {
        return null;
    }
    public void StartDialogue(DialogueActor listner, DialogueActor speaker) 
    {
        DialogueNode node;
        if (m_currentNode == null)
            node = m_startNode;
        else
            node = m_currentNode;

        DialogueContext ctx = new DialogueContext
        {
            Listner = listner,
            Speaker = speaker,
            Node = node
        };
        m_dialogue.StartDialogue(ctx);
    }
    public void AdvanceDialogue(DialogueNode node) 
    {        
        if (m_currentNode != null)
            m_history.Push(m_currentNode);

        m_currentNode = node;
    }
    public void GoBack() 
    {
        if (m_history.Count > 0)
            m_currentNode = m_history.Pop();
    }
}
