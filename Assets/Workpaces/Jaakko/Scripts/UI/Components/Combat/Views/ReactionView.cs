using UnityEngine;
using UnityEngine.UI;

public class ReactionView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private Image m_promptImage;
    public void Init()
    {

    }
    public void AttackerPromptOpened(InputPrompt prompt) 
    {
        if (m_promptImage == null ||
            prompt == null ||
            prompt.icon == null) { return; }

        m_promptImage.sprite = prompt.icon;
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