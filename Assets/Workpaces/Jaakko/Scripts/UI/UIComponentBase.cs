using UnityEngine;

public abstract class UIComponentBase : IUIComponent
{
    public abstract void Initialize();    
    public abstract void Dispose();
    public abstract void Toggle(bool show);
}
