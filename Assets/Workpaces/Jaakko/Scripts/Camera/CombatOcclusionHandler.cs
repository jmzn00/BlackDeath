using System.Collections.Generic;
using UnityEngine;

public class CombatOcclusionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask m_buildingLayer;
    [SerializeField] private Behaviour m_deoccluder;
    [SerializeField] private float m_hideRadius = 20f;

    private readonly List<Renderer> m_hiddenRenderers = new();

    private void OnEnable()
    {
        CombatEvents.OnCombatActorsChanged += OnActorsChanged;
        CombatEvents.OnCombatEnded += OnCombatEnded;
    }

    private void OnDisable()
    {
        CombatEvents.OnCombatActorsChanged -= OnActorsChanged;
        CombatEvents.OnCombatEnded -= OnCombatEnded;
    }

    private void OnActorsChanged(List<CombatActor> actors)
    {
        RestoreRenderers();

        if (m_deoccluder != null) m_deoccluder.enabled = false;

        Vector3 combatCenter = Vector3.zero;
        int count = 0;
        foreach (var actor in actors)
        {
            if (actor == null) continue;
            combatCenter += actor.transform.position;
            count++;
        }
        if (count == 0) return;
        combatCenter /= count;

        foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            if (!IsOnBuildingLayer(r.transform)) continue;
            if (Vector3.Distance(r.transform.position, combatCenter) < m_hideRadius)
            {
                r.enabled = false;
                m_hiddenRenderers.Add(r);
            }
        }
    }

    private bool IsOnBuildingLayer(Transform t)
    {
        while (t != null)
        {
            if ((m_buildingLayer.value & (1 << t.gameObject.layer)) != 0)
                return true;
            t = t.parent;
        }
        return false;
    }

    private void OnCombatEnded(CombatResult result)
    {
        RestoreRenderers();
    }

    private void RestoreRenderers()
    {
        if (m_deoccluder != null) m_deoccluder.enabled = true;
        foreach (var r in m_hiddenRenderers)
            if (r != null) r.enabled = true;
        m_hiddenRenderers.Clear();
    }
}
