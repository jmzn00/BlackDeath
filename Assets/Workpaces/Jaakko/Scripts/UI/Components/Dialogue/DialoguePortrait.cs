using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePortrait : MonoBehaviour
{
    [Header("Actor")]
    [SerializeField] private Image m_actorImage;
    [SerializeField] private TMP_Text m_actorNameText;

    private Actor m_actor;
    private DialogueActor m_dialogue;

    public void Bind(DialogueActor d) 
    {
        m_actor = d.Actor;
        m_dialogue = m_actor.Get<DialogueActor>();
        if (m_dialogue == null) 
        {
            Debug.LogWarning($"DV: DialogueActorComponent is NULL on {m_actor.name}");
            return;                
        }
        m_actorImage.sprite = m_actor.actorSprite;
        m_actorNameText.text = m_actor.name;
    }
    public void Clear() 
    {
        m_actor = null;
    }
}
