public abstract class UIComponentBase<TGroup> : IUIComponent
    where TGroup : UIViewGroup
{
    protected readonly TGroup m_group;
    protected readonly GameManager m_game;
    protected UIComponentBase(GameManager game, TGroup group)
    {
        m_game = game;
        m_group = group;
    }

    public abstract void Initialize();    
    public abstract void Dispose();
    public abstract void Toggle(bool show);
    public abstract bool IsVisible();
}
