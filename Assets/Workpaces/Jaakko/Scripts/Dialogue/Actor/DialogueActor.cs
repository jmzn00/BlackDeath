using UnityEngine;

public class DialogueActor : MonoBehaviour, IActorComponent
{
    [SerializeField] private DialogueNode m_startNode;

    private Actor m_actor;
    public Actor Actor => m_actor;
    private DialogueManager m_dialogue;

    private DialogueNode m_currentNode = null;
    public DialogueNode CurrentNode => m_currentNode;
    
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
        if (data.DialogueNodeID == null) return;

        m_currentNode = m_dialogue.GetNodeByID(data.DialogueNodeID);
    }
    public void SaveData(ActorSaveData data) 
    {
        if (m_currentNode == null) return;

        data.DialogueNodeID = m_currentNode.id;
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
        m_currentNode = node;
    }
}
