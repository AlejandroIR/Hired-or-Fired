using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PhoneSocketDetector : MonoBehaviour
{
    public PhoneManager phoneManager;

    private XRSocketInteractor socketInteractor;

    private void Awake()
    {
        socketInteractor = GetComponent<XRSocketInteractor>();

        if (socketInteractor == null)
        {
            Debug.LogError("No se encontr� XRSocketInteractor en el objeto.");
            enabled = false;
            return;
        }

        socketInteractor.selectEntered.AddListener(OnSocketEnter);
    }

    private void OnDestroy()
    {
        socketInteractor.selectEntered.RemoveListener(OnSocketEnter);
    }

    private void OnSocketEnter(SelectEnterEventArgs args)
    {
        // Verifica que el objeto que entra es el tel�fono
        if (args.interactableObject.transform.GetComponent<PhoneManager>() == phoneManager)
        {
            Debug.Log("Tel�fono colocado en socket. Ejecutando Confirmar().");
            phoneManager.ConfirmarDesdeSocket(); // llamado seguro
        }
    }
}
