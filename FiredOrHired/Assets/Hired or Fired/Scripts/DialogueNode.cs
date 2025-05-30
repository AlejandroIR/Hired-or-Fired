using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    [TextArea(3, 10)] public string message;
    public List<DialogueOption> options;
}