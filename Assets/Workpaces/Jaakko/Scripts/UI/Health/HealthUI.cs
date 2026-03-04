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
    public override void Initialize(Actor actor)
    {
        m_view.Init(actor);

        m_health.OnHealthChanged += m_view.OnHealthChanged;        
    }
    public override void Dispose()
    {
        m_health.OnHealthChanged -= m_view.OnHealthChanged;
    }
    public override void OnActorChanged(Actor actor)
    {
        if (m_health != null)
            m_health.OnHealthChanged -= m_view.OnHealthChanged;

        m_health = actor.Get<HealthComponent>();
        if (m_health != null)
            m_health.OnHealthChanged += m_view.OnHealthChanged;

        m_view.OnActorChanged(actor);
        m_view.OnHealthChanged(m_health.GetHealth());
    }

    public override void Toggle(bool show)
    {
        if (show)
            m_view.View();
        else
            m_view.Hide();
    }
}
