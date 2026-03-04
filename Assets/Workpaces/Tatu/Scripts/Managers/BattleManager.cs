using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Setup")]
    public Transform enemiesParent;
    public BattleCameraController battleCam;
    public BattleUI battleUI;
    [Tooltip("Assign your InputPromptLibrary asset here.")]
    public InputPromptLibrary promptLibrary;

    [Header("Camera Zoom")]
    [SerializeField] private Vector2 normalZoom = new Vector2(75, 130);
    [SerializeField] private Vector2 actionSelectZoom = new Vector2(30, 80);
    [SerializeField] private Vector2 targetSelectZoom = new Vector2(50, 100);
    [SerializeField] private float normalFollowOffset = 0f;
    [SerializeField] private float playerFollowOffset = -5f;

    [Header("Parry")]
    public float parryActiveDuration = 0.2f;
    public float parryCooldown = 1.0f;
    public int parryDamage = 10;

    [Header("Dodge")]
    public float dodgeActiveDuration = 0.4f;
    public float dodgeCooldown = 0.5f;
    [SerializeField] private float dodgeSlideDistance = 2.0f;
    [SerializeField] private float dodgeSlideDuration = 0.08f;
    [SerializeField] private float dodgeReturnDuration = 0.2f;
    //[SerializeField] private bool isDodging = false;
    private Coroutine m_dodgeCoroutine;

    [Header("TestAudio")]
    [SerializeField] private AudioClip parrySound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip dodgeSound;
    [SerializeField] private AudioSource battleAudioSource;


    // runtime
    private List<Combatant> m_battlers = new List<Combatant>();
    private Combatant m_player;
    private int m_turnIndex = 0;
    private bool m_battleRunning = false;
    private bool m_actionResolved = false;

    // Parry state machine
    private bool m_parryActive = false;
    private float m_parryActiveEnd = 0f;
    private bool m_parryOnCooldown = false;
    private float m_parryCooldownEnd = 0f;

    // Dodge state machine
    private bool m_dodgeActive = false;
    private float m_dodgeActiveEnd = 0f;
    private bool m_dodgeOnCooldown = false;
    private float m_dodgeCooldownEnd = 0f;

    // Player offensive prompt
    private InputPrompt m_activePrompt = null;
    private bool m_promptSuccess = false;

    // Current actor
    private Combatant m_currentActor;

    // ── Unity ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (battleCam == null) battleCam = FindAnyObjectByType<BattleCameraController>();
        if (battleUI == null) battleUI = FindAnyObjectByType<BattleUI>();
        if (battleAudioSource == null) battleAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        UpdateDefenceStates();

        var inputManager = Services.Get<InputManager>();
        var actions = inputManager?.InputActions;

        if (m_currentActor == m_player)
        {
            // Player offensive prompt, check correct button presses
            if (m_activePrompt != null && !m_promptSuccess)
            {
                switch (m_activePrompt.inputType)
                {
                    case PromptInputType.Confirm: if (WasConfirmPressed(actions)) m_promptSuccess = true; break;
                    case PromptInputType.Parry: if (WasParryPressed(actions)) m_promptSuccess = true; break;
                    case PromptInputType.Dodge: if (WasDodgePressed(actions)) m_promptSuccess = true; break;
                }
            }
        }
        else if (m_currentActor != null)
        {
            // Enemy turn - defensive inputs parry/dodge
            if (!m_parryActive && !m_parryOnCooldown && !m_dodgeActive && WasParryPressed(actions))
                ActivateParry();

            if (!m_dodgeActive && !m_dodgeOnCooldown && !m_parryActive && WasDodgePressed(actions))
                ActivateDodge();
        }
    }

    // Defence state machines

    private void UpdateDefenceStates()
    {
        if (m_parryActive && Time.time >= m_parryActiveEnd)
        {
            m_parryActive = false;
            battleUI?.SetParryIndicator(false);
        }
        if (m_parryOnCooldown && Time.time >= m_parryCooldownEnd)
            m_parryOnCooldown = false;

        if (m_dodgeActive && Time.time >= m_dodgeActiveEnd)
        {
            m_dodgeActive = false;
            battleUI?.SetDodgeIndicator(false);
        }
        if (m_dodgeOnCooldown && Time.time >= m_dodgeCooldownEnd)
            m_dodgeOnCooldown = false;
    }

    private void ActivateParry()
    {
        // Cancel dodge if active
        if (m_dodgeActive)
        {
            m_dodgeActive = false;
            battleUI?.SetDodgeIndicator(false);
        }
        m_parryActive = true;
        m_parryActiveEnd = Time.time + parryActiveDuration;
        // Cooldown resets at OnWindowClose — not here — so every hit is fair game.
        battleUI?.SetParryIndicator(true);
    }

    private void ActivateDodge()
    {
        // Cancel parry if active
        if (m_parryActive)
        {
            m_parryActive = false;
            battleUI?.SetParryIndicator(false);
        }
        m_dodgeActive = true;
        m_dodgeActiveEnd = Time.time + dodgeActiveDuration;
        // Cooldown resets at OnWindowClose — not here.
        m_dodgeOnCooldown = true;
        m_dodgeCooldownEnd = Time.time + dodgeCooldown;
        // Trigger the slide visual
        StartDodgeVisual(m_player?.gameObject);
        battleUI?.SetDodgeIndicator(true);
    }

    // Animation events (call in animation)

    public void OnWindowOpen(Combatant attacker, string promptKey)
    {
        m_activePrompt = promptLibrary != null ? promptLibrary.Get(promptKey) : null;
        m_promptSuccess = false;

        if (m_activePrompt != null)
            battleUI?.ShowReactivePrompt(m_activePrompt.label, m_activePrompt.icon);
        else
            Debug.LogWarning($"[BattleManager] InputPrompt key not found: '{promptKey}'");
    }

    public void OnWindowClose(Combatant attacker)
    {
        battleUI?.HideReactivePrompt();

        bool isPlayerAttacking = attacker == m_player;
        battleAudioSource?.PlayOneShot(hitSound);

        if (isPlayerAttacking)
        {
            bool confirmed = m_activePrompt == null || m_promptSuccess;
            attacker.ApplyHitDamage(dodged: false, parried: confirmed);
            m_activePrompt = null;
            m_promptSuccess = false;
        }
        else
        {
            // Parry takes priority if somehow both active (should not happen but never too sure)
            bool parried = m_parryActive;
            bool dodged = m_dodgeActive && !parried;

            attacker.ApplyHitDamage(dodged: dodged, parried: parried);

            if (parried)
            {
                attacker.ApplyDamage(parryDamage);
                battleUI?.ShowReactivePrompt("PARRY!");
                battleAudioSource?.PlayOneShot(parrySound);
            }
            else if (dodged)
            {
                battleUI?.ShowReactivePrompt("DODGE!");
                battleAudioSource?.PlayOneShot(dodgeSound);
            }

            if (parried || dodged)
                StartCoroutine(Co_HidePromptAfterDelay(0.4f));
        }

        // Reset both states on close, no stunlocking 
        if (m_parryActive) { m_parryActive = false; battleUI?.SetParryIndicator(false); }
        if (m_dodgeActive) { m_dodgeActive = false; battleUI?.SetDodgeIndicator(false); }

        // Reset both cooldowns
        m_parryOnCooldown = false;
        m_dodgeOnCooldown = false;
    }

    public void OnActionResolved(Combatant actor)
    {
        m_actionResolved = true;
    }

    public void StartBattle(BattleArea area)
    {
        if (m_battleRunning) return;
        StartCoroutine(Co_StartBattle(area));
    }

    public IReadOnlyList<Combatant> GetBattlers() => m_battlers;

    // Battle start and setup

    private IEnumerator Co_StartBattle(BattleArea area)
    {
        m_battleRunning = true;
        m_battlers.Clear();

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError("BattleManager: no GameObject tagged 'Player'.");
            m_battleRunning = false;
            yield break;
        }

        if (area.playerSpawnPoint != null)
            playerGO.transform.SetPositionAndRotation(
                area.playerSpawnPoint.position, area.playerSpawnPoint.rotation);

        var controller = playerGO.GetComponent<Character_Controller>();
        if (controller != null) controller.enabled = false;

        m_player = playerGO.GetComponent<Combatant>() ?? playerGO.AddComponent<Combatant>();
        m_battlers.Add(m_player);

        int spawnCount = Mathf.Min(area.enemiesToSpawn.Length, area.enemySpawnPoints.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            var prefab = area.enemiesToSpawn[i];
            var point = area.enemySpawnPoints[i];
            if (prefab == null || point == null) continue;
            var go = Instantiate(prefab, point.position, point.rotation, enemiesParent);
            var c = go.GetComponent<Combatant>() ?? go.AddComponent<Combatant>();
            m_battlers.Add(c);
        }

        if (battleUI != null)
        {
            battleUI.BindPlayer(m_player);
            battleUI.BindEnemies(GetAliveEnemies());
        }

        SetCameraTarget(playerGO, normalZoom, normalFollowOffset);
        yield return new WaitForSeconds(0.25f);

        m_turnIndex = 0;
        StartCoroutine(Co_BattleLoop());
    }

    // Battle loop / turns. Stuns etc are to be tested

    private IEnumerator Co_BattleLoop()
    {
        while (m_battleRunning)
        {
            if (m_turnIndex >= m_battlers.Count) m_turnIndex = 0;

            var actor = m_battlers[m_turnIndex];
            m_currentActor = actor;

            if (actor == null || !actor.gameObject.activeSelf || actor.health <= 0)
            {
                AdvanceTurn();
                m_currentActor = null;
                yield return null;
                continue;
            }

            actor.ProcessTurnStart();

            if (actor.health <= 0)
            {
                if (IsBattleOver(out bool wonEarly)) { m_currentActor = null; EndBattle(wonEarly); yield break; }
                AdvanceTurn();
                m_currentActor = null;
                yield return null;
                continue;
            }

            bool actionPrevented = actor.IsActionPrevented();
            ShowTransition(actionPrevented
                ? $"{actor.gameObject.name} is stunned!"
                : $"{actor.gameObject.name}'s Turn");
            yield return new WaitForSeconds(0.6f);
            HideTransition();

            if (!actionPrevented)
            {
                if (actor == m_player)
                    yield return StartCoroutine(Co_PlayerTurn(actor));
                else
                    yield return StartCoroutine(Co_EnemyTurn(actor));
            }

            actor.ProcessTurnEnd();

            if (IsBattleOver(out bool playerWon)) { m_currentActor = null; EndBattle(playerWon); yield break; }
            AdvanceTurn();
            m_currentActor = null;
            yield return null;
        }
    }

    // Player Turn

    private IEnumerator Co_PlayerTurn(Combatant actor)
    {
        ReturnToIdle(actor);
        var playerGO = actor.gameObject;
        var inputManager = Services.Get<InputManager>();
        var actions = inputManager?.InputActions;

        SetCameraTarget(playerGO, actionSelectZoom, playerFollowOffset);

        int chosenActionIndex = -1;
        battleUI?.ShowMenu();
        if (battleUI != null) battleUI.onOptionSelected = idx => chosenActionIndex = idx;

        float navCooldown = 0f;
        while (chosenActionIndex < 0)
        {
            navCooldown -= Time.deltaTime;
            if (navCooldown <= 0f)
            {
                bool up = false, down = false, confirm = false;
                var kb = Keyboard.current;
                if (kb != null)
                {
                    up = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
                    down = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
                    confirm = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame;
                }
                try
                {
                    if (actions != null)
                    {
                        var move = actions.Player.Move.ReadValue<UnityEngine.Vector2>();
                        if (move.y > 0.5f) up = true;
                        if (move.y < -0.5f) down = true;
                        if (actions.Player.Confirm.WasPressedThisFrame()) confirm = true;
                    }
                }
                catch { }

                if (up) { battleUI?.Previous(); navCooldown = 0.2f; }
                if (down) { battleUI?.Next(); navCooldown = 0.2f; }
                if (confirm) { chosenActionIndex = battleUI?.SelectedIndex ?? 0; }
            }
            yield return null;
        }

        battleUI?.HideMenu();

        CombatAction action = PickPlayerAction(actor, chosenActionIndex);
        if (action == null) yield break;

        var enemies = GetAliveEnemies();
        if (enemies.Count == 0) yield break;

        Combatant target;
        if (enemies.Count == 1)
        {
            target = enemies[0];
        }
        else
        {
            SetCameraTarget(playerGO, targetSelectZoom, normalFollowOffset);
            int targetIndex = 0;
            battleUI?.ShowTargetSelection(enemies, targetIndex);
            bool targetConfirmed = false;
            navCooldown = 0f;

            while (!targetConfirmed)
            {
                navCooldown -= Time.deltaTime;
                if (navCooldown <= 0f)
                {
                    bool right = false, left = false, confirm = false;
                    var kb = Keyboard.current;
                    if (kb != null)
                    {
                        right = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
                        left = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
                        confirm = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame;
                    }
                    try
                    {
                        if (actions != null)
                        {
                            var move = actions.Player.Move.ReadValue<UnityEngine.Vector2>();
                            if (move.x > 0.5f) right = true;
                            if (move.x < -0.5f) left = true;
                            if (actions.Player.Confirm.WasPressedThisFrame()) confirm = true;
                        }
                    }
                    catch { }

                    if (right) { targetIndex = (targetIndex + 1) % enemies.Count; battleUI?.UpdateTargetSelection(enemies, targetIndex); navCooldown = 0.2f; }
                    if (left) { targetIndex = (targetIndex - 1 + enemies.Count) % enemies.Count; battleUI?.UpdateTargetSelection(enemies, targetIndex); navCooldown = 0.2f; }
                    if (confirm) targetConfirmed = true;
                }
                yield return null;
            }

            battleUI?.HideTargetSelection();
            target = enemies[targetIndex];
        }

        SetCameraTarget(playerGO, normalZoom, normalFollowOffset);
        yield return StartCoroutine(Co_ExecuteAction(actor, action, target));
    }

    // Enemy Turn
    private IEnumerator Co_EnemyTurn(Combatant actor)
    {
        yield return new WaitForSeconds(0.4f);
        CombatAction action = actor.ChooseAction(this);
        if (action == null) yield break;
        battleUI?.ShowReactivePrompt("DODGE / PARRY!");
        yield return StartCoroutine(Co_ExecuteAction(actor, action, m_player));
        battleUI?.HideReactivePrompt();
    }

    // Action execution

    private IEnumerator Co_ExecuteAction(Combatant actor, CombatAction action, Combatant target)
    {
        m_actionResolved = false;
        actor.PlayAction(action, target);

        if (action.animationClip == null) yield break;

        float timeout = action.animationClip.length + 1f;
        float elapsed = 0f;
        while (!m_actionResolved && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!m_actionResolved)
            Debug.LogWarning($"[BattleManager] ResolveAction() event missing on '{action.animationClip.name}'. Advanced via timeout.");

        ReturnToIdle(actor);
    }

    // Helpers

    private IEnumerator Co_HidePromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        battleUI?.HideReactivePrompt();
    }

    private List<Combatant> GetAliveEnemies()
    {
        var list = new List<Combatant>();
        foreach (var b in m_battlers)
        {
            if (b == null || b == m_player) continue;
            if (b.gameObject.activeSelf && b.health > 0) list.Add(b);
        }
        return list;
    }

    private bool IsBattleOver(out bool playerWon)
    {
        playerWon = false;
        if (m_player == null || m_player.health <= 0) return true;
        if (GetAliveEnemies().Count == 0) { playerWon = true; return true; }
        return false;
    }

    private void EndBattle(bool playerWon)
    {
        m_battleRunning = false;
        Debug.Log($"[BattleManager] Battle ended. Player won: {playerWon}");

        var controller = m_player?.GetComponent<Character_Controller>();
        if (controller != null) controller.enabled = true;

        if (battleUI != null)
        {
            battleUI.HideMenuImmediate();
            battleUI.HideTransitionImmediate();
            battleUI.HideReactivePrompt();
            battleUI.HideTargetSelection();
            battleUI.SetParryIndicator(false);
            battleUI.SetDodgeIndicator(false);
            battleUI.UnbindPlayer();
            battleUI.UnbindAllEnemies();
        }

        if (m_player != null)
            SetCameraTarget(m_player.gameObject, normalZoom, normalFollowOffset);
    }

    public void ReturnToIdle(Combatant actor)
    {
        actor.animator.Play(actor.idleAnim.name);
    }

    private void AdvanceTurn() =>
        m_turnIndex = (m_turnIndex + 1) % Mathf.Max(1, m_battlers.Count);

    private CombatAction PickPlayerAction(Combatant actor, int menuIndex)
    {
        if (actor.availableActions == null || actor.availableActions.Length == 0) return null;
        return actor.availableActions[Mathf.Min(menuIndex, actor.availableActions.Length - 1)];
    }

    private void SetCameraTarget(GameObject target, UnityEngine.Vector2 zoom, float followOffset) =>
        battleCam?.SetZoom(zoom, target, followOffset);

    private void ShowTransition(string text) => battleUI?.ShowTransition(text);
    private void HideTransition() => battleUI?.HideTransition();

    // Input helpers and dodge visual (will be replaced maybe with animation events=)

    private void StartDodgeVisual(GameObject go)
    {
        if (m_dodgeCoroutine != null) StopCoroutine(m_dodgeCoroutine);
        m_dodgeCoroutine = StartCoroutine(Co_DoDodgeSlide(go));
    }

    private IEnumerator Co_DoDodgeSlide(GameObject go)
    {
        if (go == null) yield break;

        var tr = go.transform;
        Vector3 start = tr.position;
        Vector3 back = -tr.right * dodgeSlideDistance;
        Vector3 target = start + back;

        // Slide backwards
        float t = 0f;
        while (t < dodgeSlideDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / dodgeSlideDuration));
            tr.position = Vector3.Lerp(start, target, f);
            yield return null;
        }
        tr.position = target;

        // Return to anchor
        t = 0f;
        while (t < dodgeReturnDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / dodgeReturnDuration));
            tr.position = Vector3.Lerp(target, start, f);
            yield return null;
        }
        tr.position = start;
        m_dodgeCoroutine = null;
    }

    private static bool WasConfirmPressed(InputSystem_Actions actions)
    {
        var kb = Keyboard.current;
        if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) return true;
        try { return actions != null && actions.Player.Confirm.WasPressedThisFrame(); } catch { return false; }
    }

    private static bool WasParryPressed(InputSystem_Actions actions)
    {
        var kb = Keyboard.current;
        if (kb != null && kb.leftShiftKey.wasPressedThisFrame) return true;
        try { return actions != null && actions.Player.Parry.WasPressedThisFrame(); } catch { return false; }
    }

    private static bool WasDodgePressed(InputSystem_Actions actions)
    {
        var kb = Keyboard.current;
        if (kb != null && kb.leftAltKey.wasPressedThisFrame) return true;
        try { return actions != null && actions.Player.Dodge.WasPressedThisFrame(); } catch { return false; }
    }
}