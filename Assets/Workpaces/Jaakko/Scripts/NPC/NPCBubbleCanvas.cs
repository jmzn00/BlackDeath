using UnityEngine;

public class NPCBubbleCanvas : MonoBehaviour
{
    public static NPCBubbleCanvas Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
