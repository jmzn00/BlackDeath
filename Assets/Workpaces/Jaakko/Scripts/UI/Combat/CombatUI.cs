using UnityEngine;
[UIFor(typeof(CombatActor), typeof(CombatView))]
public class CombatUI : UIComponentBase
{
    private Actor m_actor;
    private CombatActor m_combatActor;
    private CombatView m_view;

    public CombatUI(CombatActor combatActor, CombatView view) 
    {
        m_combatActor = combatActor;
        m_view = view;
    }
    private void OnContextChanged(CombatContext ctx) 
    {
        m_view.OnContextChanged(ctx);
    }
    public override void Initialize(Actor actor) 
    {
        m_actor = actor;
        m_view.Init(actor);

        m_combatActor.OnContextChanged += OnContextChanged;
    }
    public override void Dispose()
    {
        m_combatActor.OnContextChanged -= OnContextChanged;
    }
    public override void Toggle(bool show) 
    {
        if (show)
            m_view.View();
        else
            m_view.Hide();
    }
    public override void OnActorChanged(Actor actor)
    {
        m_view.OnActorChanged(actor);
    }

}
