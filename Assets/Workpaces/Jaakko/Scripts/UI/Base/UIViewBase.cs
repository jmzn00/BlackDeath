using UnityEngine;

public class UIViewBase : MonoBehaviour, IUIComponent 
{
    public virtual void SceneChanged(SceneData data) 
    {
    
    }
    public virtual void Initialize() 
    {
    
    }
    public virtual void Dispose() 
    {
    
    }
    public virtual void Toggle(bool show)
    {

    }
    public virtual bool IsVisible() 
    {
        return false;
    }
}