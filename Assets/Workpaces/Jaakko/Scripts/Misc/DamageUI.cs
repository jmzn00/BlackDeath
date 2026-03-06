using UnityEngine;
using UnityEngine.UI;

public class DamageUI : MonoBehaviour
{
    [SerializeField] private Button m_damageButton;
    [SerializeField] private Button m_healButton;
    private void Start()
    {
        m_damageButton.onClick.AddListener(() =>
        {
            Services.Get<ActorManager>().CurrentControlled.Get<HealthComponent>().ApplyDamage(10f);
        });
        m_healButton.onClick.AddListener(() =>
        {
            Services.Get<ActorManager>().CurrentControlled.Get<HealthComponent>().ApplyHealth(10f);
        });
    }
}
