using UnityEngine;

[CreateAssetMenu(menuName = "Audio/FootstepBank", fileName = "FootstepBank")]
public class FootstepBank : ScriptableObject
{
    public AudioClip[] clips;
    [Min(0.05f)] public float stepInterval = 0.35f;
    [Min(0.05f)] public float runStepInterval = 0.2f;

    public AudioClip GetRandom()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
