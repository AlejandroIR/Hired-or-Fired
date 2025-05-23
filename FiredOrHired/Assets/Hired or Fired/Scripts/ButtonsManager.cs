using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Para cambiar de escena
using UnityEngine.UI;             // Por si usas UI
using UnityEngine.EventSystems;   // Opcional

public class ButtonsManager : MonoBehaviour
{
    public string sceneToLoad;   
    public AudioClip clickSound;      
    private AudioSource audioSource;  

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnButtonClick()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        StartCoroutine(LoadSceneAfterSound());
    }

    private IEnumerator LoadSceneAfterSound()
    {
        float waitTime = clickSound != null ? clickSound.length : 0.5f;
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneToLoad);
    }
}
