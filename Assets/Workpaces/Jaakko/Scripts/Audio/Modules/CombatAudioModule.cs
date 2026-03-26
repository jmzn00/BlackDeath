using UnityEngine;

public class CombatAudioModule : AudioModuleBase 
{
    public CombatAudioModule(AudioManager audio) : base(audio) { }

    private AudioSource m_source;
    public override void Activate()
    {
        base.Activate();

        m_source = m_audio.Controller.CombatSouce;

        CombatEvents.OnActionResolved += ActionResolved;
        CombatEvents.OnReactionWindowOpened += ActionStarted;
    }
    public override void Deactivate()
    {
        base.Deactivate();

        CombatEvents.OnActionResolved -= ActionResolved;
        CombatEvents.OnReactionWindowOpened -= ActionStarted;
    }
    public override void Update(float dt)
    {
        base.Update(dt);
        
    }
    
    private void ActionStarted(ActionContext ctx) 
    {
        if (ctx.Action.clip == null) return;

        m_source.PlayOneShot(ctx.Action.clip);
    }
    private void ActionResolved(ActionContext ctx, ActionResult res) 
    {
    
    }
}