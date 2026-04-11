using System;
using System.Collections.Generic;
using UnityEngine;
public abstract class UIViewGroup : MonoBehaviour
{
    public abstract Type ComponentType { get; }
    [SerializeField] private List<UIViewBase> m_views;

    public List<UIViewBase> GetActiveViews() 
    {
        List<UIViewBase> activeViews = new List<UIViewBase>();
        for (int i = 0; i < m_views.Count; i++) 
        {
            if (m_views[i].IsActive())
            {
                activeViews.Add(m_views[i]);
            }
        }
        return activeViews;
    }
    public List<UIViewBase> GetAllViews() 
    {
        return m_views;
    }
    public T Get<T>() where T : UIViewBase 
    {
        for (int i = 0; i < m_views.Count; i++) 
        {
            if (m_views[i] is T t) 
            {
                return t;
            }
        }
        return null;
    }
    public void InitAll() 
    {
        for (int i = 0; i < m_views.Count; i++)
        {
            m_views[i].Init();
        }
    }
    public void DisposeAll() 
    {
        for (int i = 0; i < m_views.Count; i++)
        {
            m_views[i].Dispose();
        }
    }
    public void HideAll() 
    {
        for (int i = 0; i < m_views.Count; i++) 
        {
            m_views[i].Hide();
        }
    }
    public void ViewAll() 
    {
        for (int i = 0; i < m_views.Count; i++) 
        {
            m_views[i].View();
        }
    }
}
