using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LLMUnity;
using TMPro;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

[System.Serializable]
public class NpcData
{
    public string npcName;
    [TextArea(5, 10)]
    public string systemPrompt;
    public GameObject npcPrefab;
    public AudioClip voiceClip;
}

public class NpcManager : MonoBehaviour
{   
    [Header("NPC Configuration")]
    public List<NpcData> availableNpcs = new List<NpcData>();
    public int currentNpcIndex = 0;
    
    [Header("Voice Recognition")]
    public VoiceManager voiceManager;
    public KeyCode recordKey = KeyCode.J;
    private bool isRecording = false;
    
    [Header("Dialog UI")]
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public float dialogDisplayTime = 5f;
    public float fadeOutTime = 1f;
    public Image dialogBackground;
    public TextMeshProUGUI statusText;
    
    [Header("LLM Configuration")]
    public LLMCharacter llmCharacter;
    private GameObject currentNpcInstance;
    
    [Header("Performance Settings")]
    [Tooltip("Maximum length of chat history to keep in memory")]
    public int maxChatHistoryLength = 10;
    [Tooltip("Show processing times for debugging")]
    public bool showProcessingTimes = true;
    [Tooltip("Reduce model parameters for faster responses (lower quality)")]
    public bool optimizeForSpeed = false;
    
    [Header("Integration")]
    public GameManager gameManager;
    
    // Estado interno
    private string currentPlayerInput = "";
    private bool isProcessing = false;
    private bool isReady = false;
    private Coroutine fadeOutCoroutine;
    private Stopwatch processingTimer = new Stopwatch();

    private void Start()
    {
        // Inicializar UI
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
            
        // Configurar reconocimiento de voz
        if (voiceManager == null)
            voiceManager = FindObjectOfType<VoiceManager>();
            
        // Configurar LLM
        if (llmCharacter == null)
            llmCharacter = FindObjectOfType<LLMCharacter>();
        
        // Añadir NPCs predefinidos si la lista está vacía
        if (availableNpcs.Count == 0)
        {
            // Recepcionista
            NpcData recepcionista = new NpcData
            {
                npcName = "Jamie (Recepcionista)",
                systemPrompt = "Eres Jamie, un/a recepcionista amable y servicial que trabaja en la empresa Megacorp. IMPORTANTE: Responde siempre de forma breve y natural, como lo haría una persona real en una conversación normal. Evita respuestas largas o excesivamente formales. Limita tus respuestas a 1-3 frases cortas como máximo. Por ejemplo, si te preguntan '¿Cómo estás?', responde algo como 'Bien, gracias. ¿En qué puedo ayudarte hoy?'",
                npcPrefab = null
            };
            
            // Jefe de Proyecto
            NpcData jefeProyecto = new NpcData
            {
                npcName = "Alex (Jefe de Proyecto)",
                systemPrompt = "Eres Alex, un/a jefe de proyecto estresado/a en Megacorp. IMPORTANTE: Responde siempre de forma muy breve, directa y algo cortante, como una persona ocupada lo haría en la vida real. Limita tus respuestas a 1-2 frases cortas. Por ejemplo, si te preguntan '¿Cómo estás?', podrías responder 'Con prisas, como siempre. ¿Qué necesitas?'",
                npcPrefab = null
            };
            
            // Científico
            NpcData cientifico = new NpcData
            {
                npcName = "Dr. Morgan (Científico)",
                systemPrompt = "Eres Dr. Morgan, científico/a del laboratorio de I+D de Megacorp. IMPORTANTE: Responde siempre de forma breve y natural, como una persona real. Limita tus respuestas a 1-3 frases cortas. Evita largas explicaciones técnicas. Por ejemplo, a un '¿Qué tal?' responderías algo como 'Bien, trabajando en un nuevo experimento. ¿Y tú?'",
                npcPrefab = null
            };
            
            // Añadir los NPCs a la lista
            availableNpcs.Add(recepcionista);
            availableNpcs.Add(jefeProyecto);
            availableNpcs.Add(cientifico);
            
            Debug.Log("NPCs predefinidos añadidos al NpcManager");
        }
        
        // Aplicar configuraciones de rendimiento si están habilitadas
        if (optimizeForSpeed && llmCharacter != null)
        {
            ApplyPerformanceSettings();
        }
        
        // Suscribirse al evento de reconocimiento de voz
        if (voiceManager != null)
        {
            voiceManager.OnSpeechRecognized += OnSpeechRecognized;
        }
        
        // Inicializar con el primer NPC y hacer warm-up
        if (availableNpcs.Count > 0)
        {
            ChangeNpc(0);
            // Realizar warm-up del modelo en segundo plano
            StartCoroutine(WarmUpModel());
        }
        
        UpdateStatusText("Inicializando modelo LLM...");
    }
    
