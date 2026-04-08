using System.Collections;
using TMPro;
using UnityEngine;

public class CombatEffectManager : MonoBehaviour
{
    [SerializeField] GameObject damagePopUpPrefab;
    [SerializeField] GameObject particleEffect;

    // All effects triggered by listening to combat events

    private void OnEnable()
    {
        CombatEvents.OnDamageApplied += HandleDamageApplied;
    }

    private void OnDisable()
    {
        CombatEvents.OnDamageApplied -= HandleDamageApplied;
    }

    private void HandleDamageApplied(CombatActor actor, IDamageSource source, float damage)
    {
        // Instantiate damage popup
        if (damagePopUpPrefab != null)
        {
            StartCoroutine(ShowDamagePopup(actor, damage));
        }

        // Instantiate particle effect
        if (particleEffect != null)
        {
            Instantiate(particleEffect, actor.transform.position, Quaternion.identity);
        }
    }

    private IEnumerator ShowDamagePopup(CombatActor actor, float damage)
    {
        GameObject popup = Instantiate(damagePopUpPrefab, actor.transform.position + Vector3.up * 2f, Quaternion.identity);
        TextMeshPro text = popup.GetComponent<TextMeshPro>();
        text.text = damage.ToString("F0");
        // Make the popup float upwards and back down over time
                float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = popup.transform.position;
        Vector3 endPos = startPos + Vector3.up * 1f + (Vector3.right * Random.Range(-0.5f, 0.5f));
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
