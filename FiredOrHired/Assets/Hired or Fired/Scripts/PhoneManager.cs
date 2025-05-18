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

    private bool yaAgarrado = false;
    private bool confirmacionHecha = false;

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
        // Simulación de agarre con tecla "I"
        if (!yaAgarrado && Input.GetKeyDown(KeyCode.I))
        {
            EjecutarAccionDeAgarrar();
        }

        // Confirmar solo si ya se agarró y no se ha confirmado aún
        if (yaAgarrado && !confirmacionHecha)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                Confirmar();
            }

            // Confirmar con botón del mando (VR)
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

            if (devices.Count > 0)
            {
                InputDevice rightHand = devices[0];

                if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed) && pressed)
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
        yaAgarrado = true;

        ringSound.Stop();
        pickUpSound.Play();

        textoUI.gameObject.SetActive(true);

        if (textoUI != null && gameManager != null)
        {
            int index = gameManager.currentNPCIndex - 1; // -1 porque ya se incrementó después del spawn

            if (index >= 0 && index < textosPorNPC.Length)
                textoUI.text = textosPorNPC[index];
            else
                textoUI.text = "error, you fon not lingin :(";
        }
    }

    private void Confirmar()
    {
        confirmacionHecha = true;
        
        hangUpSound.Play();

        foreach (var obj in cosasAActivar)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        textoUI.gameObject.SetActive(false);
    }

    public void ResetPhone()
    {
        yaAgarrado = false;
        confirmacionHecha = false;

        if (textoUI != null)
            textoUI.gameObject.SetActive(true);
            textoUI.text = "you fon lingin";

        foreach (var obj in cosasAActivar)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        ringSound.Play(); // Vuelve a sonar el teléfono
    }
}


