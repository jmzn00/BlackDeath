using System.Runtime.InteropServices;
using UnityEngine;
[UIComponent(typeof(CombatView))]
public class CombatUI : UIComponentBase
{
    private CombatView m_view;
    private CombatManager m_combatManager;
    private UIManager m_uiManager;

    public CombatUI(GameManager game, CombatView view) : base(game) 
    {
        m_combatManager = game.Resolve<CombatManager>();
        m_uiManager = game.Resolve<UIManager>();
        m_view = view;
    }
    public override void Initialize()
    {
        m_view.Initialize(m_uiManager);
        if (m_combatManager == null) 
        {
            Debug.LogWarning("CombatManager is NULL");
            return;
        }
        if (m_combatManager.ReactiveWindow == null) 
        {
            Debug.LogWarning("Reactive Window is NULL");
            return;
        }

        m_combatManager.OnContextChanged
            += m_view.OnContextChanged;

        m_combatManager.ReactiveWindow.OnConfirmWindowOpened
            += m_view.OnConfirmWindowOpened;
        m_combatManager.ReactiveWindow.OnDodgeWindowOpened
            += m_view.OnDodgeWindowOpened;
        m_combatManager.ReactiveWindow.OnParryWindowOpened
            += m_view.OnParryWindowOpened;
        m_combatManager.ReactiveWindow.OnWindowOpened
            += m_view.OnWindowOpened;
        m_combatManager.OnCombatStarted += m_view.OnCombatStarted;

    }

    public override void Dispose()
    {
        m_combatManager.OnContextChanged
            -= m_view.OnContextChanged;

        m_combatManager.ReactiveWindow.OnConfirmWindowOpened
            -= m_view.OnConfirmWindowOpened;
        m_combatManager.ReactiveWindow.OnDodgeWindowOpened
            -= m_view.OnDodgeWindowOpened;
        m_combatManager.ReactiveWindow.OnParryWindowOpened
            -= m_view.OnParryWindowOpened;
        m_combatManager.ReactiveWindow.OnWindowOpened
            -= m_view.OnWindowOpened;
        m_combatManager.OnCombatStarted -= m_view.OnCombatStarted;
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
