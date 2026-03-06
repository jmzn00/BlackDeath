public interface IDamageable
{
    void ApplyDamage(float amount, Actor attacker = null);
    void ApplyHealth(float amount, Actor healer = null);
    float GetHealth();
}
