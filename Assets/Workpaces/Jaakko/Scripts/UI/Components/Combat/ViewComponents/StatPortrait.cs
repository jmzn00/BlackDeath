using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatPortrait : MonoBehaviour
{
    [Header("Portrait")]
    [SerializeField] private Image m_portraitImage;
    [Header("Text")]
    [SerializeField] private TMP_Text m_parryText;
    [SerializeField] private TMP_Text m_dogeText;
    [SerializeField] private TMP_Text m_dealtDamageText;
    [SerializeField] private TMP_Text m_recievedDamageText;
    [SerializeField] private TMP_Text m_dealtHealText;
    [SerializeField] private TMP_Text m_recievedHealText;
    [SerializeField] private TMP_Text m_scoreText;

    public void Bind(CombatActorStats stats) 
    {
        if (stats == null)
        {
            Debug.LogWarning("StatPortrait.Bind: stats is null");
            return;
        }

        // Portrait sprite (safe navigation)
        if (m_portraitImage != null)
        {
            Sprite sprite = null;
            if (stats.Actor == null) 
            {
                Debug.LogWarning("StatPortrait.Bind: stats.CombatActor is null");
                return;
            }
            if (stats.Actor.Actor == null) 
            {
                Debug.LogWarning("StatPortrait.Bind: stats.Actor is NULL");
                return;                    
            }
            sprite = stats.Actor.Actor.actorSprite;
            m_portraitImage.sprite = sprite;
        }

        if (m_parryText != null)
            m_parryText.SetText($"Parries Performed: {stats.ParriesPerformed}");
        if (m_dogeText != null)
            m_dogeText.SetText($"Dodges Performed: {stats.DodgesPerformed}");
        if (m_dealtDamageText != null)
            m_dealtDamageText.SetText($"Damage Dealt: {stats.DamageDealt}");
        if (m_recievedDamageText != null)
            m_recievedDamageText.SetText($"Damage Taken: {stats.DamageTaken}");
        if (m_dealtHealText != null)
            m_dealtHealText.SetText($"Heal Dealt: {stats.HealDealt}");
        if (m_recievedHealText != null)
            m_recievedHealText.SetText($"Heal Taken: {stats.HealTaken}");

        if (m_scoreText != null)
            m_scoreText.SetText($"Score: {stats.score}");
    }
}
