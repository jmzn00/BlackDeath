public interface ICombatSystem 
{
    void Init(CombatContext context);
    void Dispose();
    void Reset();
    void Update(float dt);
}