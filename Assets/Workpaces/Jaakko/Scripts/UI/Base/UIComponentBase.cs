using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIComponentBase<TGroup> : IUIComponent, IUIInputReceiver
    where TGroup : UIViewGroup
{
    protected readonly TGroup m_group;
    protected readonly GameManager m_game;
    protected readonly UIManager m_ui;
    protected UIComponentBase(GameManager game, TGroup group)
    {
        m_game = game;
        m_group = group;

        m_ui = game.Resolve<UIManager>();
    }
    protected void ViewSelectablesChanged()
    {
        if (!IsVisible()) return;

        List<Selectable> selectables = GetSelectables();

        m_ui.Navigation.UpdateButtons(selectables);
    }
    public virtual void SceneChanged(SceneData data) { }
    public virtual bool OnCancel() { return false; }
    public virtual bool OnSubmit() { return false; }
    public virtual bool OnNavigate(Vector2 dir) {  return false; }
    public virtual void Initialize() 
    {
        List<UIViewBase> views = m_group.GetAllViews();
        for (int i = 0; i < views.Count; i++)
        {
            views[i].OnSelectablesChanged += ViewSelectablesChanged;
        }
        m_group.InitAll();
    }
    public virtual void Dispose() 
    {
        List<UIViewBase> views = m_group.GetAllViews();
        for (int i = 0; i < views.Count; i++)
        {
            views[i].OnSelectablesChanged -= ViewSelectablesChanged;
        }
        m_group.DisposeAll();
    }
    public virtual void Toggle(bool show) 
    {
        if (show) 
        {
            m_ui.PushUI(this);
            UpdateNavigation();
        }
        else 
        {
            m_ui.PopUI(this);
        }
    }
    public virtual bool IsVisible() 
    {
        List<UIViewBase> activeViews = m_group.GetActiveViews();
            
        return activeViews.Count > 0;
    }
    protected void UpdateNavigation() 
    {
        if (!IsVisible()) return;

        List<Selectable> selectables = GetSelectables();

        m_ui.Navigation.UpdateButtons(selectables);
    }
    public virtual List<Selectable> GetSelectables()
    {
        List<Selectable> result = new();

        foreach (var view in m_group.GetActiveViews())
        {
            var viewSelectables = view.GetSelectables();
            if (viewSelectables == null) continue;

            for (int i = 0; i < viewSelectables.Count; i++)
            {
                var s = viewSelectables[i];
                if (s != null && s.gameObject.activeInHierarchy
                    && s.IsInteractable()) 
                {
                    result.Add(s);
                }                    
            }
        }
        return result;
    }
}
