using UnityEngine;

public class Actor_Player : Actor
{
    public override void Init(GameManager game)
    {
        AddComponent<HealthComponent>();
        AddComponent<InventoryComponent>();

        base.Init(game);  
    }
}
