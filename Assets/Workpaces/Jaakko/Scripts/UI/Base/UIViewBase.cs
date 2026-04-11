using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIViewBase : MonoBehaviour, IUIComponentView
{
    protected List<Selectable> m_selectables = new List<Selectable>();
    public event Action OnSelectablesChanged;

    public virtual bool IsActive() { return gameObject.activeInHierarchy; }
    public virtual void View() 
    {
        gameObject.SetActive(true);
    }
    public virtual void Hide() 
    {
        gameObject.SetActive(false);
    }
    public virtual void Init() { m_selectables.Clear(); }
    public virtual void Dispose() { m_selectables.Clear(); } 
    protected void RaiseSelectablesChanged() 
    {
        OnSelectablesChanged?.Invoke();
    }
    public virtual List<Selectable> GetSelectables() 
    {
        return m_selectables; 
    }
    protected virtual void RegisterSelectable(Selectable selectable) 
    {
        if (!m_selectables.Contains(selectable))
            m_selectables.Add(selectable);
    }
    protected virtual void UnregisterSelectable(Selectable selectable) 
    {
        if (m_selectables.Contains(selectable))
            m_selectables.Remove(selectable);
    }
    protected virtual void ToggleButton(Button button, bool value)
    {
        if (value)
        {
            if (!m_selectables.Contains(button))
                m_selectables.Add(button);

            button.gameObject.SetActive(true);
        }
        else
        {
            if (m_selectables.Contains(button))
                m_selectables.Remove(button);

            button.gameObject.SetActive(false);
        }
        RaiseSelectablesChanged();
    }
}