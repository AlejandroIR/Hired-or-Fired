using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections.Generic;



public class PhoneManager : MonoBehaviour
{
    public GameManager gameManager;

    public TMP_Text textoUI;
    public string[] textosPorNPC;

    public AudioSource ringSound;
    public AudioSource pickUpSound;
    private XRGrabInteractable grabInteractable;

    [SerializeField] private List<GameObject> cosasAActivar;

    private bool yaAgarrado = false;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        grabInteractable = GetComponent<XRGrabInteractable>();
        ringSound.Play();

        // Detener el sonido al agarrar
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void Update()
    {
        // Modo debug: presionar "I" para simular agarrar
        if (!yaAgarrado && Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("DEBUG: Teléfono 'agarrado' con tecla I");
            EjecutarAccionDeAgarrar();
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        EjecutarAccionDeAgarrar();
    }

    private void EjecutarAccionDeAgarrar()
    {
        yaAgarrado = true;

        ringSound.Stop();
        pickUpSound.Play();

        foreach (var obj in cosasAActivar)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        if (textoUI != null && gameManager != null)
        {
            int index = gameManager.currentNPCIndex - 1; // -1 porque ya se incrementó después del spawn

            if (index >= 0 && index < textosPorNPC.Length)
                textoUI.text = textosPorNPC[index];
            else
                textoUI.text = "error, you fon not lingin :(";
        }
    }
}
