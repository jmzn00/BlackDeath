public interface IManager
{
    bool Init();
    void OnManagersInitialzied();
    bool Dispose();
    void Update(float dt);
    void OnSceneLoaded(SceneData data);
}
