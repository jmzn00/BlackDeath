public interface IActorComponent
{
    void SetInputSource(IInputSource source);
    void LoadData(ActorSaveData data);
    void SaveData(ActorSaveData data);
    bool Initialize(GameManager game);
    void OnActorComponentsInitialized(Actor actor);
    bool Dispose();
}
