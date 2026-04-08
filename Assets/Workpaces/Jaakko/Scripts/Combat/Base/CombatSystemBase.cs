public class CombatSystemBase : ICombatSystem 
{
    public virtual void Init(CombatContext context) { }
    public virtual void Dispose() { }
    public virtual void Update(float dt) { }
    public virtual void Reset() { }
}