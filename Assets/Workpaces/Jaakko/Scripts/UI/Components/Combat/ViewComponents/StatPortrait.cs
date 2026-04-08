using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        m_portraitImage.sprite = stats.Actor.Actor.actorSprite;

        m_parryText.SetText($"Parries Performed: {stats.ParriesPerformed}");
        m_dogeText.SetText($"Dodges Performed: {stats.DodgesPerformed}");
        m_dealtDamageText.SetText($"Damage Dealt: {stats.DamageDealt}");
        m_recievedDamageText.SetText($"Damage Taken: {stats.DamageTaken}");
        m_dealtHealText.SetText($"Heal Dealt: {stats.HealDealt}");                                      
        m_recievedHealText.SetText($"Heal Taken: {stats.HealTaken}");           

        m_scoreText.SetText($"Score: {CalculateScore(stats)}");
    }
    private float CalculateScore(CombatActorStats stats) 
    {
        return stats.DamageDealt + stats.HealDealt + stats.ActionsHit;
    }
}
