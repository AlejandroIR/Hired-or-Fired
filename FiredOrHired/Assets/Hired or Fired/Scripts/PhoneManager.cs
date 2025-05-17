using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PhoneManager : MonoBehaviour
{
    public AudioSource ringSound;
    public AudioSource pickUpSound;
    private XRGrabInteractable grabInteractable;

    [SerializeField] private GameObject cosasAActivar; // Opcional, para activar algo al agarrar

    private bool yaAgarrado = false;

    void Start()
    {
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
        if (cosasAActivar != null)
            cosasAActivar.SetActive(true);
    }
}
