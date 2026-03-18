using System.Collections.Generic;
using UnityEngine;

public class PhonemeLibrary : MonoBehaviour
{
    [SerializeField] private AudioClip[] m_clips;
    private Dictionary<string, AudioClip> m_phonemeDict = new();

    private void Awake()
    {
        foreach (var clip in m_clips) 
        {
            m_phonemeDict[clip.name.ToUpper()] = clip;
        }
    }
    public AudioClip GetClip(string phoneme) 
    {
        m_phonemeDict.TryGetValue(phoneme.ToUpper(), out var clip);
        return clip;
    }
}
