using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;




public class PhoneManager : MonoBehaviour
{
    public GameManager gameManager;

    public TMP_Text textoUI;
    public string[] textosPorNPC;

    public AudioSource ringSound;
    public AudioSource pickUpSound;
    public AudioSource hangUpSound;
    private XRGrabInteractable grabInteractable;

    [SerializeField] private List<GameObject> cosasAActivar;

    private bool yaAgarrado = false;
    private bool confirmacionHecha = false;

    public GameObject button;
    public MonoBehaviour pokeFilter;
    public MonoBehaviour pokeFollowAffordance;

    //Light
    public Light luzParpadeo; 
    public float intervaloParpadeo = 0.5f;

    private Coroutine parpadeoCoroutine;

    public XRGrabInteractable grabComponent;


    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }


        ringSound.Play();
        if (luzParpadeo != null)
            parpadeoCoroutine = StartCoroutine(ParpadearLuz());


        if (pokeFilter != null) pokeFilter.enabled = false;
        if (pokeFollowAffordance != null) pokeFollowAffordance.enabled = false;
    }


    void Update()
    {
        if (!yaAgarrado && Input.GetKeyDown(KeyCode.I))
        {
            EjecutarAccionDeAgarrar();
        }

        if (yaAgarrado && !confirmacionHecha)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                Confirmar();
            }
        }
    }

    public void SetPhoneGrabbable(bool canGrab)
    {
        if (grabComponent != null)
            grabComponent.enabled = canGrab;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Ignorar si fue colocado en un socket (no fue agarrado por el jugador)
        if (args.interactorObject is XRSocketInteractor)
            return;

        // Evitar repetir si ya fue agarrado antes
        if (yaAgarrado)
            return;

        EjecutarAccionDeAgarrar();
    }


    private void EjecutarAccionDeAgarrar()
    {
        yaAgarrado = true;

        ringSound.Stop();
        pickUpSound.Play();

        textoUI.gameObject.SetActive(true);

        if (textoUI != null && gameManager != null)
        {
            int index = gameManager.currentNPCIndex - 1; // -1 porque ya se incremento despues del spawn

            if (index >= 0 && index < textosPorNPC.Length)
                textoUI.text = textosPorNPC[index];
            else
                textoUI.text = "error, you fon not lingin :(";
        }
        if (parpadeoCoroutine != null)
        {
            StopCoroutine(parpadeoCoroutine);
            parpadeoCoroutine = null;
            luzParpadeo.enabled = false;
        }
    }

    public void Confirmar()
    {
        if (pokeFilter != null) pokeFilter.enabled = true;
        if (pokeFollowAffordance != null) pokeFollowAffordance.enabled = true;

        confirmacionHecha = true;

        hangUpSound.Play();

        foreach (var obj in cosasAActivar)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        if (textoUI != null)
            textoUI.gameObject.SetActive(false);
    }


    public void ResetPhone()
    {
        if (pokeFilter != null) pokeFilter.enabled = false;
        if (pokeFollowAffordance != null) pokeFollowAffordance.enabled = false;

        yaAgarrado = false;
        confirmacionHecha = false;

        if (textoUI != null)
        {
            textoUI.gameObject.SetActive(true);
            textoUI.text = "you fon lingin";
        }

        foreach (var obj in cosasAActivar)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        if (luzParpadeo != null)
            parpadeoCoroutine = StartCoroutine(ParpadearLuz());

        ringSound.Play();
    }

    private IEnumerator ParpadearLuz()
    {
        while (true)
        {
            luzParpadeo.enabled = !luzParpadeo.enabled;
            yield return new WaitForSeconds(intervaloParpadeo);
        }
    }

}


