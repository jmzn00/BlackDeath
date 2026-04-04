using System;

public interface IManager
{
    event Action OnReady;
    bool IsReady { get; }
    bool Init();
    void OnManagersInitialzied();
    bool Dispose();
    void Update(float dt);
    void OnSceneLoaded(SceneData data);
}
