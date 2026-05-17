using UnityEngine;

public class MusicModule : AudioModuleBase
{
    private AudioSource m_sourceA;
    private AudioSource m_sourceB;
    private float m_maxVolume;
    private bool m_sourceAIsActive = true;
    private const float FadeSpeed = 1.5f;

    private AudioSource ActiveSource   => m_sourceAIsActive ? m_sourceA : m_sourceB;
    private AudioSource InactiveSource => m_sourceAIsActive ? m_sourceB : m_sourceA;

    public MusicModule(AudioManager audio) : base(audio) { }

    public override void Activate()
    {
        m_sourceA   = m_audio.Controller.MusicSource;
        m_sourceB   = m_audio.Controller.MusicSourceB;
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
        if (m_sourceA == null || m_sourceB == null) return;

        AudioClip clip = state == GameState.Combat
            ? m_audio.Controller.CombatMusic
            : m_audio.Controller.ExplorationMusic;

        if (clip == null) return;
        if (ActiveSource.clip == clip && ActiveSource.isPlaying) return;

        // Start new clip on the inactive source at zero volume
        InactiveSource.clip   = clip;
        InactiveSource.volume = 0f;
        InactiveSource.loop   = true;
        InactiveSource.Play();

        // Swap: the previously inactive source is now the one fading in
        m_sourceAIsActive = !m_sourceAIsActive;
    }

    public override void Update(float dt)
    {
        // Bypass base.Update() — music runs regardless of m_active
        if (m_sourceA == null || m_sourceB == null) return;

        ActiveSource.volume   = Mathf.MoveTowards(ActiveSource.volume,   m_maxVolume, FadeSpeed * dt);
        InactiveSource.volume = Mathf.MoveTowards(InactiveSource.volume, 0f,          FadeSpeed * dt);

        if (InactiveSource.volume <= 0f && InactiveSource.isPlaying)
            InactiveSource.Stop();
    }
}
