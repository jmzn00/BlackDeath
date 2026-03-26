using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatPortrait : MonoBehaviour
{
    [Header("Actor")]
    [SerializeField] private Image m_portraitImage;
    [Header("Health")]
    [SerializeField] private Slider m_healthSlider;
    [SerializeField] private TMP_Text m_healthText;
    [Header("AP")]
    [SerializeField] private Slider m_apSlider;
    [SerializeField] private TMP_Text m_apText;
    [Header("Status Effect")]
    [SerializeField] private Transform m_statusAnchor;
    [SerializeField] private StatusEffectPortrait m_statusPortraitPrefab;

    private CombatActor m_actor;
    public void Bind(CombatActor actor) 
    {
        m_actor = actor;


        if (actor.Actor.actorSprite)
            m_portraitImage.sprite = actor.Actor.actorSprite;

        m_healthSlider.maxValue = m_actor.Health.MaxHealth;
        m_actor.Health.OnHealthChanged += HealthChanged;
        HealthChanged(m_actor.Health.CurrentHealth);

        m_apSlider.maxValue = m_actor.MaxActionPoints;
        m_actor.OnActionPointsChanged += ApChanged;
        ApChanged(m_actor.ActionPoints);

        m_actor.OnStatusEffectsChanged += StatusEffectsChanged;
    }
    private void OnDestroy()
    {
        m_actor.Health.OnHealthChanged -= HealthChanged;
        m_actor.OnActionPointsChanged -= ApChanged;
    }
    private void HealthChanged(float newHealth) 
    {
        int max = Mathf.CeilToInt(m_actor.Health.MaxHealth);
        int health = Mathf.CeilToInt(newHealth);

        m_healthSlider.value = health;
        m_healthText.text = $"{health} / {max}";
    }
    private void ApChanged(int newValue) 
    {
        m_apSlider.value = newValue;
        m_apText.text = $"{newValue} / {m_actor.MaxActionPoints}";
    }
    private void StatusEffectsChanged(List<StatusEffectInstance> effects) 
    {
        foreach (Transform child in m_statusAnchor) 
        {
            Destroy(child.gameObject);
        }

        foreach (var e in effects) 
        {
            StatusEffectPortrait p = Instantiate(m_statusPortraitPrefab,
                m_statusAnchor);
            p.Bind(e);
        }
    }
    
}
