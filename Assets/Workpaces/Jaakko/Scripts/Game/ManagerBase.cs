using System;

public class ManagerBase : IManager 
{
    public virtual event Action OnReady;
    public bool IsReady { get; protected set; }

    protected void SetReady() 
    {
        if (IsReady) return;

        IsReady = true;
        OnReady?.Invoke();
    }
    public virtual bool Init() { return true; }
    public virtual bool Dispose() { return true; }
    public virtual void Update(float dt) { }
    public virtual void OnManagersInitialzied() { }
    public virtual void OnSceneLoaded(SceneData data) 
    {
        IsReady = false;
        SetReady();
    }
}