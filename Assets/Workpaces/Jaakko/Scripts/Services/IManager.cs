public interface IManager
{
    bool Init(GameManager manager);
    bool Dispose(GameManager manager);
    void Update(float dt);
}
