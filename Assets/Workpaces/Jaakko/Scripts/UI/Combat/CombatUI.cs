using NUnit.Framework;
using System.Collections.Generic;

[UIComponent(typeof(CombatView))]
public class CombatUI : UIComponentBase
{
    private CombatView m_view;
    private UIManager m_uiManager;
    public CombatUI(GameManager game, CombatView view) : base(game) 
    {
        m_view = view;
        m_uiManager = game.Resolve<UIManager>();
    }
    public override void Initialize()
    {
        CombatEvents.OnTurnStarted += TurnStarted;
        CombatEvents.OnCombatActorsChanged += ActorsChanged;

        m_view.Initialize(m_uiManager);
    }
    public override void Dispose()
    {
        CombatEvents.OnTurnStarted -= TurnStarted;
        CombatEvents.OnCombatActorsChanged -= ActorsChanged;
    }
    private void ActorsChanged(List<CombatActor> actors) 
    {
        m_view.ActorsChanged(actors);
    }
    private void TurnStarted(CombatActor actor) 
    {
        if (actor.IsPlayer && !actor.IsDead) 
        {            
            m_view.TurnStarted(actor);
            Toggle(true);
        }
        else 
        {
            Toggle(false);
        }
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
