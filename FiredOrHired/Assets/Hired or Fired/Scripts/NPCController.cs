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
            Debug.LogError("NPCController: No se asignó ningún DialogueCharacter.");
            return;
        }
        Debug.Log("NPCController: Iniciando diálogo con " + dialogueCharacter.characterName);
        Debug.Log("NPCController: Awake llamado.");
        DialogueUI.instance.StartDialogue(dialogueCharacter);
        DialogueManager.Instance.StartDialogue(dialogueCharacter);
        
    }
}
