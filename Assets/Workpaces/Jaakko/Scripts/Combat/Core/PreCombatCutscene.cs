using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreCombatCutscene : MonoBehaviour
{
    [SerializeField] private NPCSpeechBubble m_bubblePrefab;
    [SerializeField] private float m_cameraTransitionDelay = 0.8f;
    [SerializeField] private float m_lineDisplayTime = 2.5f;
    [SerializeField] private Vector3 m_bubbleOffset = new Vector3(0, 2.5f, 0);

    private void OnEnable()
    {
        CombatEvents.OnPreCombatCutsceneRequested += OnCutsceneRequested;
    }

    private void OnDisable()
    {
        CombatEvents.OnPreCombatCutsceneRequested -= OnCutsceneRequested;
    }

    private void OnCutsceneRequested(List<CombatActor> actors)
    {
        StartCoroutine(PlayCutscene(actors));
    }

    private IEnumerator PlayCutscene(List<CombatActor> actors)
    {
        CameraAnimationEvents.NotifyPresetChange(CameraPresetType.PreCombatDialogue);

        foreach (var actor in actors)
        {
            if (actor == null || actor.Team != Team.Enemy) continue;
            if (actor.PreCombatLines == null || actor.PreCombatLines.Length == 0) continue;

            if (actor.Target != null)
                CameraAnimationEvents.NotifyTargetChanged(actor.Target);

            yield return new WaitForSeconds(m_cameraTransitionDelay);

            foreach (var line in actor.PreCombatLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                Vector3 spawnPos = actor.SpeechBubbleAnchor != null
                    ? actor.SpeechBubbleAnchor.position
                    : actor.transform.position + m_bubbleOffset;

                Transform parent = actor.SpeechBubbleAnchor != null
                    ? actor.SpeechBubbleAnchor
                    : actor.transform;

                NPCSpeechBubble bubble = Instantiate(m_bubblePrefab, spawnPos, Quaternion.identity, parent);
                bubble.SetText(line);

                yield return new WaitForSeconds(m_lineDisplayTime);

                Destroy(bubble.gameObject);
            }
        }

        CombatEvents.PreCombatCutsceneCompleted();
    }
}
