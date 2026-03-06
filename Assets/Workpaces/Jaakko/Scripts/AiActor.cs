public class AiActor : Actor
{
    public override void Init(GameManager game)
    {                        
        base.Init(game);

        AddComponent<InventoryComponent>();

        OnActorComponentsInitialized();
    }
}
