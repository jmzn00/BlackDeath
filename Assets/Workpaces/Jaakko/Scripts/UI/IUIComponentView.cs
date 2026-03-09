public interface IUIComponentView
{
    void Init();
    void View();
    void Hide();
    void OnActorChanged(Actor actor);
}
