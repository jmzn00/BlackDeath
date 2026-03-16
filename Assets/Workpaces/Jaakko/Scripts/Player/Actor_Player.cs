public class Actor_Player : Actor
{    
    public override void Init(GameManager game)
    {
        base.Init(game);
        AddComponent<InventoryComponent>();
    }    
}
