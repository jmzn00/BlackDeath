using UnityEngine;

public class Actor_Player : Actor
{
    public override void Init(GameManager game)
    {
        AddComponent<HealthComponent>();
        AddComponent<InventoryComponent>();

        base.Init(game);  
        /*
        UIController uiController = FindFirstObjectByType<UIController>();
        if (uiController != null)
        {
            uiController.Init(this);
        }
        else
        {
            Debug.LogError("UI Controller not found in scene");
        }
        */
    }
}
