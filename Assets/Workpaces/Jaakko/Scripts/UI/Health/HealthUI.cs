[UIFor(typeof(HealthComponent), typeof(HealthView))]
public class HealthUI : UIComponentBase
{
    private HealthComponent m_health;
    private HealthView m_view;
    public HealthUI(HealthComponent health, HealthView view) 
    {
        m_health = health;
        m_view = view;
    }
    public override void Initialize()
    {
        m_view.Init();

        m_health.OnHealthChanged += m_view.OnHealthChanged;        
    }
    public override void Dispose()
    {
        m_health.OnHealthChanged -= m_view.OnHealthChanged;
    }
    public override void Toggle(bool show)
    {
        if (show)
            m_view.View();
        else
            m_view.Hide();
    }
}
