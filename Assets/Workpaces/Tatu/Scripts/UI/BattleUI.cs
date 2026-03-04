using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BattleUI : MonoBehaviour
{
    [Header("Action Menu")]
    public GameObject menuPanel;
    public Button attackButton;
    public Button skillsButton;
    public Button itemsButton;

    [Header("Player Stats")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerAPText;
    public Slider playerHealthBar;
    public Slider playerAPBar;

    [Header("Enemy Stats (multi-enemy)")]
    [Tooltip("One EnemyHUDEntry prefab/panel per enemy slot. Can be fewer than max — extras are hidden.")]
    public EnemyHUDEntry[] enemyHUDEntries;

    [Header("Target Selection")]
    [Tooltip("Highlight shown over the currently targeted enemy HUD entry.")]
    public GameObject targetHighlight;

    [Header("Reactive Prompt")]
    public GameObject reactiveRoot;
    public TextMeshProUGUI reactiveLabel;
    [Tooltip("Image used to display the input button icon.")]
    public UnityEngine.UI.Image reactiveIcon;
    [Tooltip("Indicator shown while parry state is active (e.g. a glowing shield icon).")]
    public GameObject parryIndicator;
    [Tooltip("Indicator shown while dodge state is active.")]
    public GameObject dodgeIndicator;

    [Header("Transition Banner")]
    public GameObject transitionPanel;
    public TextMeshProUGUI transitionText;

    // Events / state
    public Action<int> onOptionSelected;
    public int SelectedIndex => m_selectedIndex;

    private List<Button> m_menuOptions = new List<Button>();
    private int m_selectedIndex = 0;
    private Combatant m_boundPlayer;
    private List<Combatant> m_boundEnemies = new List<Combatant>();

    private void Awake()
    {
        HideMenuImmediate();
        HideTransitionImmediate();
        if (reactiveRoot != null) reactiveRoot.SetActive(false);
        if (parryIndicator != null) parryIndicator.SetActive(false);
        if (dodgeIndicator != null) dodgeIndicator.SetActive(false);
        if (targetHighlight != null) targetHighlight.SetActive(false);

        if (attackButton != null) attackButton.onClick.AddListener(() => ConfirmIndex(0));
        if (skillsButton != null) skillsButton.onClick.AddListener(() => ConfirmIndex(1));
        if (itemsButton != null) itemsButton.onClick.AddListener(() => ConfirmIndex(2));

        // Hide all enemy HUD slots initially
        if (enemyHUDEntries != null)
            foreach (var e in enemyHUDEntries) e?.Hide();
    }

    // Player binding
    public void BindPlayer(Combatant player)
    {
        UnbindPlayer();
        if (player == null) return;
        m_boundPlayer = player;

        if (playerNameText != null) playerNameText.text = player.gameObject.name;
        if (playerHealthBar != null) { playerHealthBar.maxValue = player.maxHealth; playerHealthBar.value = player.health; }
        if (playerAPBar != null) { playerAPBar.maxValue = player.maxActionPoints; playerAPBar.value = player.actionPoints; }
        RefreshPlayerTexts(player.health, player.actionPoints);
        player.onStatsChanged.AddListener(OnPlayerStats);
    }

    public void UnbindPlayer()
    {
        if (m_boundPlayer == null) return;
        m_boundPlayer.onStatsChanged.RemoveListener(OnPlayerStats);
        m_boundPlayer = null;
    }

    // Enemy binding

    public void BindEnemies(List<Combatant> enemies)
    {
        UnbindAllEnemies();
        if (enemies == null || enemyHUDEntries == null) return;

        int count = Mathf.Min(enemies.Count, enemyHUDEntries.Length);
        for (int i = 0; i < count; i++)
        {
            var enemy = enemies[i];
            var hud = enemyHUDEntries[i];
            if (enemy == null || hud == null) continue;

            hud.Bind(enemy);
            m_boundEnemies.Add(enemy);
        }
    }

    public void UnbindAllEnemies()
    {
        if (enemyHUDEntries != null)
            foreach (var hud in enemyHUDEntries) hud?.Unbind();
        m_boundEnemies.Clear();
    }

    //// Legacy single-enemy support
    //public void BindEnemy(Combatant enemy) => BindEnemies(new List<Combatant> { enemy });
    //public void UnbindEnemy() => UnbindAllEnemies();

    // Target selection
    public void ShowTargetSelection(List<Combatant> enemies, int selectedIndex)
    {
        UpdateTargetSelection(enemies, selectedIndex);
    }

    public void UpdateTargetSelection(List<Combatant> enemies, int selectedIndex)
    {
        if (enemyHUDEntries == null) return;

        for (int i = 0; i < enemyHUDEntries.Length; i++)
        {
            bool isSelected = i == selectedIndex && i < enemies.Count;
            enemyHUDEntries[i]?.SetTargeted(isSelected);
        }

        // Move highlight anchor to selected HUD entry if using a single highlight object
        if (targetHighlight != null && selectedIndex < enemyHUDEntries.Length)
        {
            var hud = enemyHUDEntries[selectedIndex];
            if (hud != null)
            {
                targetHighlight.SetActive(true);
                targetHighlight.transform.SetParent(hud.transform, false);
                targetHighlight.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void HideTargetSelection()
    {
        if (targetHighlight != null) targetHighlight.SetActive(false);
        if (enemyHUDEntries != null)
            foreach (var hud in enemyHUDEntries) hud?.SetTargeted(false);
    }

    // Stat callbacks

    private void OnPlayerStats(int hp, int ap)
    {
        RefreshPlayerTexts(hp, ap);
        if (playerHealthBar != null) playerHealthBar.value = hp;
        if (playerAPBar != null) playerAPBar.value = ap;
    }

    private void RefreshPlayerTexts(int hp, int ap)
    {
        if (playerHealthText != null)
            playerHealthText.text = $"{hp}/{(playerHealthBar != null ? (int)playerHealthBar.maxValue : 0)}";
        if (playerAPText != null)
            playerAPText.text = $"{ap}/{(playerAPBar != null ? (int)playerAPBar.maxValue : 0)}";
    }

    // Action menu

    public void ShowMenu(bool attackOnly = false)
    {
        if (menuPanel == null) return;
        menuPanel.SetActive(true);
        if (attackButton != null) attackButton.interactable = true;
        if (skillsButton != null) skillsButton.interactable = !attackOnly;
        if (itemsButton != null) itemsButton.interactable = !attackOnly;

        m_menuOptions.Clear();
        if (attackButton != null) m_menuOptions.Add(attackButton);
        if (skillsButton != null) m_menuOptions.Add(skillsButton);
        if (itemsButton != null) m_menuOptions.Add(itemsButton);

        m_selectedIndex = 0;
        RefreshMenuSelection();
    }

    public void HideMenu()
    {
        if (menuPanel == null) return;
        menuPanel.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void HideMenuImmediate()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void Next()
    {
        if (m_menuOptions.Count == 0) return;
        m_selectedIndex = (m_selectedIndex + 1) % m_menuOptions.Count;
        RefreshMenuSelection();
    }

    public void Previous()
    {
        if (m_menuOptions.Count == 0) return;
        m_selectedIndex = (m_selectedIndex - 1 + m_menuOptions.Count) % m_menuOptions.Count;
        RefreshMenuSelection();
    }

    public void Confirm() => ConfirmIndex(m_selectedIndex);

    private void ConfirmIndex(int index) => onOptionSelected?.Invoke(index);

    private void RefreshMenuSelection()
    {
        if (m_menuOptions.Count == 0) return;
        var btn = m_menuOptions[m_selectedIndex];
        if (btn == null) return;
        btn.Select();
        EventSystem.current?.SetSelectedGameObject(btn.gameObject);
    }

    // Reactive prompts and indicators (most probably just button icons)

    public void ShowReactivePrompt(string label, Sprite icon = null)
    {
        if (reactiveRoot == null) return;
        reactiveRoot.SetActive(true);
        if (reactiveLabel != null) reactiveLabel.text = label;
        if (reactiveIcon != null)
        {
            reactiveIcon.sprite = icon;
            reactiveIcon.enabled = icon != null;
        }
    }

    public void HideReactivePrompt()
    {
        if (reactiveRoot != null) reactiveRoot.SetActive(false);
    }

    public void SetParryIndicator(bool active)
    {
        if (parryIndicator != null) parryIndicator.SetActive(active);
    }

    public void SetDodgeIndicator(bool active)
    {
        if (dodgeIndicator != null) dodgeIndicator.SetActive(active);
    }

    // Transition panel for between turns or other

    public void ShowTransition(string text)
    {
        if (transitionPanel == null || transitionText == null) return;
        transitionPanel.SetActive(true);
        transitionText.text = text;
    }

    public void HideTransition() { if (transitionPanel != null) transitionPanel.SetActive(false); }
    public void HideTransitionImmediate() { if (transitionPanel != null) transitionPanel.SetActive(false); }
}
