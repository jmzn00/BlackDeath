using UnityEngine;

public class Actor_Player : Actor
{
    private MovementController m_movement;
    public override void Init(GameManager game)
    {
        Debug.Log($"Initializing Actor_Player {name}");
        AddComponent<InventoryComponent>();        

        base.Init(game);
        m_movement = Get<MovementController>();
    }
    public override void Dispose()
    {
        Debug.Log($"Disposing Actor_Player {name}");
        base.Dispose();
    }
    private void OnDestroy()
    {
        Debug.Log($"OnDestroy Actor_Player {name}");
    }
    protected override void GameStateChanged(GameState state)
    {
        if (m_movement == null) 
        {
            Debug.Log($"MovementController is NULL on {name}");
        }
        switch (state) 
        {
            case GameState.Combat:
                m_movement.enabled = false;
                break;
            case GameState.None:
                m_movement.enabled = true;
                break;
            case GameState.Dialogue:
                m_movement.enabled = false;
                break;
        }
    }
    
}
