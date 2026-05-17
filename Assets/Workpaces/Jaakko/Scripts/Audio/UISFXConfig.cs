using UnityEngine;

[CreateAssetMenu(menuName = "Audio/UISFXConfig", fileName = "UISFXConfig")]
public class UISFXConfig : ScriptableObject
{
    public AudioClip confirmSound;
    public AudioClip cancelSound;
    public AudioClip navigateSound;
}
