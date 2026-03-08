using NUnit.Framework;
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

    private Button m_button;

    private Action<CombatActor> m_onClick;

    private CombatActor m_actor;

    public void Initialize(CombatActor actor, Action<CombatActor> onClick)
    {
        m_actor = actor;
        m_onClick = onClick;

        m_button = GetComponent<Button>();
        m_button.onClick.RemoveAllListeners();
        m_button.onClick.AddListener(() =>
        {
            m_onClick?.Invoke(m_actor);
        });
        m_actorName.text = m_actor.name;
        if (m_actor.Actor.actorSprite != null)
        {
            m_actorSprite.sprite = m_actor.Actor.actorSprite;
        }
        UpdateStatusEffects(m_actor.StatusEffects);
        m_actor.OnStatusEffectsChanged += UpdateStatusEffects;
    }
    public void Dispose()
    {
        // If the Unity object was already destroyed, the overloaded == operator
        // will return true — bail out to avoid accessing gameObject or other properties.
        if (this == null)
            return;

        if (m_actor != null)
            m_actor.OnStatusEffectsChanged -= UpdateStatusEffects;

        if (m_button != null)
            m_button.onClick.RemoveAllListeners();

        Destroy(gameObject);
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