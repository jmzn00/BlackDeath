using System.Xml.Serialization;

public interface IMovementModule
{    
    void Initialize(MovementController controller);
    void Tick();
}
public interface IEnvironmentModule { void UpdateEnviroment(); }
public interface IIntentModule { void UpdateIntent(); }
public interface IImpulseModule { void UpdateImpulse(); }
public interface IForceModule { void UpdateForce(); }
public interface IPostProcessModule { void UpdatePostProcess(); }
