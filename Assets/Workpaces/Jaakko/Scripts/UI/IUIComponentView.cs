public interface IUIComponentView
{
    void Init(Actor actor);
    void View();
    void Hide();
    void OnActorChanged(Actor actor);
}
