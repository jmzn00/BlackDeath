using System.Collections;
using UnityEngine;

public class DoorTeleporter : MonoBehaviour
{
    [SerializeField] private Transform m_destination;
    [SerializeField] private ScreenFade m_screenFade;
    [SerializeField] private float m_triggerRadius = 2f;
    [SerializeField] private float m_fadeDuration = 0.5f;

    private Transform m_player;
    private MovementController m_movement;
    private bool m_triggered;

    private void Start()
    {
        FindPlayer();

        if (m_destination == null)
            Debug.LogWarning($"DoorTeleporter on {name}: m_destination is not assigned.");
        if (m_screenFade == null)
            Debug.LogWarning($"DoorTeleporter on {name}: m_screenFade is not assigned — teleport will happen without fade.");
    }

    private void OnEnable()
    {
        CombatEvents.OnCombatEnded += OnCombatEnded;
    }

    private void OnDisable()
    {
        CombatEvents.OnCombatEnded -= OnCombatEnded;
    }

    private void OnCombatEnded(CombatResult result)
    {
        FindPlayer();
        m_triggered = false;
    }

    private void FindPlayer()
    {
        // Use MovementController as the authoritative source — it's always on the
        // physically moving object, unlike the "Player" tag which may be on a
        // different static object in the built scene.
        m_movement = Object.FindFirstObjectByType<MovementController>();
        if (m_movement != null)
        {
            m_player = m_movement.transform;
            return;
        }

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning($"DoorTeleporter on {name}: No GameObject tagged 'Player' found.");
            return;
        }
        m_player = playerObj.transform;
        m_movement = playerObj.GetComponentInChildren<MovementController>();

        if (m_movement == null)
            Debug.LogWarning($"DoorTeleporter on {name}: MovementController not found anywhere in the scene.");
    }

    private void Update()
    {
        if (m_player == null || m_triggered) return;
        if (Vector3.Distance(transform.position, m_player.position) < m_triggerRadius)
            StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        m_triggered = true;
        Debug.Log($"DoorTeleporter: TeleportSequence started on {name}. destination={m_destination != null}, fade={m_screenFade != null}, movement={m_movement != null}");

        if (m_screenFade != null)
            yield return StartCoroutine(m_screenFade.FadeToBlack(m_fadeDuration));

        if (m_destination != null)
        {
            if (m_movement != null)
                m_movement.Move(m_destination.position);
            else
                transform.position = m_destination.position;
        }

        if (m_screenFade != null)
            yield return StartCoroutine(m_screenFade.FadeFromBlack(m_fadeDuration));

        m_triggered = false;
    }
}
