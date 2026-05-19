using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private string m_text = "Hello!";
    [SerializeField] private Transform m_anchor;
    [SerializeField] private NPCSpeechBubble m_bubblePrefab;
    [SerializeField] private float m_triggerRadius = 3f;

    private Transform m_player;
    private NPCSpeechBubble m_activeBubble;
    private bool m_playerInRange;

    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            m_player = playerObj.transform;
    }

    private void Update()
    {
        if (m_player == null) return;

        bool inRange = Vector3.Distance(transform.position, m_player.position) < m_triggerRadius;

        if (inRange && !m_playerInRange)
            ShowBubble();
        else if (!inRange && m_playerInRange)
            HideBubble();

        m_playerInRange = inRange;
    }

    private void ShowBubble()
    {
        if (m_bubblePrefab == null || m_anchor == null) return;
        m_activeBubble = Instantiate(m_bubblePrefab, m_anchor.position, m_anchor.rotation, m_anchor);
        m_activeBubble.SetText(m_text);
    }

    private void HideBubble()
    {
        if (m_activeBubble == null) return;
        Destroy(m_activeBubble.gameObject);
        m_activeBubble = null;
    }
}
