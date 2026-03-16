using TMPro;
using UnityEngine;

public class CurrentActorText : MonoBehaviour
{
    private TMP_Text m_text;

    private void Start()
    {
        m_text = GetComponent<TMP_Text>();


        CombatEvents.OnTurnStarted += TurnStarted;
        
    }
    private void TurnStarted(CombatActor actor) 
    {
        m_text.text = actor.name;
    }
}
