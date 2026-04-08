public interface ICombatSystem 
{
    void Init(CombatContext context);
    void Dispose();
    void Update(float dt);
}