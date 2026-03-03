public interface IDamageable
{
    void ApplyDamage(Actor attacker, float amount);
    void ApplyHealth(Actor healer, float amount);
    float GetHealth();
}
