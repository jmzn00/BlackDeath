public interface IUIComponent
{
    void SceneChanged(SceneData data);
    void Initialize();
    void Dispose();
    void Toggle(bool show);
    bool IsVisible();
}
