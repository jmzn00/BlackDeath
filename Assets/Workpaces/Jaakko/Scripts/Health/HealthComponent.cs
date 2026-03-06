using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IActorComponent
{
    [SerializeField] private float m_maxHealth = 100f;

    public event Action<float> OnHealthChanged;
    private float m_currentHealth;
    public bool Initialize(GameManager game) 
    {
        SetHealth(m_maxHealth);
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor)
    {
        // Subscribe to health changes or other events here
    }
    public bool Dispose() 
    {
        return true;
    }
    public void LoadData(ActorSaveData data) 
    {
        SetHealth(data.Health);
    }
    public void SaveData(ActorSaveData data) 
    {
        data.Health = m_currentHealth;
    }
    public void SetInputSource(IInputSource source) 
    {

    }
    public float GetHealth() 
    {
        return m_currentHealth;
    }
    public void ApplyDamage(float amount, Actor attacker = null) 
    {
        Debug.Log($"Applied {amount} Damage To {name}");
        float newHealth = Mathf.Max(0, m_currentHealth - amount);
        SetHealth(newHealth);
    }
    public void ApplyHealth(float amount, Actor healer = null) 
    {
        Debug.Log($"Applied {amount} Health To {name}");
        float newHealth = Mathf.Min(m_currentHealth + amount, m_maxHealth);
        SetHealth(newHealth);
    }
    private void SetHealth(float value) 
    {
        m_currentHealth = value;

        OnHealthChanged?.Invoke(m_currentHealth);
    }
}
