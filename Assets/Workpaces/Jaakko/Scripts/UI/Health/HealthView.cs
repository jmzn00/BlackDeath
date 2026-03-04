using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private Slider m_healthSlider;
    public void Init(Actor actor) 
    {
        m_healthSlider.interactable = false;
    }
    public void View() 
    {
        
    }
    public void Hide() 
    {
    
    }
    public void OnHealthChanged(float newHealth) 
    {
        if (m_healthSlider == null) return;

        Debug.Log($"Health changed: {newHealth}");

        m_healthSlider.value = newHealth;
    }
}
