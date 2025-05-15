using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private DialogueCharacter currentCharacter;
    private int currentNodeIndex;

    void Awake()
    {
        // Asegúrate de que la instancia se inicializa solo una vez
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Si deseas que persista entre escenas
        }
        else
        {
            Destroy(gameObject); // Si ya hay una instancia, destruye esta
        }
    }

    public void StartDialogue(DialogueCharacter character)
    {
        currentCharacter = character;
        currentNodeIndex = -1;
       //DialogueUI.instance.StartDialogue(character.startMessage, character.characterName, character.characterPhoto);
    }

    public void ProceedToNode(int nodeIndex)
    {
        currentNodeIndex = nodeIndex;
        DialogueNode node = currentCharacter.nodes[nodeIndex];
        DialogueUI.instance.ShowNode(currentCharacter, node);
    }
}
