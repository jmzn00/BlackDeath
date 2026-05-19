using TMPro;
using UnityEngine;

public class NPCSpeechBubble : MonoBehaviour
{
    [SerializeField] private TMP_Text m_text;

    private Transform m_cam;

    private void Start()
    {
        m_cam = Camera.main?.transform;
    }

    private void LateUpdate()
    {
        if (m_cam != null)
            transform.rotation = m_cam.rotation;
    }

    public void SetText(string text)
    {
        if (m_text != null)
            m_text.text = text;
    }
}
