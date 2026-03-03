public interface IActorComponent
{
    void LoadData(ActorSaveData data);
    void SaveData(ActorSaveData data);
    bool Initialize(GameManager game);
    bool Dispose();
}
