public interface ICameraMode
{
    bool CanEnter();
    void Enter();
    void Exit();
    void Update(float dt);

}
