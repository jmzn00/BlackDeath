using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryAdd : MonoBehaviour
{
    [SerializeField] private Transform m_buttonSpawn;
    [SerializeField] private GameObject m_buttonPrefab;
    private void Start()
    {        
        var itemManager = Services.Get<ItemManager>();
        var player = Services.Get<ActorManager>().Player;

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
    }
}
