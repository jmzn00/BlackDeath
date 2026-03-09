using UnityEngine;

public abstract class UIComponentBase : IUIComponent
{
    protected GameManager Game;
    protected UIComponentBase(GameManager game)
    {
        Game = game;
    }
    public abstract void Initialize();    
    public abstract void Dispose();
    public abstract void Toggle(bool show);
    public abstract bool IsVisible();
}
