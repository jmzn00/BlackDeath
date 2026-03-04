using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHUDEntry : MonoBehaviour
{
    [Header("Display")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;
    public Slider healthBar;
    [Tooltip("Visual indicator shown when this enemy is the current target.")]
    public GameObject targetedIndicator;

    private Combatant m_bound;

    public void Bind(Combatant enemy)
    {
        Unbind();
        if (enemy == null) return;
        m_bound = enemy;

        gameObject.SetActive(true);

        if (nameText   != null) nameText.text = enemy.gameObject.name;
        if (healthBar  != null) { healthBar.maxValue = enemy.maxHealth; healthBar.value = enemy.health; }
        RefreshTexts(enemy.health);

        enemy.onStatsChanged.AddListener(OnStats);
    }

    public void Unbind()
    {
        if (m_bound != null)
        {
            m_bound.onStatsChanged.RemoveListener(OnStats);
            m_bound = null;
        }
        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        SetTargeted(false);
    }

    public void SetTargeted(bool targeted)
    {
        if (targetedIndicator != null) targetedIndicator.SetActive(targeted);
    }

    private void OnStats(int hp, int ap)
    {
        RefreshTexts(hp);
        if (healthBar != null) healthBar.value = hp;
    }

    private void RefreshTexts(int hp)
    {
        if (healthText != null)
            healthText.text = $"{hp}/{(healthBar != null ? (int)healthBar.maxValue : 0)}";
    }
}
