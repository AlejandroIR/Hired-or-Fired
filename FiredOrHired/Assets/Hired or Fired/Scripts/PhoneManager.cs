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

    private bool puedeAgarrar => ringSound.isPlaying && !yaAgarrado && !confirmacionHecha;

    private bool fueAgarradoManualmente = false;

    private Rigidbody rb;


    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        grabInteractable.selectEntered.AddListener(OnGrab);

        if (grabInteractable != null)
            grabInteractable.enabled = true;

        yaAgarrado = false;
        confirmacionHecha = false;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }


    public void StartPhoneCall()
    {
        if (pokeFilter != null) pokeFilter.enabled = false;
        if (pokeFollowAffordance != null) pokeFollowAffordance.enabled = false;

        yaAgarrado = false;
        confirmacionHecha = false;
        grabInteractable.enabled = true;
        fueAgarradoManualmente = false;


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


    void Update()
    {
        if (puedeAgarrar && Input.GetKeyDown(KeyCode.I))
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


    private void OnGrab(SelectEnterEventArgs args)
    {
        if (args.interactorObject is XRSocketInteractor)
            return;

        if (yaAgarrado || !puedeAgarrar)
            return;

        EjecutarAccionDeAgarrar();
    }


    private void EjecutarAccionDeAgarrar()
    {
        yaAgarrado = true;
        fueAgarradoManualmente = true;

        if (rb != null)
            rb.isKinematic = false;

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

    private void Confirmar()
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

        if (rb != null)
        rb.isKinematic = true; 

        StartCoroutine(DesactivarGrabInteractableConDelay());

    }


    public void ConfirmarDesdeSocket()
    {
        if (!fueAgarradoManualmente || confirmacionHecha)
            return;

        Confirmar();
    }


    public void ResetPhone()
    {
        if (pokeFilter != null) pokeFilter.enabled = false;
        if (pokeFollowAffordance != null) pokeFollowAffordance.enabled = false;

        grabInteractable.enabled = false;

        yaAgarrado = false;
        confirmacionHecha = false;
        fueAgarradoManualmente = false;

        if (rb != null)
            rb.isKinematic = true;


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

    private IEnumerator DesactivarGrabInteractableConDelay()
    {
        // Espera un par de frames para que el socket haga el snap correctamente
        yield return new WaitForSeconds(0.1f); // puedes ajustar si es necesario

        if (grabInteractable != null)
            grabInteractable.enabled = false;
    }


}


