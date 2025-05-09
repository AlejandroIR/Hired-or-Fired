using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System;
using LLMUnitySamples;

public class VoiceManager : MonoBehaviour
{
    public string witToken = "TILF65OEZMWFWBSCV6UCJLKXGFT5PPTO";
    public AudioSource audioSource;
    public Button recordButton;
    private AudioClip recordedClip;
    private bool isRecording = false;
    
    // Carpeta para guardar los archivos
    private string saveFolderPath;
    // Ruta donde se guardó el último archivo WAV
    private string lastWavPath;
    // Ruta donde se guardó la última respuesta JSON
    private string lastJsonPath;

    public ChatBot chatBot;
    
    // Agregamos referencia al NpcManager
    public NpcManager npcManager;
    
    // Sistema de eventos para reconocimiento de voz
    public delegate void SpeechRecognizedDelegate(string recognizedText);
    public event SpeechRecognizedDelegate OnSpeechRecognized;
    
    private void Awake()
    {
        if (chatBot == null)
        {
            chatBot = FindObjectOfType<ChatBot>();
            if (chatBot == null)
            {
                Debug.LogWarning("No se encontró ningún ChatBot en la escena. La integración de voz a chat no funcionará.");
            }
        }
        
        if (npcManager == null)
        {
            npcManager = FindObjectOfType<NpcManager>();
        }
    }

    void Start()
    {
        // Configurar la carpeta para guardar los archivos
        saveFolderPath = Path.Combine(Application.persistentDataPath, "VoiceRecordings");
        
        // Crear el directorio si no existe
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }
        else
        {
            // Eliminar archivos WAV y JSON existentes
            DeletePreviousRecordings();
        }
        
        Debug.Log("Los archivos se guardarán en: " + saveFolderPath);
        
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
    
    private void DeletePreviousRecordings()
    {
        try
        {
            // Obtener todos los archivos WAV y JSON en la carpeta
            string[] wavFiles = Directory.GetFiles(saveFolderPath, "*.wav");
            string[] jsonFiles = Directory.GetFiles(saveFolderPath, "*.json");
            
            int deletedCount = 0;
            
            // Eliminar archivos WAV
            foreach (string file in wavFiles)
            {
                File.Delete(file);
                deletedCount++;
            }
            
            // Eliminar archivos JSON
            foreach (string file in jsonFiles)
            {
                File.Delete(file);
                deletedCount++;
            }
            
            if (deletedCount > 0)
            {
                Debug.Log("Se eliminaron " + deletedCount + " archivos WAV y JSON anteriores.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error al eliminar archivos anteriores: " + ex.Message);
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
            // Generar nombre de archivo con timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            // Guardar el archivo WAV
            byte[] wavData = SavWav.GetWavBytes(recordedClip);
            lastWavPath = Path.Combine(saveFolderPath, "recording_" + timestamp + ".wav");
            File.WriteAllBytes(lastWavPath, wavData);
            Debug.Log("Archivo WAV guardado en: " + lastWavPath);
            
            yield return StartCoroutine(SendToWit(wavData, timestamp));
        }
        else
        {
            Debug.LogError("No valid recording to process.");
        }
    }

    IEnumerator SendToWit(byte[] wavData, string timestamp)
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
            
            // Guardar la respuesta JSON
            lastJsonPath = Path.Combine(saveFolderPath, "response_" + timestamp + ".json");
            File.WriteAllText(lastJsonPath, response);
            Debug.Log("Respuesta JSON guardada en: " + lastJsonPath);
            
            string transcript = ExtractTranscriptFromResponse(response);
            Debug.Log("You said: \"" + transcript + "\"");
            
            SendTranscriptToChatBot(transcript);
        }
        else
        {
            Debug.LogError("Error sending to Wit.ai: " + www.error);
        }
    }
    
    private void SendTranscriptToChatBot(string transcript)
    {
        if (string.IsNullOrEmpty(transcript))
        {
            Debug.LogWarning("No se puede enviar un texto vacío al ChatBot");
            return;
        }
        
        // Opción 1: Usar el sistema de eventos
        // Si hay suscriptores al evento OnSpeechRecognized (como el NpcManager), 
        // enviamos el texto a través del evento
        if (OnSpeechRecognized != null)
        {
            OnSpeechRecognized.Invoke(transcript);
            return;
        }
        
        // Opción 2: Llamar directamente al NpcManager
        // Solo si no hay suscriptores al evento, intentamos enviar
        // directamente al NpcManager
        if (npcManager != null)
        {
            npcManager.OnSpeechRecognized(transcript);
            return;
        }
        
        // Opción 3: Compatibilidad con el ChatBot existente
        // Solo como último recurso
        if (chatBot != null)
        {
            chatBot.SetVoiceRecognizedText(transcript);
            return;
        }
        
        Debug.LogError("No se pudo enviar el texto reconocido. No hay receptores configurados.");
    }
    
    private string ExtractTranscriptFromResponse(string jsonResponse)
    {
        try
        {
            // La respuesta de Wit.ai puede contener múltiples bloques JSON
            // Necesitamos extraer el último valor de "text" ya que este es el más completo
            
            // Obtener todas las coincidencias de "text":"valor"
            MatchCollection matches = Regex.Matches(jsonResponse, "\"text\"\\s*:\\s*\"([^\"]*)\"");
            
            if (matches.Count > 0)
            {
                // Usar la última coincidencia (la más completa)
                return matches[matches.Count - 1].Groups[1].Value;
            }
            
            // Intentar con formato alternativo si no hay coincidencias
            matches = Regex.Matches(jsonResponse, "text[\"']?\\s*:\\s*[\"']([^\"']*)[\"']");
            if (matches.Count > 0)
            {
                return matches[matches.Count - 1].Groups[1].Value;
            }
            
            // Si hay "transcript" en lugar de "text"
            matches = Regex.Matches(jsonResponse, "\"transcript\"\\s*:\\s*\"([^\"]*)\"");
            if (matches.Count > 0)
            {
                return matches[matches.Count - 1].Groups[1].Value;
            }
            
            // Mostrar parte del JSON para debug
            Debug.Log("JSON completo: " + jsonResponse);
            return "No se pudo extraer (fragmento: " + 
                   (jsonResponse.Length > 100 ? jsonResponse.Substring(0, 100) + "..." : jsonResponse) + 
                   ")";
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al analizar la respuesta JSON: " + e.Message);
            Debug.Log("JSON con problemas: " + jsonResponse);
            return "Error al analizar la respuesta: " + e.Message;
        }
    }
}
