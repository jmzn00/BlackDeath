using TMPro;
using UnityEngine;

public class AnnouncementView : UIViewBase
{
    [SerializeField] private TMP_Text m_text;

    public void Display(string actorName, string actionName)
    {
        m_text.text = $"{actorName} uses {actionName}";
        View();
    }
}
