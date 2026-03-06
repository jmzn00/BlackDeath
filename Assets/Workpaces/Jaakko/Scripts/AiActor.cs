using UnityEditor.Build;
using UnityEngine;

public class AiActor : Actor
{
    public override void Init(GameManager game)
    {
        AddComponent<HealthComponent>();
        AddComponent<InventoryComponent>();
        AddComponent<CombatActor>();
        

        base.Init(game);
    }
}
