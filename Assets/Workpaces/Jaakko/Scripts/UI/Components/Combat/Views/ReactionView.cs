using UnityEngine;
using UnityEngine.UI;

public class ReactionView : UIViewBase
{
    [SerializeField] private Image m_promptImage;

    public void AttackerPromptOpened(InputPrompt prompt)
    {
        if (m_promptImage == null || prompt == null || prompt.icon == null) return;
        m_promptImage.sprite = prompt.icon;
    }
}
