using UnityEngine;

public class MusicModule : AudioModuleBase
{
    private AudioSource m_source;
    private float m_maxVolume;
    private float m_currentVolume;
    private float m_targetVolume;
    private AudioClip m_pendingClip;
    private const float FadeSpeed = 1.5f;

    public MusicModule(AudioManager audio) : base(audio) { }

    public override void Activate()
    {
        m_source    = m_audio.Controller.MusicSource;
        m_maxVolume = m_audio.Controller.MusicVolume;
        m_active    = true;
    }

    public override void Deactivate()
    {
        // Intentionally empty — music persists across state changes.
        // PlayForState() handles track switching.
    }

    public void PlayForState(GameState state)
    {
        if (m_source == null) return;

        AudioClip clip = state == GameState.Combat
            ? m_audio.Controller.CombatMusic
            : m_audio.Controller.ExplorationMusic;

        if (clip == null) { m_targetVolume = 0f; return; }
        if (m_source.clip == clip && m_source.isPlaying) return;

        if (!m_source.isPlaying)
        {
            m_source.clip  = clip;
            m_source.loop  = true;
            m_source.Play();
            m_targetVolume = m_maxVolume;
        }
        else
        {
            // Fade out current track then switch
            m_pendingClip  = clip;
            m_targetVolume = 0f;
        }
    }

    public override void Update(float dt)
    {
        // Bypass base.Update() — music runs regardless of m_active
        if (m_source == null) return;

        m_currentVolume = Mathf.MoveTowards(m_currentVolume, m_targetVolume, FadeSpeed * dt);
        m_source.volume = m_currentVolume;

        if (m_pendingClip != null && m_currentVolume <= 0f)
        {
            m_source.clip  = m_pendingClip;
            m_source.loop  = true;
            m_source.Play();
            m_targetVolume = m_maxVolume;
            m_pendingClip  = null;
        }
    }
}
