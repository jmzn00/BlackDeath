using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour, IUIComponentView
{
    [Header("Prefabs")]
    [SerializeField] private DialoguePortrait m_portraitPrefab;
    [SerializeField] private Button m_choiceButtonPrefab;

    [Header("Anchors")]
    [SerializeField] private Transform m_choiceAnchor;
    [Header("Speaker")]
    [SerializeField] private DialoguePortrait m_speakerPortrait;
    [SerializeField] private TMP_Text m_speakerText;

    [Header("Dialogue")]
    [SerializeField] private Button m_endButton;

    private DialogueManager m_dialogue;
    private List<Button> m_choiceButtons = new();

    public void OnActorChanged(Actor actor) 
    {
        
    }
    public void Init() 
    {
        
    }
    public void Initialize(DialogueManager dialogue) 
    {
        m_dialogue = dialogue;

        m_dialogue.OnDialogueStarted += DialogueStarted;
        m_dialogue.OnDialogueAdvanced += DialogueAdvanced;

        m_endButton.onClick.AddListener(() =>
        {
            m_dialogue.EndDialogue();
            ClearChoiceButtons();
        });
    }
    public void DialogueStarted(DialogueContext context) 
    {
        m_speakerPortrait.Bind(context.Speaker);
        DisplayNode(context.Node);
    }
    public void DialogueAdvanced(DialogueContext ctx) 
    {
        DisplayNode(ctx.Node);
    }
    private void DisplayNode(DialogueNode node) 
    {
        m_speakerText.text = node.text;

        foreach (var c in node.choices)
        {
            if (!string.IsNullOrEmpty(c.conditionFlag))
            {
                if (!m_dialogue.HasFlag(c.conditionFlag)) 
                {
                    continue;
                }
            }

            Button b = Instantiate(m_choiceButtonPrefab, m_choiceAnchor);
            m_choiceButtons.Add(b);

            var choice = c;
            b.GetComponentInChildren<TMP_Text>().text = choice.choiceText;            

            if (choice.nextNode != null)
            {
                b.onClick.AddListener(() =>
                {               
                    if (!string.IsNullOrEmpty(choice.setFlag)) 
                    {
                        m_dialogue.SetFlag(choice.setFlag);
                    }
                    m_dialogue.NextNode(choice.nextNode);
                    ClearChoiceButtons();
                });
            }
        }        
    }    
    private void ClearChoiceButtons() 
    {
        foreach (var b in m_choiceButtons) 
        {
            Destroy(b.gameObject);
        }
        m_choiceButtons.Clear();
    }
    public void View() 
    {
        gameObject.SetActive(true);
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }
}
