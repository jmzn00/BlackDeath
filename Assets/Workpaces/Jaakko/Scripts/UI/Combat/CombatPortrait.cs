using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CombatPortrait : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Image m_actorSprite;
    [SerializeField] private TMP_Text m_actorName;
    [SerializeField] private TMP_Text m_statusEffectsText;
    [SerializeField] private Slider m_healthSlider;

    private Button m_button;

    private CombatActor m_actor;
    public event Action<CombatActor> OnClick;

    public void Initialize(CombatActor actor)
    {
        m_actor = actor;    

        m_button = GetComponent<Button>();
                
        m_healthSlider.maxValue = m_actor.Health.MaxHealth;
        m_healthSlider.minValue = 0f;
        m_healthSlider.value = m_actor.Health.CurrentHealth;

        m_actor.Health.OnHealthChanged += HealthChanged;
        m_actor.OnStatusEffectsChanged += UpdateStatusEffects;

        m_actorName.text = m_actor.name;
        if (m_actor.Actor.actorSprite != null)
        {
            m_actorSprite.sprite = m_actor.Actor.actorSprite;
        }                
        if (m_actor.IsPlayer)
        {
            m_button.interactable = false;
            return;
        }
        
        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(() =>
        {
            OnClick?.Invoke(m_actor);
        });
        name = "CombatPortrait " + m_actor.name;
    }
    public Selectable GetSelectable() 
    {
        return m_button;
    }
    public void Dispose()
    {
        m_actor.OnStatusEffectsChanged -= UpdateStatusEffects;

        m_button.onClick.RemoveAllListeners();

        m_actor.Health.OnHealthChanged -= HealthChanged;

        Destroy(gameObject);
    }
    public void UpdateData(CombatActor actor) 
    {
        m_actor = actor;
        m_actorName.text = actor.name;

        if (actor.Actor.actorSprite != null)
            m_actorSprite.sprite = actor.Actor.actorSprite;

        UpdateStatusEffects(actor.StatusEffects);
    }
    void UpdateStatusEffects(List<ActorStatusEffect> effects)
    {
        string text = "Status Effects: ";
        if (effects != null && effects.Count > 0)
        {
            foreach (var e in effects)
            {
                text += $"\n {e.displayName} R:{e.RemainingTurns}";
            }
        }
        m_statusEffectsText.text = text;
    }    
    void HealthChanged(float newHealth) 
    {
        m_healthSlider.value = newHealth;
    }
}