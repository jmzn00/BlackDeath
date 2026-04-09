public class Actor_Player : Actor
{
    private MovementController m_movement;

    public override void Init(GameManager game)
    {
        AddComponent<InventoryComponent>();

        base.Init(game);
        m_movement = Get<MovementController>();
    }
    protected override void GameStateChanged(GameState state)
    {
        switch (state) 
        {
            case GameState.Combat:
                m_movement.enabled = false;
                break;
            case GameState.Dialogue:
                m_movement.enabled = false;
                break;
            case GameState.None:
                m_movement.enabled = true;
                break;
        }
    }
}
