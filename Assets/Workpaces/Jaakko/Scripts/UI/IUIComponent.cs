public interface IUIComponent
{
    void Initialize(Actor actor);
    void Dispose();
    void OnActorChanged(Actor actor);
    void Toggle(bool show);
}
