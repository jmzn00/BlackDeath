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

    [Header("Skills Menu")]
    public GameObject skillsPanel;
    public Button skillButton1;
    public Button skillButton2;
    public Button skillButton3;
    public Button skillBackButton;
    public TextMeshProUGUI skillButton1Label;
    public TextMeshProUGUI skillButton2Label;
    public TextMeshProUGUI skillButton3Label;
    public TextMeshProUGUI skillAPCost1Label;
    public TextMeshProUGUI skillAPCost2Label;
    public TextMeshProUGUI skillAPCost3Label;

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
    [Tooltip("Panel showing detailed info about the currently targeted enemy.")]
    public GameObject targetInfoPanel;
    public TextMeshProUGUI targetNameText;
    public TextMeshProUGUI targetHealthText;
    public Slider targetHealthBar;
    public TextMeshProUGUI targetStatusText;

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
    public Action onBackToMainMenu;
    public int SelectedIndex => m_selectedIndex;
    public int SkillButtonCount => m_skillButtonCount;

    private List<Button> m_menuOptions = new List<Button>();
    private int m_selectedIndex = 0;
    private int m_skillButtonCount = 0;
    private Combatant m_boundPlayer;
    private List<Combatant> m_boundEnemies = new List<Combatant>();
    private Combatant m_currentTargetedEnemy;

    private void Awake()
    {
        HideMenuImmediate();
        HideTransitionImmediate();
        if (reactiveRoot != null) reactiveRoot.SetActive(false);
        if (parryIndicator != null) parryIndicator.SetActive(false);
        if (dodgeIndicator != null) dodgeIndicator.SetActive(false);
        if (targetHighlight != null) targetHighlight.SetActive(false);
        if (skillsPanel != null) skillsPanel.SetActive(false);
        if (targetInfoPanel != null) targetInfoPanel.SetActive(false);

        if (attackButton != null) attackButton.onClick.AddListener(() => ConfirmIndex(0));
        if (skillsButton != null) skillsButton.onClick.AddListener(() => ConfirmIndex(1));
        if (itemsButton != null) itemsButton.onClick.AddListener(() => ConfirmIndex(2));

        if (skillButton1 != null) skillButton1.onClick.AddListener(() => ConfirmSkillIndex(0));
        if (skillButton2 != null) skillButton2.onClick.AddListener(() => ConfirmSkillIndex(1));
        if (skillButton3 != null) skillButton3.onClick.AddListener(() => ConfirmSkillIndex(2));
        if (skillBackButton != null) skillBackButton.onClick.AddListener(() => onBackToMainMenu?.Invoke());

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
        if (targetInfoPanel != null) targetInfoPanel.SetActive(true);
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

        // Update target info panel
        if (selectedIndex >= 0 && selectedIndex < enemies.Count)
        {
            UpdateTargetInfo(enemies[selectedIndex]);
        }
    }

    public void HideTargetSelection()
    {
        if (targetHighlight != null) targetHighlight.SetActive(false);
        if (targetInfoPanel != null) targetInfoPanel.SetActive(false);
        if (enemyHUDEntries != null)
            foreach (var hud in enemyHUDEntries) hud?.SetTargeted(false);
        
        UnbindCurrentTarget();
    }

    // Target info panel
    private void UpdateTargetInfo(Combatant target)
    {
        if (target == null) return;

        // Unbind previous target
        UnbindCurrentTarget();

        // Bind new target
        m_currentTargetedEnemy = target;
        
        if (targetNameText != null) targetNameText.text = target.gameObject.name;
        if (targetHealthBar != null)
        {
            targetHealthBar.maxValue = target.maxHealth;
            targetHealthBar.value = target.health;
        }
        if (targetHealthText != null)
            targetHealthText.text = $"{target.health}/{target.maxHealth}";

        // Display status effects
        if (targetStatusText != null)
        {
            string statusInfo = GetStatusEffectsText(target);
            targetStatusText.text = statusInfo;
        }

        // Listen to target stat changes
        target.onStatsChanged.AddListener(OnTargetStatsChanged);
    }

    private void UnbindCurrentTarget()
    {
        if (m_currentTargetedEnemy != null)
        {
            m_currentTargetedEnemy.onStatsChanged.RemoveListener(OnTargetStatsChanged);
            m_currentTargetedEnemy = null;
        }
    }

    private void OnTargetStatsChanged(int hp, int ap)
    {
        if (m_currentTargetedEnemy == null) return;
        
        if (targetHealthBar != null) targetHealthBar.value = hp;
        if (targetHealthText != null)
            targetHealthText.text = $"{hp}/{m_currentTargetedEnemy.maxHealth}";
    }

    private string GetStatusEffectsText(Combatant target)
    {
        if (target == null || target.ActiveEffects == null || target.ActiveEffects.Count == 0)
            return "No Status Effects";

        List<string> statuses = new List<string>();
        
        foreach (var activeEffect in target.ActiveEffects)
        {
            if (activeEffect?.effect != null)
            {
                // Display effect name and remaining duration
                string effectName = activeEffect.effect.name; // or use a display name property if available
                string durationText = activeEffect.remainingDuration > 0 
                    ? $" ({activeEffect.remainingDuration})" 
                    : "";
                statuses.Add(effectName + durationText);
            }
        }

        if (statuses.Count == 0)
            return "No Status Effects";
        
        return string.Join(", ", statuses);
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
        if (skillsPanel != null) skillsPanel.SetActive(false);

        if (attackButton != null) attackButton.interactable = true;
        if (skillsButton != null) skillsButton.interactable = !attackOnly;
        if (itemsButton != null) itemsButton.interactable = !attackOnly;

        m_menuOptions.Clear();
        if (attackButton != null) m_menuOptions.Add(attackButton);
        if (skillsButton != null) m_menuOptions.Add(skillsButton);
        if (itemsButton != null) m_menuOptions.Add(itemsButton);

        m_selectedIndex = 0;
        m_skillButtonCount = 0;
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
        if (skillsPanel != null) skillsPanel.SetActive(false);
    }

    public void ShowSkills(CombatAction[] availableSkills)
    {
        if (skillsPanel == null) return;

        // Hide main menu, show skills panel
        if (menuPanel != null) menuPanel.SetActive(false);
        skillsPanel.SetActive(true);

        m_menuOptions.Clear();
        m_skillButtonCount = 0;

        // Populate skill buttons based on available skills
        int skillCount = availableSkills != null ? availableSkills.Length : 0;

        if (ConfigureSkillButton(skillButton1, skillButton1Label, skillAPCost1Label, skillCount > 0 ? availableSkills[0] : null, 0))
            m_skillButtonCount++;
        if (ConfigureSkillButton(skillButton2, skillButton2Label, skillAPCost2Label, skillCount > 1 ? availableSkills[1] : null, 1))
            m_skillButtonCount++;
        if (ConfigureSkillButton(skillButton3, skillButton3Label, skillAPCost3Label, skillCount > 2 ? availableSkills[2] : null, 2))
            m_skillButtonCount++;

        // Add back button to options
        if (skillBackButton != null) m_menuOptions.Add(skillBackButton);

        m_selectedIndex = 0;
        RefreshMenuSelection();
    }

    private bool ConfigureSkillButton(Button button, TextMeshProUGUI label, TextMeshProUGUI apLabel, CombatAction skill, int index)
    {
        if (button == null) return false;

        if (skill != null)
        {
            button.gameObject.SetActive(true);
            
            // Check if player has enough AP
            int currentAP = m_boundPlayer != null ? m_boundPlayer.actionPoints : 0;
            bool canAfford = currentAP >= skill.apCost;
            button.interactable = canAfford;
            
            if (label != null)
            {
                label.text = skill.actionName;
                // Gray out if can't afford
                label.color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            
            // apCost int to string, or hide if 0 cost
            if (apLabel != null)
            {
                apLabel.text = skill.apCost > 0 ? $"AP: {skill.apCost}" : "";
                apLabel.color = canAfford ? Color.white : Color.red;
            }

            m_menuOptions.Add(button);
            return true;
        }
        else
        {
            button.gameObject.SetActive(false);
            button.interactable = false;
            if (label != null) label.text = "---";
            return false;
        }
    }

    public void HideSkills()
    {
        if (skillsPanel == null) return;
        skillsPanel.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
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

    private void ConfirmSkillIndex(int index) => onOptionSelected?.Invoke(index);

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