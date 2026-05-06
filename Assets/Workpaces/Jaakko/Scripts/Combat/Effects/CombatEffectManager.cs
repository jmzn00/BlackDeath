using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CombatEffectManager : MonoBehaviour
{
    [SerializeField] private GameObject m_damagePopupPrefab;
    [SerializeField] private GameObject m_gradePopupPrefab;
    [SerializeField] private GameObject m_particleEffect;

    [Header("Screen Shake")]
    [SerializeField] private CinemachineImpulseSource m_impulseSource;
    [SerializeField] private float m_hitShakeForce = 0.3f;

    private void OnEnable()
    {
        CombatEvents.OnDamageApplied += HandleDamageApplied;
        CombatEvents.OnConfirmGraded += HandleConfirmGraded;
    }

    private void OnDisable()
    {
        CombatEvents.OnDamageApplied -= HandleDamageApplied;
        CombatEvents.OnConfirmGraded -= HandleConfirmGraded;
    }

    private void HandleDamageApplied(CombatActor actor, IDamageSource source, float damage)
    {
        if (m_damagePopupPrefab != null)
            StartCoroutine(ShowTextPopup(m_damagePopupPrefab, actor.transform.position, damage.ToString("F0"), Color.white));

        if (m_particleEffect != null)
            Instantiate(m_particleEffect, actor.transform.position, Quaternion.identity);

        if (actor.Team == Team.Player && m_impulseSource != null)
            m_impulseSource.GenerateImpulse(m_hitShakeForce);
    }

    private void HandleConfirmGraded(ActionContext ctx, ConfirmGrade grade)
    {
        if (m_gradePopupPrefab == null || ctx.Source == null) return;

        string label;
        Color color;
        switch (grade)
        {
            case ConfirmGrade.Perfect:
                label = "PERFECT!";
                color = new Color(1f, 0.85f, 0f);
                break;
            case ConfirmGrade.Good:
                label = "GOOD";
                color = new Color(0.6f, 1f, 0.6f);
                break;
            default:
                label = "MISS";
                color = new Color(0.7f, 0.7f, 0.7f);
                break;
        }

        StartCoroutine(ShowTextPopup(m_gradePopupPrefab, ctx.Source.transform.position, label, color));
    }

    private IEnumerator ShowTextPopup(GameObject prefab, Vector3 worldPos, string text, Color color)
    {
        GameObject popup = Instantiate(prefab, worldPos + Vector3.up * 3.5f, Quaternion.identity);
        TextMeshPro tmp = popup.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = popup.transform.position;
        Vector3 endPos = startPos + Vector3.up * 1f + Vector3.right * Random.Range(-0.5f, 0.5f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            popup.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.5f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(popup);
    }
}
