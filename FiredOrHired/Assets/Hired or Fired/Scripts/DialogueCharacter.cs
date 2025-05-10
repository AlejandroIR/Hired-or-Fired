using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Character", menuName = "Dialogue/Character")]

public class DialogueCharacter : ScriptableObject
{
    [Header("Description")]
    public Sprite characterPhoto;
    public string characterName;

    [Header("Dialogue")]
    [SerializeField] public string Message;
    [SerializeField] public string Response_11;
    [SerializeField] public string Response_12;
    [SerializeField] public string Response_13;

    [SerializeField] public string Message_11;
    [SerializeField] public string Message_12;

    [SerializeField] public string Response_21;
    [SerializeField] public string Response_22;
    [SerializeField] public string Response_23;

    [SerializeField] public string Message_21;
    [SerializeField] public string Message_22;

}
