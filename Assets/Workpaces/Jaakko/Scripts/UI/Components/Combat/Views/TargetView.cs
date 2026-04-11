using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetView : UIViewBase
{
    [Header("Elements")]
    [SerializeField] private TMP_Text m_targetNameText;
    [SerializeField] private Slider m_healthSlider;
    [Header("Anchors")]
    [SerializeField] private Transform m_statusEffectImageAnchor;
    [Header("Prefabs")]
    [SerializeField] private Image m_statusEffectImagePrefab;
    [Header("Position")]
    [SerializeField] private Vector3 m_positionOffset;
    public void SetPosition(Vector3 position) 
    {
        transform.position = position + m_positionOffset;
    }
    private List<Image> m_statusEffectImages = new();

    private CombatActor m_target = null;
    public void ChangeTarget(CombatActor target) 
    {
        if (m_target != null) 
        {
            m_target.Health.OnHealthChanged -= HealthChanged;
        }
        m_target = target;

        m_healthSlider.maxValue = target.Health.MaxHealth;
        m_healthSlider.value = target.Health.CurrentHealth;

        m_target.Health.OnHealthChanged += HealthChanged;

        m_targetNameText.text = target.name;
        
        ClearImages();
        foreach (var i in target.CurrentStatusEffects) 
        {
            if (i.Template.statusEffectSprite == null)
                continue;

            Image image = Instantiate(m_statusEffectImagePrefab
                , m_statusEffectImageAnchor);
            image.sprite = i.Template.statusEffectSprite;
            m_statusEffectImages.Add(image);
        }
    }
    private void HealthChanged(float newHealth) 
    {
        m_healthSlider.value = newHealth;
    }
    private void ClearImages() 
    {
        foreach (var i in m_statusEffectImages) 
        {
            Destroy(i.gameObject);
        }
        m_statusEffectImages.Clear();
    }   
}
