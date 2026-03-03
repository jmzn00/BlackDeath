using UnityEngine;

public class Actor_Player : Actor
{
    public override void Init(GameManager game)
    {
        ActorManager actorManager = game.Resolve<ActorManager>();
        actorManager.AddPlayer(this);
        base.Init(game);
    }
}
