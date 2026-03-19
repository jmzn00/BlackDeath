using System;
using UnityEngine;

public class DialogueViewGroup : UIViewGroup
{
    public DialogueView dialogueView;

    public override Type ComponentType => typeof(DialogueUI);
}
