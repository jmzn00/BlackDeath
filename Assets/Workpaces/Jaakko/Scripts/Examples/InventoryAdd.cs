using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryAdd : Actor
{
    [SerializeField] private Transform m_buttonSpawn;
    [SerializeField] private GameObject m_buttonPrefab;

    public override void Init(GameManager game)
    {
        var itemManager = game.Resolve<ItemManager>();    
        var player = game.Resolve<ActorManager>().Player;

        foreach (var item in itemManager.GetAllItems())
        {
            Button button = Instantiate(m_buttonPrefab, m_buttonSpawn).GetComponent<Button>();
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();
            text.text = item.name;
            button.onClick.AddListener(() =>
            {
                player.Get<InventoryComponent>().TryAddItem(item.ItemID);
            });
        }

        base.Init(game);        
    }    
}
