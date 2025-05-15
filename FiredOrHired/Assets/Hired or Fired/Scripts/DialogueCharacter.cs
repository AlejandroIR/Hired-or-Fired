using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Character", menuName = "Dialogue/Character")]
public class DialogueCharacter : ScriptableObject
{
    public Sprite characterPhoto;
    public string characterName;

    [TextArea(3, 10)] public string startMessage;

    public List<DialogueNode> nodes;
}
