using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour, IUIComponentView
{
    [SerializeField] private Slider m_healthSlider;
    public void Init() 
    {
        m_healthSlider.interactable = false;
    }
    public void View() 
    {
        
    }
    public void Hide() 
    {
    
    }
    public void OnActorChanged(Actor actor) 
    {
        HealthComponent health = actor.Get<HealthComponent>();
        m_healthSlider.maxValue = health.MaxHealth;

        health.OnHealthChanged += OnHealthChanged;        
        m_healthSlider.value = health.GetHealth();
    }
    public void OnHealthChanged(float newHealth) 
    {
        m_healthSlider.value = newHealth;
    }
}
