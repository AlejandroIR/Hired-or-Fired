using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public DialogueCharacter dialogueCharacter;

    void Start()
    {
        if (dialogueCharacter == null)
        {
            Debug.LogError("NPCController: No se asign� ning�n DialogueCharacter.");
            return;
        }
        Debug.Log("NPCController: Iniciando di�logo con " + dialogueCharacter.characterName);
        Debug.Log("NPCController: Awake llamado.");
        DialogueUI.instance.StartDialogue(dialogueCharacter);
        DialogueManager.Instance.StartDialogue(dialogueCharacter);
        
    }
}
