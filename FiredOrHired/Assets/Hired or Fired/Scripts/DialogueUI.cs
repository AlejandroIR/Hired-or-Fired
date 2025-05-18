using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;
using TMPro;
using System.Net.NetworkInformation;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI instance;

    [Header("Settings")]
    [SerializeField] private bool animateText = true;
    private bool typing;

    [Range(0.1f, 1f)]
    [SerializeField] private float textAnimationSpeed = 0.5f;

    [Header("References")]
    public GameObject dialoguePanel;
    public GameObject responsePanel;
    public Image portrait;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI messageText;

    public Button[] optionButtons;
    public TextMeshProUGUI[] optionTexts;

    private void Awake()
    {
        instance = this;
        dialoguePanel.SetActive(true);
        responsePanel.SetActive(false);
    }

    public void StartDialogue(DialogueCharacter character)
    {
        Debug.Log("UI: StartDialogue llamado con personaje: " + character.characterName);
        portrait.sprite = character.characterPhoto;
        nameText.text = character.characterName;
        messageText.text = character.startMessage;
        dialoguePanel.SetActive(true);

        HideOptions();

        // Mostrar opciones iniciales despu�s de una peque�a pausa si quieres
        Invoke(nameof(ShowInitialOptions), 1f);
    }

    void ShowInitialOptions()
    {
        DialogueManager.Instance.ProceedToNode(0);
    }

    public void ShowNode(DialogueCharacter character, DialogueNode node)
    {
        HideResponses();
        StopAllCoroutines();

        if (animateText)
        {
            StartCoroutine(WriteTextToTextmesh(node.message, messageText));
        }
        else
        {
            messageText.text = node.message;
        }
        //messageText.text = node.message;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < node.options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionTexts[i].text = node.options[i].optionText;

                int index = i; // evita problemas de captura
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() =>
                {
                    DialogueManager.Instance.ProceedToNode(node.options[index].nextNodeIndex);
                });
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideOptions()
    {
        foreach (var button in optionButtons)
        {
            button.gameObject.SetActive(false);
        }
    }

    IEnumerator WriteTextToTextmesh(string _text, TextMeshProUGUI _textMeshObject)
    {
        //HideResponses();
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

    public void ShowResponses() { responsePanel.SetActive(true);}
    public void HideResponses() { responsePanel.SetActive(false);}
}


