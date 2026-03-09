using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
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

    private Action<CombatActor> m_onClick;

    private CombatActor m_actor;

    public void Initialize(CombatActor actor, Action<CombatActor> onClick)
    {
        m_actor = actor;
        m_onClick = onClick;        

        m_button = GetComponent<Button>();

        HealthComponent health = actor.Health;
        m_healthSlider.maxValue = health.MaxHealth;
        m_healthSlider.onValueChanged.AddListener(value =>
        {
            m_healthSlider.value = value;
        });
        m_healthSlider.value = health.GetHealth();

        m_actorName.text = m_actor.name;
        if (m_actor.Actor.actorSprite != null)
        {
            m_actorSprite.sprite = m_actor.Actor.actorSprite;
        }
        UpdateStatusEffects(m_actor.StatusEffects);
        m_actor.OnStatusEffectsChanged += UpdateStatusEffects;

        if (m_actor.IsPlayer)
        {
            m_button.interactable = false;
            return;
        }

        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(() =>
        {
            m_onClick?.Invoke(m_actor);
        });
        name = "CombatPortrait " + m_actor.name;
    }
    public void Dispose()
    {
        if (this == null)
            return;

        if (m_actor != null)
            m_actor.OnStatusEffectsChanged -= UpdateStatusEffects;

        if (m_button != null)
            m_button.onClick.RemoveAllListeners();

        m_healthSlider.onValueChanged.RemoveAllListeners();

        Destroy(gameObject);
    }
    public void UpdateData(CombatActor actor) 
    {
        m_actor = actor;

        // Update health
        m_healthSlider.maxValue = actor.Health.MaxHealth;
        m_healthSlider.value = actor.Health.GetHealth();

        // Update name and sprite
        m_actorName.text = actor.name;
        if (actor.Actor.actorSprite != null)
            m_actorSprite.sprite = actor.Actor.actorSprite;

        // Update status effects
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
}