using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR;

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

    private GameObject buttonActive;

    private bool yaAgarrado = false;
    private bool confirmacionHecha = false;

    void Start()
    {
        buttonActive.GetComponent<Collider>().enabled = false;

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        grabInteractable = GetComponent<XRGrabInteractable>();

        // Configurar sonido de llamada en loop hasta que se agarre
        ringSound.loop = true;
        ringSound.Play();

        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void Update()
    {
        // Simulación de agarre con teclado
        if (!yaAgarrado && Input.GetKeyDown(KeyCode.I))
        {
            EjecutarAccionDeAgarrar();
        }

        if (yaAgarrado && !confirmacionHecha)
        {
            // Simulación con teclado
            if (Input.GetKeyDown(KeyCode.O))
            {
                Confirmar();
            }

            // Confirmar con botón de interacción VR (secondaryButton)
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

            if (devices.Count > 0)
            {
                InputDevice rightHand = devices[0];

                if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool pressed) && pressed)
                {
                    Confirmar();
                }
            }
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        EjecutarAccionDeAgarrar();
    }

    private void EjecutarAccionDeAgarrar()
    {
        if (yaAgarrado) return;

        yaAgarrado = true;

        // Detener llamada, reproducir sonido de levantar
        ringSound.Stop();
        ringSound.loop = false; // Por si acaso se cambia al vuelo

        if (!pickUpSound.isPlaying)
            pickUpSound.Play();

        textoUI.gameObject.SetActive(true);

        if (textoUI != null && gameManager != null)
        {
            int index = gameManager.currentNPCIndex - 1;

            if (index >= 0 && index < textosPorNPC.Length)
                textoUI.text = textosPorNPC[index];
            else
                textoUI.text = "error, you fon not lingin :(";
        }
    }

    private void Confirmar()
    {
        if (confirmacionHecha) return;

        confirmacionHecha = true;

        if (!hangUpSound.isPlaying)
            hangUpSound.Play();

        foreach (var obj in cosasAActivar)
        {
            if (obj != null)
                obj.SetActive(true);
        }
        buttonActive.GetComponent<Collider>().enabled = true;

        textoUI.gameObject.SetActive(false);
    }

    public void ResetPhone()
    {
        yaAgarrado = false;
        confirmacionHecha = false;
        buttonActive.GetComponent<Collider>().enabled = false;

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

        // Preparar el ring de nuevo
        if (!ringSound.isPlaying)
        {
            ringSound.loop = true;
            ringSound.Play();
        }
    }
}
