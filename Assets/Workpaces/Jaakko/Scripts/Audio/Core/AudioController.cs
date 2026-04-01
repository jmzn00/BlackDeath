using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController I;
    private AudioManager m_audio;

    [Header("AudioSources")]
    [SerializeField] private AudioSource m_combatSource;
    [SerializeField] private AudioSource m_musicSource;
    [SerializeField] private AudioSource m_dialogueSource;

    public AudioSource CombatSouce => m_combatSource;
    public AudioSource MusicSource => m_musicSource;
    public AudioSource DialogueSource => m_dialogueSource;
    
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
