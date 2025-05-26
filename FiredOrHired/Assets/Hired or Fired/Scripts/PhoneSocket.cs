using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PhoneSocket : MonoBehaviour
{
    public PhoneManager phoneManager;
    private XRSocketInteractor socket;

    private void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socket.selectEntered.AddListener(OnPhonePlaced);
    }

    private void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnPhonePlaced);
    }

    private void OnPhonePlaced(SelectEnterEventArgs args)
    {
        if (args.interactableObject.transform.CompareTag("Phone"))
        {
            Debug.Log("Tel�fono colocado en el socket.");
            phoneManager.Confirmar(); // Aqu� se cuelga
        }
    }
}
