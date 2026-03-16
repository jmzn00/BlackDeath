using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IActorComponent
{
    private float m_currentHealth;
    public float CurrentHealth => m_currentHealth;

    [SerializeField] private float ch;
    [SerializeField] private float m_maxHealth = 5f;
    public float MaxHealth => m_maxHealth;

    public event Action<float> OnHealthChanged;

    public bool IsDead { get; private set; }

    

    public bool Initialize(GameManager game) 
    {
        SetHealth(m_maxHealth);
        return true;
    }
    public void OnActorComponentsInitialized(Actor actor)
    {
        
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
        float newHealth = Mathf.Max(0, m_currentHealth - amount);
        SetHealth(newHealth);
    }
    public void ApplyHealth(float amount, Actor healer = null) 
    {
        float newHealth = Mathf.Min(m_currentHealth + amount, m_maxHealth);
        SetHealth(newHealth);
    }
    private void SetHealth(float value) 
    {        
        m_currentHealth = value;
        ch = m_currentHealth;

        if (m_currentHealth <= 0f) 
        {
            IsDead = true;
        }
        OnHealthChanged?.Invoke(m_currentHealth);
    }
}
