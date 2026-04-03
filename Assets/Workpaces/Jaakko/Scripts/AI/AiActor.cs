using UnityEngine;

public class AiActor : Actor
{
    public override void Init(GameManager game)
    {                        
        base.Init(game);

        AddComponent<InventoryComponent>();
    }
    private void OnDestroy()
    {
        var am = m_game.Resolve<ActorManager>();
        if (!am.Unregister(this)) 
        {
            Debug.Log($"{name} failed to unregister OnDestroy");
        }
    }
}
