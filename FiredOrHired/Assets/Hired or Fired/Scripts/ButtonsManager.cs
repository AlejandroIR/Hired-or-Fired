using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonsManager : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;
    private string nextScene;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
    public void LoadSceneWithClick(string sceneName)
    {
        PlayClickSound();
        StartCoroutine(LoadSceneAfterSound(sceneName));
    }

    private IEnumerator LoadSceneAfterSound(string sceneName)
    {
        float waitTime = clickSound != null ? clickSound.length : 0.5f;
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(sceneName);
    }

    public void TogglePauseMenu(GameObject pauseCanvas)
    {
        PlayClickSound();
        pauseCanvas.SetActive(!pauseCanvas.activeSelf);
    }
}
