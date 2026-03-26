public class AudioModuleBase : IAudioModule 
{
    public AudioModuleBase(AudioManager audio) 
    {
        m_audio = audio;
    }   
    protected bool m_active;
    protected AudioManager m_audio;
    public virtual void Activate() { m_active = true; }
    public virtual void Deactivate() { m_active = false; }
    public virtual void Update(float dt) 
    {
        if (!m_active) return;
    }
}