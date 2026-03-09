[UIComponent(typeof(HealthView))]
public class HealthUI : UIComponentBase
{
    private HealthComponent m_health;
    private HealthView m_view;
    public HealthUI(GameManager game, HealthView view)  : base(game)
    {
        m_view = view;
    }
    public override void Initialize()
    {
    }
    public override void Dispose()
    {

    }
    public override void Toggle(bool show)
    {
        if (show)
            m_view.View();
        else
            m_view.Hide();
    }
    public override bool IsVisible()
    {
        return m_view.gameObject.activeInHierarchy;
    }
}
