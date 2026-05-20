using TMPro;
using UnityEngine;

public class NPCSpeechBubble : MonoBehaviour
{
    [SerializeField] private TMP_Text m_text;

    private Transform m_cam;

    private void Start()
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = 0;

        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 50;
        }
    }

    private void LateUpdate()
    {
        if (m_cam == null)
            m_cam = Camera.main?.transform;
        if (m_cam != null)
            transform.rotation = m_cam.rotation;
    }

    public void SetText(string text)
    {
        if (m_text != null)
            m_text.text = text;
    }
}
