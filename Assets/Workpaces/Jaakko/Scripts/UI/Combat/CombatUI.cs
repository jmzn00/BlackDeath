using System.Runtime.InteropServices;
using UnityEngine;
[UIComponent(typeof(CombatView))]
public class CombatUI : UIComponentBase
{
    private CombatView m_view;
    private CombatManager m_combatManager;

    public CombatUI(GameManager game, CombatView view) : base(game) 
    {
        m_combatManager = game.Resolve<CombatManager>();
        m_view = view;
    }   
    public override void Initialize() 
    {        
        m_combatManager.OnContextChanged += m_view.OnContextChanged;
    }
    public override void Dispose()
    {
        m_combatManager.OnContextChanged -= m_view.OnContextChanged;
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
