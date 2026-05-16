using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController I;
    private AudioManager m_audio;

    [Header("AudioSources")]
    [SerializeField] private AudioSource m_combatSource;
    [SerializeField] private AudioSource m_musicSource;
    [SerializeField] private AudioSource m_dialogueSource;

    [Header("Music")]
    [SerializeField] private AudioClip m_explorationMusic;
    [SerializeField] private AudioClip m_combatMusic;
    [SerializeField, Range(0f, 1f)] private float m_musicVolume = 0.6f;

    [Header("Combat SFX")]
    [SerializeField] private CombatSFXConfig m_combatSFX;

    [Header("Footsteps")]
    [SerializeField] private FootstepBank m_humanFootstepBank;

    public AudioSource     CombatSouce      => m_combatSource;
    public AudioSource     MusicSource      => m_musicSource;
    public AudioSource     DialogueSource   => m_dialogueSource;
    public AudioClip       ExplorationMusic => m_explorationMusic;
    public AudioClip       CombatMusic      => m_combatMusic;
    public float           MusicVolume      => m_musicVolume;
    public CombatSFXConfig CombatSFX        => m_combatSFX;
    public FootstepBank    HumanFootstepBank => m_humanFootstepBank;
    
    private void Awake()
    {
        if (I != null) 
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(this);
    }
    private void OnDestroy()
    {
        if (I == this)
            I = null;
    }
    public void Inject(AudioManager audio) 
    {
        m_audio = audio;
    }
}
