using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    [SerializeField] private Image m_overlay;

    private void Awake()
    {
        if (m_overlay != null)
            m_overlay.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator FadeToBlack(float duration)
    {
        yield return Fade(0f, 1f, duration);
    }

    public IEnumerator FadeFromBlack(float duration)
    {
        yield return Fade(1f, 0f, duration);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            m_overlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        m_overlay.color = new Color(0, 0, 0, to);
    }
}