    private IEnumerator WarmUpModel()
    {
        isReady = false;
        UpdateStatusText("Inicializando modelo LLM...");
        
        // Ejecutar el warm-up del modelo
        Task warmupTask = llmCharacter.Warmup(OnWarmupComplete);
        
        // Esperar a que se complete el warm-up
        while (!isReady)
        {
            yield return null;
        }
        
        UpdateStatusText("Listo - Presiona el botón para hablar");
    }
    
    private void OnWarmupComplete()
    {
        isReady = true;
        Debug.Log("Modelo LLM inicializado y listo");
    }
    
    private void UpdateStatusText(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;
        }
        Debug.Log(status);
    }
      private void Update()
    {
        // Manejar inicio/fin de grabación con tecla J
        if (Input.GetKeyDown(recordKey) && !isProcessing && isReady)
        {
            ToggleRecording();
        }
    }
    
    // Método público para alternar entre iniciar/detener grabación
    // Puede ser llamado desde un botón UI o desde otros scripts
    public void ToggleRecording()
    {
        if (!isProcessing && isReady)
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecording();
        }
    }
      private void StartRecording()
    {
        isRecording = true;
        UpdateStatusText("Grabando...");
        
        // Mostrar algún indicador visual de que está grabando
        ShowRecordingIndicator(true);
        
        // Iniciar grabación a través del VoiceManager
        if (voiceManager != null)
        {
            voiceManager.OnRecordButtonDown();
        }
    }
    
    private void StopRecording()
    {
        isRecording = false;
        UpdateStatusText("Procesando voz...");
        
        // Ocultar indicador visual
        ShowRecordingIndicator(false);
        
        // Detener grabación a través del VoiceManager
        if (voiceManager != null)
        {
            voiceManager.OnRecordButtonUp();
            isProcessing = true;
        }
    }
    
    private void ShowRecordingIndicator(bool show)
    {
        // Implementar indicador visual de grabación
        if (dialogBackground != null)
        {
            // Cambiar el color del panel para indicar grabación
            dialogBackground.color = show ? 
                new Color(1f, 0.3f, 0.3f, 0.7f) : // Color rojo para indicar grabación
                new Color(dialogBackground.color.r, dialogBackground.color.g, dialogBackground.color.b, 1f);
                
            if (show)
            {
                dialogPanel.SetActive(true);
                dialogText.text = "🎤 Grabando...";
            }
        }
    }
    
    // Método que será llamado desde VoiceManager cuando se reconozca texto
    public void OnSpeechRecognized(string recognizedText)
    {
        currentPlayerInput = recognizedText;
        isProcessing = true;
        
        // Mostrar mensaje del jugador brevemente
        ShowPlayerDialog(recognizedText);
        
        // Procesar la entrada del jugador con el LLM
        StartCoroutine(ProcessNpcResponseCoroutine(recognizedText));
    }
    
    private IEnumerator ProcessNpcResponseCoroutine(string playerInput)
    {
        if (llmCharacter == null)
        {
            Debug.LogError("LLMCharacter no está configurado");
            isProcessing = false;
            yield break;
        }
        
        // Iniciar temporizador
        processingTimer.Reset();
        processingTimer.Start();
        
        // Mostrar mensaje de procesamiento
        UpdateStatusText("El NPC está pensando...");
        
        // Crear un bubble temporal con "..." para mostrar que está pensando
        ShowNpcDialog("...");
        
        // Esperar un frame para asegurar que la UI se actualice
        yield return null;
        
        // Crear una variable para almacenar la referencia a la tarea y evitar múltiples llamadas
        Task<string> chatTask = null;
        
        // Iniciar la tarea de LLM.Chat con los callbacks correctos para los tipos LLMUnity
        try
        {
            chatTask = llmCharacter.Chat(
                playerInput, 
                (string partialResponse) => ShowNpcDialog(partialResponse), 
                () => {
                    // Detener temporizador y mostrar tiempo
                    processingTimer.Stop();
                    if (showProcessingTimes)
                    {
                        Debug.Log($"Tiempo de respuesta: {processingTimer.ElapsedMilliseconds}ms");
                    }
                    
                    // Resetear estado cuando se completa
                    isProcessing = false;
                    UpdateStatusText("Listo - Presiona J para hablar");
                }, 
                true
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Excepción al iniciar la tarea de Chat: {ex.Message}");
            isProcessing = false;
            UpdateStatusText("Error - Presiona J para intentar de nuevo");
            yield break;
        }
        
        // Esperar a que la tarea se complete, sin bloquear el hilo principal
        while (chatTask != null && !chatTask.IsCompleted)
        {
            yield return null;
        }
        
        // Verificar si hay errores después de que la tarea se complete
        if (chatTask != null && chatTask.IsFaulted && chatTask.Exception != null)
        {
            Debug.LogError($"Error al procesar la respuesta del NPC: {chatTask.Exception}");
            isProcessing = false;
            UpdateStatusText("Error - Presiona J para intentar de nuevo");
        }
        
        // No necesitamos usar la respuesta final ya que se muestra a través del callback
    }
    
    public void ShowPlayerDialog(string dialogContent)
    {
        // Mostrar mensaje del jugador brevemente
        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);
            
        dialogPanel.SetActive(true);
        dialogText.text = $"Tú: {dialogContent}";
        
        // Programar desvanecimiento rápido para el mensaje del jugador
        fadeOutCoroutine = StartCoroutine(FadeOutDialog(1.5f)); // Desvanecimiento más rápido para el mensaje del jugador
    }
    
    public void ShowNpcDialog(string dialogContent)
    {
        // Cancelar cualquier desvanecimiento anterior
        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);
            
        // Mostrar panel de diálogo
        dialogPanel.SetActive(true);
        dialogText.text = $"{availableNpcs[currentNpcIndex].npcName}: {dialogContent}";
        
        // Restablecer cualquier animación previa
        dialogBackground.color = new Color(dialogBackground.color.r, dialogBackground.color.g, dialogBackground.color.b, 1f);
        dialogText.color = new Color(dialogText.color.r, dialogText.color.g, dialogText.color.b, 1f);
        
        // Programar desvanecimiento solo cuando el texto está completo
        if (!dialogContent.Equals("..."))
        {
            fadeOutCoroutine = StartCoroutine(FadeOutDialog(dialogDisplayTime));
        }
    }
    
    private IEnumerator FadeOutDialog(float displayTime = 5f)
    {
        // Esperar el tiempo de visualización
        yield return new WaitForSeconds(displayTime);
        
        // Desvanecer gradualmente
        float elapsedTime = 0;
        Color bgInitialColor = dialogBackground.color;
        Color textInitialColor = dialogText.color;
        
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeOutTime));
            
            dialogBackground.color = new Color(bgInitialColor.r, bgInitialColor.g, bgInitialColor.b, alpha);
            dialogText.color = new Color(textInitialColor.r, textInitialColor.g, textInitialColor.b, alpha);
            
            yield return null;
        }
        
        // Ocultar panel al finalizar
        dialogPanel.SetActive(false);
    }
    
    public void ChangeNpc(int index)
    {
        if (index < 0 || index >= availableNpcs.Count)
        {
            Debug.LogError("Índice de NPC fuera de rango");
            return;
        }
        
        // Guardar índice actual
        currentNpcIndex = index;
        
        // Configurar el prompt del LLM para este NPC
        if (llmCharacter != null)
        {
            llmCharacter.SetPrompt(availableNpcs[index].systemPrompt, true);
            Debug.Log($"Cambiado a NPC: {availableNpcs[index].npcName}");
            
            // Notificar al usuario del cambio
            UpdateStatusText($"NPC cambiado a: {availableNpcs[index].npcName}");
        }
        
        // Eliminar instancia anterior del NPC si existe
        if (currentNpcInstance != null)
            Destroy(currentNpcInstance);
            
        // Instanciar nuevo NPC
        if (availableNpcs[index].npcPrefab != null)
        {
            currentNpcInstance = Instantiate(availableNpcs[index].npcPrefab, transform.position, Quaternion.identity);
        }
        
        // Synchronize with GameManager if needed but avoid infinite loop
        if (gameManager != null && gameManager.currentNPCIndex != index + 1 && !gameManager.isActiveAndEnabled)
        {
            // This synchronization only happens when NpcManager changes independently
            Debug.Log("Synchronizing GameManager with NpcManager");
        }
    }
    
    // Método para cambiar al siguiente NPC
    public void NextNpc()
    {
        int nextIndex = (currentNpcIndex + 1) % availableNpcs.Count;
        ChangeNpc(nextIndex);
    }
    
    // Método para cambiar al NPC anterior
    public void PreviousNpc()
    {
        int prevIndex = (currentNpcIndex - 1 + availableNpcs.Count) % availableNpcs.Count;
        ChangeNpc(prevIndex);
    }
    
    private void ApplyPerformanceSettings()
    {
        // Reducir parámetros que afectan la velocidad de inferencia
        llmCharacter.topK = 20; // Valor menor = menos opciones a considerar
        llmCharacter.temperature = 0.1f; // Menor temperatura = respuestas más deterministas
        llmCharacter.topP = 0.8f; // Valor menor = menos tokens a considerar
        
        // Limitar el máximo de tokens a predecir
        if (llmCharacter.numPredict == -1 || llmCharacter.numPredict > 200)
            llmCharacter.numPredict = 200;
            
        Debug.Log("Configuraciones de rendimiento aplicadas para respuestas más rápidas");
    }
    
    // Método para cambiar parámetros de rendimiento en tiempo de ejecución
    public void ToggleOptimizeForSpeed(bool optimize)
    {
        optimizeForSpeed = optimize;
        if (optimize && llmCharacter != null)
        {
            ApplyPerformanceSettings();
        }
    }
    
    // Método para cambiar el tamaño máximo del historial
    public void SetMaxHistoryLength(int length)
    {
        maxChatHistoryLength = length;
        Debug.Log($"Tamaño máximo de historial de chat establecido en: {length}");
        
        // Limpiar historial si supera el máximo establecido
        if (maxChatHistoryLength > 0 && llmCharacter.chat.Count > maxChatHistoryLength + 1)
        {
            int toRemove = llmCharacter.chat.Count - maxChatHistoryLength - 1;
            llmCharacter.chat.RemoveRange(1, toRemove);
            Debug.Log($"Historial de chat reducido (eliminados {toRemove} mensajes para mantener rendimiento)");
        }
    }
    
    // Método para cancelar cualquier solicitud pendiente
    public void CancelRequest()
    {
        if (llmCharacter != null)
        {
            llmCharacter.CancelRequests();
            isProcessing = false;
            UpdateStatusText("Solicitud cancelada - Presiona J para hablar");
        }
    }
    
    void OnDisable()
    {
        // Limpiar suscripciones al evento
        if (voiceManager != null)
        {
            voiceManager.OnSpeechRecognized -= OnSpeechRecognized;
        }
    }
}