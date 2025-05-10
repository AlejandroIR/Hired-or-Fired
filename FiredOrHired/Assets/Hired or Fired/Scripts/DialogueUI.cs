using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    #region Singleton

    public static DialogueUI instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //Hide dialogue and interaction UI at awake
        dialogueWindow.SetActive(false);
        interactionUI.SetActive(true);
        responseUI.SetActive(false);
    }

    #endregion

    private DialogueManager currentDialogueManager;
    private bool typing;
    private string currentMessage;
    private float startDialogueDelayTimer;

    private DialogueCharacter character;

    [Header("References")]
    [SerializeField] private Image portrait;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject dialogueWindow;
    [SerializeField] private GameObject interactionUI; 
    [SerializeField] private GameObject responseUI;

    [Header("Response Buttons")]
    [SerializeField] private Button[] responseButtons;
    [SerializeField] private TextMeshProUGUI[] responseTexts;

    [Header("Settings")]
    [SerializeField] private bool animateText = true;

    [Range(0.1f, 1f)]
    [SerializeField] private float textAnimationSpeed = 0.5f;

    [Header("Next sentence input")]
    public KeyCode actionInput = KeyCode.Space;

    private void Update()
    {
        //Delay timer
        if (startDialogueDelayTimer > 0f)
        {
            startDialogueDelayTimer -= Time.deltaTime;
        }

        InputUpdate();
    }

    public virtual void InputUpdate()
    {
        //Next dialogue input
        if (Input.GetKeyDown(actionInput))
        {
            NextSentenceSoft();
        }
    }

    /// <summary>
    /// If a sentence is being written and this function is called, the sentence is completed instead of immediately moving to the next sentence.
    /// This function needs to be called twice if you want to switch to a new sentence.
    /// </summary>
    public void NextSentenceSoft()
    {
        if (startDialogueDelayTimer <= 0f)
        {
            if (!typing)
            {
                NextSentenceHard();
            }
            else
            {
                StopAllCoroutines();
                typing = false;
                messageText.text = currentMessage;
            }
        }
    }

    /// <summary>
    /// Even if a sentence is being written, with this function immediately moves to the next sentence.
    /// </summary>
    public void NextSentenceHard()
    {
        //Continue only if we have dialogue
        if (currentDialogueManager == null)
            return;

        //Tell the current dialogue manager to display the next sentence. This function also gives information if we are at the last sentence
        currentDialogueManager.NextSentence(out bool lastSentence);

        //If last sentence remove current dialogue manager
        if (lastSentence)
        {
            currentDialogueManager = null;
        }
    }

    public void StartDialogue(DialogueManager _dialogueManager)
    {
        //Delay timer
        startDialogueDelayTimer = 0.1f;

        //Store dialogue manager
        currentDialogueManager = _dialogueManager;

        //Start displaying dialogue
        currentDialogueManager.StartDialogue();
    }

    public void ShowSentence(DialogueCharacter _dialogueCharacter, string _message)
    {
        StopAllCoroutines();

        dialogueWindow.SetActive(true);

        portrait.sprite = _dialogueCharacter.characterPhoto;
        nameText.text = _dialogueCharacter.characterName;
        currentMessage = _message;

        if (animateText)
        {
            StartCoroutine(WriteTextToTextmesh(_message, messageText));
        }
        else
        {
            messageText.text = _message;
        }
    }

    public void ClearText()
    {
        dialogueWindow.SetActive(false);
    }

    public void ShowInteractionUI(bool _value)
    {
        interactionUI.SetActive(_value);
    }

    public bool IsProcessingDialogue()
    {
        if (currentDialogueManager != null)
        {
            return true;
        }

        return false;
    }

    public bool IsTyping()
    {
        return typing;
    }

    public int CurrentDialogueSentenceLenght()
    {
        if (currentDialogueManager == null)
            return 0;

        return currentDialogueManager.CurrentSentenceLenght();
    }

    IEnumerator WriteTextToTextmesh(string _text, TextMeshProUGUI _textMeshObject)
    {
        typing = true;

        _textMeshObject.text = "";
        char[] _letters = _text.ToCharArray();

        float _speed = 1f - textAnimationSpeed;

        foreach (char _letter in _letters)
        {
            _textMeshObject.text += _letter;

            if (_textMeshObject.text.Length == _letters.Length)
            {
                typing = false;

                ShowResponses();
            }

            yield return new WaitForSeconds(0.1f * _speed);
        }
    }


    public void ShowResponses() 
    {
        responseUI.SetActive(true);

        string[] responses = new string[]
        {
        character.Response_11,
        character.Response_12,
        character.Response_13
        };

        for (int i = 0; i < responseButtons.Length; i++)
        {
            if (!string.IsNullOrEmpty(responses[i]))
            {
                responseButtons[i].gameObject.SetActive(true);
                responseTexts[i].text = responses[i];

                int index = i; // necesario para evitar closure bug en los listeners
                responseButtons[i].onClick.RemoveAllListeners();
                responseButtons[i].onClick.AddListener(() => OnResponseSelected(index));
            }
            else
            {
                responseButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnResponseSelected(int index)
    {
        responseUI.SetActive(false);

        string message = "";
        switch (index)
        {
            case 0: message = currentDialogueManager.GetCurrentCharacter().Message_11; break;
            case 1: message = currentDialogueManager.GetCurrentCharacter().Message_12; break;
            case 2: message = currentDialogueManager.GetCurrentCharacter().Message_12; break;
        }

        ShowSentence(currentDialogueManager.GetCurrentCharacter(), message);
    }



}

