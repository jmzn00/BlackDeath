using UnityEngine;

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
    public virtual bool OnCancel() { return false; }
    public virtual bool OnSubmit() { return false; }
    public virtual bool OnNavigate(Vector2 dir) {  return false; }

    public abstract void Initialize();    
    public abstract void Dispose();
    public abstract void Toggle(bool show);
    public abstract bool IsVisible();
}
