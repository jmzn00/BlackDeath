using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguePlayer : MonoBehaviour
{
    Dictionary<string, string[]> wordToPhonemes = new()
    {
        {"HELLO", new string[] {"HE", "L", "OO"}}
    };
    [SerializeField] private PhonemeLibrary library;
    [SerializeField] private AudioSource audioSource;

    public void SpeakWord() 
    {
        StartCoroutine(PlayPhonemes(wordToPhonemes["HELLO"]));
    }
    private IEnumerator PlayPhonemes(string[] phonemes) 
    {
        foreach (var p in phonemes) 
        {
            AudioClip clip = library.GetClip(p);
            if (clip != null) 
            {
                audioSource.PlayOneShot(clip);
                yield return new WaitForSeconds(clip.length * 0.8f);
            }
        }   
    }
}
