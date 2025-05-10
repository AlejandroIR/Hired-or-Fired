using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    private int currentSentence;
    private float coolDownTimer;
    private bool dialogueIsOn;

    [Header("References")]
    [SerializeField] private AudioSource audioSource;


    [Header("Events")]
    public UnityEvent startDialogueEvent;
    public UnityEvent nextSentenceDialogueEvent;
    public UnityEvent endDialogueEvent;

    [Header("Dialogue")]
    [SerializeField] private List<NPC_Centence> sentences = new List<NPC_Centence>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Timer
        if (coolDownTimer > 0f)
        {
            coolDownTimer -= Time.deltaTime;
        }

        //Start dialogue by input
        if (Input.GetKeyDown(DialogueUI.instance.actionInput) && !dialogueIsOn)
        {

            startDialogueEvent.Invoke();

            //If component found start dialogue
            DialogueUI.instance.StartDialogue(this);

            //Hide interaction UI
            DialogueUI.instance.ShowInteractionUI(false);

            dialogueIsOn = true;
        }
    }

    public void StartDialogue()
    {
        //Reset sentence index
        currentSentence = 0;

        //Show first sentence in dialogue UI
        ShowCurrentSentence();

        //Play dialogue sound
        PlaySound(sentences[currentSentence].sentenceSound);

        //Cooldown timer
        coolDownTimer = sentences[currentSentence].skipDelayTime;
    }

    public void NextSentence(out bool lastSentence)
    {
        //The next sentence cannot be changed immediately after starting
        if (coolDownTimer > 0f)
        {
            lastSentence = false;
            return;
        }

        //Add one to sentence index
        currentSentence++;


        nextSentenceDialogueEvent.Invoke();

        //If last sentence stop dialogue and return
        if (currentSentence > sentences.Count - 1)
        {
            StopDialogue();

            lastSentence = true;

            endDialogueEvent.Invoke();

            return;
        }

        //If not last sentence continue...
        lastSentence = false;

        //Play dialogue sound
        PlaySound(sentences[currentSentence].sentenceSound);

        //Show next sentence in dialogue UI
        ShowCurrentSentence();

        //Cooldown timer
        coolDownTimer = sentences[currentSentence].skipDelayTime;
    }

    public void StopDialogue()
    {

        //Hide dialogue UI
        DialogueUI.instance.ClearText();

        //Stop audiosource so that the speaker's voice does not play in the background
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        //Remove trigger refence
        dialogueIsOn = false;
    }

    private void PlaySound(AudioClip _audioClip)
    {
        //Play the sound only if it exists
        if (_audioClip == null || audioSource == null)
            return;

        //Stop the audioSource so that the new sentence does not overlap with the old one
        audioSource.Stop();

        //Play sentence sound
        audioSource.PlayOneShot(_audioClip);
    }

    private void ShowCurrentSentence()
    {
        if (sentences[currentSentence].dialogueCharacter != null)
        {
            //Show sentence on the screen
            DialogueUI.instance.ShowSentence(sentences[currentSentence].dialogueCharacter, sentences[currentSentence].sentence);

            //Invoke sentence event
            sentences[currentSentence].sentenceEvent.Invoke();
        }
        else
        {
            DialogueCharacter _dialogueCharacter = new DialogueCharacter();
            _dialogueCharacter.characterName = "";
            _dialogueCharacter.characterPhoto = null;

            DialogueUI.instance.ShowSentence(_dialogueCharacter, sentences[currentSentence].sentence);

            //Invoke sentence event
            sentences[currentSentence].sentenceEvent.Invoke();
        }
    }

    public int CurrentSentenceLenght()
    {
        if (sentences.Count <= 0)
            return 0;

        return sentences[currentSentence].sentence.Length;
    }

    public DialogueCharacter GetCurrentCharacter()
    {
        if (currentSentence >= 0 && currentSentence < sentences.Count)
            return sentences[currentSentence].dialogueCharacter;
        return null;
    }

    [System.Serializable]
    public class NPC_Centence
    {
        [Header("------------------------------------------------------------")]

        public DialogueCharacter dialogueCharacter;

        [TextArea(3, 10)]
        public string sentence;

        public float skipDelayTime = 0.5f;

        public AudioClip sentenceSound;

        public UnityEvent sentenceEvent;
    }
}


