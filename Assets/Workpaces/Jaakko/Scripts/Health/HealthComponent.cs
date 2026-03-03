using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable, IActorComponent
{
    private float m_currentHealth;
    public bool Initialize(GameManager game) 
    {
        return true;
    }
    public bool Dispose() 
    {
        return true;
    }
    public void LoadData(ActorSaveData data) 
    {
        m_currentHealth = data.Health;
    }
    public void SaveData(ActorSaveData data) 
    {
        data.Health = m_currentHealth;
    }
    public float GetHealth() 
    {
        return m_currentHealth;
    }
    public void ApplyDamage(Actor attacker, float amount) 
    {
        
    }
    public void ApplyHealth(Actor healer, float amount) 
    {

    }
}
