using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnTransitionView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup m_canvasGroup;
    [SerializeField] private TMP_Text m_actorNameText;
    [SerializeField] private TMP_Text m_actionText; // NEW: text to show planned action (e.g. "does 'Slash'")
    [SerializeField] private TMP_Text m_targetText; // NEW: text to show planned target (e.g. "at Goblin)
    [SerializeField] private Image m_actorPortraitImage;

    [Header("Defaults")]
    [SerializeField] private float m_defaultDuration = 4f;
    [SerializeField] private float m_fadeDuration = 0.5f;

    private Coroutine m_running;

    private void Reset()
    {
        // attempt to find components if the author forgot to wire them in inspector
        if (m_canvasGroup == null)
            m_canvasGroup = GetComponent<CanvasGroup>();
        if (m_actorNameText == null)
            m_actorNameText = GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
    {
        // ensure initial invisible state via alpha (keeps GameObject active)
        if (m_canvasGroup != null)
        {
            m_canvasGroup.alpha = 0f;
            m_canvasGroup.interactable = false;
            m_canvasGroup.blocksRaycasts = false;
        }

        if (m_actionText != null) m_actionText.text = string.Empty;
        if (m_targetText != null) m_targetText.text = string.Empty;
    }

    // Backwards-compatible overload: no action preview
    public void PlayTransition(CombatActor actor, float duration, Action onComplete)
    {
        PlayTransition(actor, null, null, duration, onComplete);
    }

    // New: accept optional planned action/target name so enemy turns can display "Enemy does X at Y"
    public void PlayTransition(CombatActor actor, string plannedActionName, string plannedTargetName, float duration, Action onComplete)
    {
        if (actor == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (duration <= 0f)
            duration = m_defaultDuration;

        if (m_running != null)
            StopCoroutine(m_running);

        // set content
        if (m_actorNameText != null)
            m_actorNameText.text = actor.name;

        if (m_actionText != null)
            m_actionText.text = string.IsNullOrEmpty(plannedActionName) ? string.Empty : $"does \"{plannedActionName}\"";
        if (m_targetText != null)
            m_targetText.text = string.IsNullOrEmpty(plannedTargetName) ? string.Empty : $"at {plannedTargetName}";

        // optional: if actor exposes a portrait sprite, try to set it
        var portraitSprite = TryGetPortraitSprite(actor);
        if (m_actorPortraitImage != null)
            m_actorPortraitImage.sprite = portraitSprite;

        m_running = StartCoroutine(RunTransition(duration, onComplete));
    }

    private Sprite TryGetPortraitSprite(CombatActor actor)
    {
        // best-effort: if actor has a public sprite field or component, you can extract it here.
        // Keep it safe and non-failing by returning null if none exists.
        return null;
    }

    private IEnumerator RunTransition(float duration, Action onComplete)
    {
        // keep GameObject active at all times; we control visibility with CanvasGroup alpha.
        if (m_canvasGroup != null)
        {
            m_canvasGroup.interactable = false;
            m_canvasGroup.blocksRaycasts = false;
        }

        float fade = Mathf.Min(m_fadeDuration, duration * 0.25f);
        float showTime = Mathf.Max(0f, duration - (fade * 2f));

        // start invisible
        if (m_canvasGroup != null)
            m_canvasGroup.alpha = 0f;

        // fade in
        float t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            if (m_canvasGroup != null)
                m_canvasGroup.alpha = Mathf.Clamp01(t / fade);
            yield return null;
        }
        if (m_canvasGroup != null)
            m_canvasGroup.alpha = 1f;

        // remain visible for showTime
        float waited = 0f;
        while (waited < showTime)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        // fade out
        t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            if (m_canvasGroup != null)
                m_canvasGroup.alpha = 1f - Mathf.Clamp01(t / fade);
            yield return null;
        }
        if (m_canvasGroup != null)
            m_canvasGroup.alpha = 0f;

        // ensure it doesn't block input after transition
        if (m_canvasGroup != null)
        {
            m_canvasGroup.interactable = false;
            m_canvasGroup.blocksRaycasts = false;
        }

        m_running = null;

        onComplete?.Invoke();
    }
}