using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Text.RegularExpressions;

public class VoiceTrivia : MonoBehaviour
{
    public string witToken = "TILF65OEZMWFWBSCV6UCJLKXGFT5PPTO";
    public AudioSource audioSource;
    public Button recordButton;
    private AudioClip recordedClip;
    private bool isRecording = false;

    void Start()
    {
        // Add button press/release event listeners
        if (recordButton != null)
        {
            // Add event triggers for press and release
            EventTrigger eventTrigger = recordButton.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = recordButton.gameObject.AddComponent<EventTrigger>();
            
            // Press event
            EventTrigger.Entry entryPress = new EventTrigger.Entry();
            entryPress.eventID = EventTriggerType.PointerDown;
            entryPress.callback.AddListener((data) => { OnRecordButtonDown(); });
            eventTrigger.triggers.Add(entryPress);
            
            // Release event
            EventTrigger.Entry entryRelease = new EventTrigger.Entry();
            entryRelease.eventID = EventTriggerType.PointerUp;
            entryRelease.callback.AddListener((data) => { OnRecordButtonUp(); });
            eventTrigger.triggers.Add(entryRelease);
        }
        else
        {
            Debug.LogError("Record Button not assigned in the inspector!");
        }
    }

    public void OnRecordButtonDown()
    {
        if (!isRecording)
        {
            isRecording = true;
            StartCoroutine(StartRecording());
        }
    }

    public void OnRecordButtonUp()
    {
        if (isRecording)
        {
            isRecording = false;
            StopRecording();
            StartCoroutine(ProcessRecording());
        }
    }

    IEnumerator StartRecording()
    {
        Debug.Log("Started recording... Press and hold to continue, release to stop.");
        recordedClip = Microphone.Start(null, false, 60, 16000); // Max 60 seconds recording
        yield return null;
    }

    void StopRecording()
    {
        if (Microphone.IsRecording(null))
        {
            int position = Microphone.GetPosition(null);
            Microphone.End(null);
            
            // Create a new clip with the correct length
            if (position > 0)
            {
                AudioClip newClip = AudioClip.Create(recordedClip.name, position, recordedClip.channels, 
                                                    recordedClip.frequency, false);
                float[] samples = new float[position * recordedClip.channels];
                recordedClip.GetData(samples, 0);
                newClip.SetData(samples, 0);
                recordedClip = newClip;
                
                Debug.Log("Recording finished. Length: " + position / 16000f + " seconds");
            }
            else
            {
                Debug.LogWarning("Recording was too short, no audio data captured.");
                recordedClip = null;
            }
        }
    }

    IEnumerator ProcessRecording()
    {
        if (recordedClip != null && recordedClip.length > 0)
        {
            byte[] wavData = SavWav.GetWavBytes(recordedClip);
            yield return StartCoroutine(SendToWit(wavData));
        }
        else
        {
            Debug.LogError("No valid recording to process.");
        }
    }

    IEnumerator SendToWit(byte[] wavData)
    {
        UnityWebRequest www = new UnityWebRequest("https://api.wit.ai/speech?v=20230225", "POST");
        www.uploadHandler = new UploadHandlerRaw(wavData);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Bearer " + witToken);
        www.SetRequestHeader("Content-Type", "audio/wav");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string response = www.downloadHandler.text;
            string transcript = ExtractTranscriptFromResponse(response);
            Debug.Log("You said: \"" + transcript + "\"");
        }
        else
        {
            Debug.LogError("Error sending to Wit.ai: " + www.error);
        }
    }
    
    private string ExtractTranscriptFromResponse(string jsonResponse)
    {
        try
        {
            // Buscar diferentes patrones comunes en la respuesta de Wit.ai
            
            // Primer intento - formato estándar "text":"valor"
            Match match = Regex.Match(jsonResponse, "\"text\"\\s*:\\s*\"([^\"]*)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            // Segundo intento - formato "text" con posibles espacios
            match = Regex.Match(jsonResponse, "text[\"']?\\s*:\\s*[\"']([^\"']*)[\"']");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            // Si hay "transcript" en lugar de "text"
            match = Regex.Match(jsonResponse, "\"transcript\"\\s*:\\s*\"([^\"]*)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            // Si ninguno coincide, mostrar parte del JSON para debug
            return "No se pudo extraer (fragmento: " + 
                   (jsonResponse.Length > 100 ? jsonResponse.Substring(0, 100) + "..." : jsonResponse) + 
                   ")";
        }
        catch (System.Exception e)
        {
            return "Error al analizar la respuesta: " + e.Message;
        }
    }
}
