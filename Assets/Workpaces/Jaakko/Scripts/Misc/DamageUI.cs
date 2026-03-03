using UnityEngine;
using UnityEngine.UI;

public class DamageUI : MonoBehaviour
{
    [SerializeField] private Button m_damageButton;
    [SerializeField] private Button m_healButton;
    private void Start()
    {
        Actor player = Services.Get<ActorManager>().Player;
        m_damageButton.onClick.AddListener(() =>
        {
            player.Get<HealthComponent>().ApplyDamage(null, 10f);
        });
        m_healButton.onClick.AddListener(() =>
        {
            player.Get<HealthComponent>().ApplyHealth(null, 10f);
        });
    }
}
