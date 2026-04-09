using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectPortrait : MonoBehaviour
{
    [SerializeField] private Image m_statusImage;
    [SerializeField] private TMP_Text m_durationText;

    private StatusEffectInstance m_instance;
    public void Bind(StatusEffectInstance instance) 
    {
        m_instance = instance;

        m_instance.OnDurationChanged += DurationChanged;

        Sprite sprite = instance.Template.statusEffectSprite;
        if (sprite != null)
            m_statusImage.sprite = sprite;
        else
            Debug.LogWarning($"{m_instance.Template.displayName} does not have a sprite");


            m_durationText.text = $"{instance.RemainingTurns}";
    }
    private void DurationChanged(int duration) 
    {
        if (duration <= 0) 
        {
            m_instance.OnDurationChanged -= DurationChanged;
            Destroy(gameObject);
            m_instance = null;
        }
        m_durationText.text = $"{duration}";
    }
    private void OnDestroy()
    {
        if (m_instance != null) 
        {
            m_instance.OnDurationChanged -= DurationChanged;
            m_instance = null;
        }
    }
}
