using UnityEngine;

[CreateAssetMenu(menuName = "Audio/CombatSFXConfig", fileName = "CombatSFXConfig")]
public class CombatSFXConfig : ScriptableObject
{
    [Header("Hit Results")]
    public AudioClip hitSound;
    public AudioClip parrySound;
    public AudioClip dodgeSound;

    [Header("Confirm Grades")]
    public AudioClip confirmGoodSound;
    public AudioClip confirmPerfectSound;

    [Header("Events")]
    public AudioClip combatStartSound;
    public AudioClip actorDeathSound;
}
