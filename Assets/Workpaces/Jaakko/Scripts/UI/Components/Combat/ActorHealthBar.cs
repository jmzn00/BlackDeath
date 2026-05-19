using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActorHealthBar : MonoBehaviour
{
    [SerializeField] private Slider m_healthSlider;
    [SerializeField] private TMP_Text m_nameText;
    [SerializeField] private TMP_Text m_healthText;
    [SerializeField] private Transform m_statusIconAnchor;
    [SerializeField] private Image m_statusIconPrefab;
    [SerializeField] private GameObject m_highlightIndicator;

    private CombatActor m_combatActor;
    private HealthComponent m_health;
    private readonly List<Image> m_statusIcons = new();
    private Transform m_cam;

    private void Start()
    {
        m_combatActor = GetComponentInParent<CombatActor>();
        m_health = GetComponentInParent<HealthComponent>();
        m_cam = Camera.main?.transform;

        if (m_health != null)
        {
            m_healthSlider.maxValue = m_health.MaxHealth;
            m_healthSlider.value = m_health.CurrentHealth;
            m_health.OnHealthChanged += OnHealthChanged;
        }

        if (m_combatActor != null)
        {
            m_nameText.text = m_combatActor.DisplayName;
            m_combatActor.OnStatusEffectsChanged += RefreshStatusIcons;
        }

        SetHighlighted(false);
    }

    private void LateUpdate()
    {
        if (m_cam != null)
            transform.rotation = m_cam.rotation;
    }

    private void OnDestroy()
    {
        if (m_health != null) m_health.OnHealthChanged -= OnHealthChanged;
        if (m_combatActor != null) m_combatActor.OnStatusEffectsChanged -= RefreshStatusIcons;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (m_highlightIndicator != null)
            m_highlightIndicator.SetActive(highlighted);
    }

    private void OnHealthChanged(float hp)
    {
        m_healthSlider.value = hp;
        if (m_healthText != null)
            m_healthText.text = $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(m_health.MaxHealth)}";
    }

    private void RefreshStatusIcons(List<StatusEffectInstance> effects)
    {
        foreach (var img in m_statusIcons) Destroy(img.gameObject);
        m_statusIcons.Clear();
        foreach (var e in effects)
        {
            if (e.Template.statusEffectSprite == null) continue;
            Image img = Instantiate(m_statusIconPrefab, m_statusIconAnchor);
            img.sprite = e.Template.statusEffectSprite;
            m_statusIcons.Add(img);
        }
    }
}
