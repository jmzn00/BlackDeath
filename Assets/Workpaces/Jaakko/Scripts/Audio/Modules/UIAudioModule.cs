using UnityEngine;

public class UIAudioModule : AudioModuleBase
{
    private AudioSource m_source;
    private UISFXConfig m_config;

    public UIAudioModule(AudioManager audio) : base(audio) { }

    public override void Activate()
    {
        base.Activate();
        m_source = m_audio.Controller.UISource;
        m_config = m_audio.Controller.UISFX;

        GameEvents.OnUIConfirm  += OnConfirm;
        GameEvents.OnUICancel   += OnCancel;
        GameEvents.OnUINavigate += OnNavigate;
    }

    public override void Deactivate() { }

    public override void Update(float dt) { }

    private void OnConfirm()
    {
        if (m_config?.confirmSound != null)
            m_source.PlayOneShot(m_config.confirmSound);
    }

    private void OnCancel()
    {
        if (m_config?.cancelSound != null)
            m_source.PlayOneShot(m_config.cancelSound);
    }

    private void OnNavigate()
    {
        if (m_config?.navigateSound != null)
            m_source.PlayOneShot(m_config.navigateSound);
    }
}
