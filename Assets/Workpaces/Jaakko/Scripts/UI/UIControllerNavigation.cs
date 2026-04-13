using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class UIControllerNavigation
{
    private List<Selectable> m_selectables = new();
    private int m_currentIndex = 0;

    private InputManager m_input;
    public UIControllerNavigation(InputManager inputManager)
    {
        m_input = inputManager;
    }
    public void Dispose()
    {
        m_selectables.Clear();

    }
    public void UpdateButtons(List<Selectable> selectables, GameObject currentSelected = null)
    {
        if (selectables == null || selectables.Count == 0)
        {
            m_selectables.Clear();
            return;
        }

        m_selectables = selectables;
        if (currentSelected != null && m_selectables.Any(s => s.gameObject == currentSelected))
        {
            EventSystem.current.SetSelectedGameObject(currentSelected);
            m_currentIndex = m_selectables.FindIndex(s => s.gameObject == currentSelected);
        }
        else if (m_selectables.Count > 0)
        {
            m_currentIndex = 0;
            EventSystem.current.SetSelectedGameObject(m_selectables[0].gameObject);
        }
    }
    public void UpdateNavigation()
    {
        if (m_selectables == null || m_selectables.Count == 0)
        {
            return;
        }

        ref UIInputState input = ref m_input.GetUIInputState();

        if (input.NavigateUpPressed)
        {
            MoveUp();
        }
        else if (input.NavigateDownPressed)
        {
            MoveDown();
        }
        else if (input.NavigateRightPressed)
        {
            MoveRight();
        }
        else if (input.NavigateLeftPressed)
        {
            MoveLeft();
        }
    }
    private void SetCurrentSelected(GameObject go)
    {
        EventSystem.current.SetSelectedGameObject(go);
    }
    private void MoveUp()
    {
        m_currentIndex = (m_currentIndex - 1 + m_selectables.Count) % m_selectables.Count;
        SetCurrentSelected(m_selectables[m_currentIndex].gameObject);
    }
    private void MoveDown()
    {
        m_currentIndex = (m_currentIndex + 1) % m_selectables.Count;
        SetCurrentSelected(m_selectables[m_currentIndex].gameObject);
    }
    private void MoveRight()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        var next = m_selectables
            .Where(s => s.gameObject != current)
            .OrderBy(s => s.transform.position.x)
            .FirstOrDefault(s => s.transform.position.x > current.transform.position.x);

        if (next != null)
        {
            SetCurrentSelected(next.gameObject);
        }
    }

    private void MoveLeft()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == null) return;

        var next = m_selectables
            .Where(s => s.gameObject != current)
            .OrderByDescending(s => s.transform.position.x)
            .FirstOrDefault(s => s.transform.position.x < current.transform.position.x);

        if (next != null)
        {
            SetCurrentSelected(next.gameObject);
        }
    }
}