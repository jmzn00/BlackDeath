using System;

[UIComponent(typeof(InventoryView))]
public class InventoryUI : UIComponentBase
{
    private InventoryView m_view;

    private GameManager m_game;
    private ActorManager m_actorManager;

    public InventoryUI(GameManager game, InventoryView view) : base(game)
    {
        m_game = game;
        m_view = view;

        m_actorManager = m_game.Resolve<ActorManager>();        
    }
    public override void Initialize()
    {
        m_actorManager.OnActorControlChanged += OnActorControlChanged;
        //m_view.Init(m_actorManager.CurrentControlled);
    }
    public override void Dispose()
    {
        m_actorManager.OnActorControlChanged -= OnActorControlChanged;
    }
    private void OnActorControlChanged(Actor actor) 
    {
        m_view.OnActorChanged(actor);
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
