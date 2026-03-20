public interface IUIComponent
{
    void Initialize();
    void Dispose();
    void Toggle(bool show);
    bool IsVisible();
}
