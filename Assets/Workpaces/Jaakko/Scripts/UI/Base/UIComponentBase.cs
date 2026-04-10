using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIComponentBase<TGroup> : IUIComponent, IUIInputReceiver
    where TGroup : UIViewGroup
{
    protected readonly TGroup m_group;
    protected readonly GameManager m_game;
    protected UIComponentBase(GameManager game, TGroup group)
    {
        m_game = game;
        m_group = group;
    }
    public virtual void SceneChanged(SceneData data) { }
    public virtual bool OnCancel() { return false; }
    public virtual bool OnSubmit() { return false; }
    public virtual bool OnNavigate(Vector2 dir) {  return false; }
    public abstract void Initialize();    
    public abstract void Dispose();
    public abstract void Toggle(bool show);
    public abstract bool IsVisible();
    public virtual List<Selectable> GetSelectables() { return null; }

    public virtual event Action<Selectable> OnSelectableAdded;
    public virtual event Action<Selectable> OnSelectableRemoved;

    protected void RaiseSelectableAdded(Selectable s)
    {
        OnSelectableAdded?.Invoke(s);
    }

    protected void RaiseSelectableRemoved(Selectable s)
    {
        OnSelectableRemoved?.Invoke(s);
    }
}
