using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class WhiskyGlass : MonoBehaviour
{
    [Header("Referencias")]
    public AudioClip drinkingSound;
    public Volume postProcessVolume;

    [Header("Configuración")]
    [Range(0f, 90f)]
    public float drinkingAngleThreshold = 45f;
    
    public float fadeOutTime = 1.0f;
    
    [Range(0f, 1f)]
    public float drinkingSoundVolume = 0.5f;
    
    [Range(0.01f, 0.5f)]
    public float drinkingSpeed = 0.05f;

    [Header("Efectos de Post-procesado")]
    [Range(0f, 1f)]
    public float drinkingVignetteIntensity = 0.3f;
    
    [Range(0f, 1f)]
    public float drinkingChromaticAberration = 0.2f;
    
    [Range(-100f, 100f)]
    public float drinkingColorAdjustment = 10f;
    
    [Range(0f, 1f)]
    public float drinkingMotionBlurIntensity = 0.5f;
    
    [Range(0f, 200f)]
    public float drinkingBloomIntensity = 1.0f;
    
    [Range(0.1f, 5f)]
    public float postProcessTransitionSpeed = 1.0f;
    
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private PlayerEffectState playerEffectState;
    private bool isBeingHeld = false;
    private bool isDrinking = false;
    private AudioSource audioSource;
    private float whiskeyAmount = 1f;
    
    [Header("Sistema de Embriaguez")]
    [Range(1f, 10f)]
    public float maxDrunkennessLevel = 5f;
    
    [Range(0.1f, 5f)]
    public float drunkennessPerSip = 0.5f;
    
    [Range(0.01f, 2f)]
    public float sobrietyRecoveryRate = 0.05f;
    
    [Range(1f, 5f)]
    public float drunkEffectMultiplier = 2f;
    
    [SerializeField] private float currentDrunkennessLevel = 0f;
    private bool hasStartedSipping = false;
    private float timeSinceLastSip = 0f;
    private float baseSwayAmount = 0.1f;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private ColorAdjustments colorAdjustments;
    private MotionBlur motionBlur;
    private Bloom bloom;
    private float initialVignetteIntensity;
    private float initialChromaticAberration;
    private float initialSaturation;
    private float initialMotionBlurIntensity;
    private float initialBloomIntensity;
    private Coroutine postProcessTransitionCoroutine;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && drinkingSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; 
            audioSource.volume = drinkingSoundVolume;
            audioSource.loop = false;
            Debug.Log("Se ha añadido un AudioSource al objeto " + gameObject.name);
        }
    }

    private void Start()
    {
        playerEffectState = FindObjectOfType<PlayerEffectState>();
        
        if (playerEffectState == null)
        {
            Debug.LogWarning("No se encontró ningún PlayerEffectState en la escena.");
        }
        
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
        
        InitializePostProcessing();
        
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    private void InitializePostProcessing()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<Volume>();
            if (postProcessVolume == null)
            {
                Debug.LogWarning("No se encontró ningún volumen de post-procesado en la escena. Los efectos visuales al beber no funcionarán.");
                return;
            }
        }
        
        if (postProcessVolume.profile.TryGet(out Vignette vignetteEffect))
        {
            vignette = vignetteEffect;
            if (vignette.intensity.overrideState)
            {
                initialVignetteIntensity = vignette.intensity.value;
            }
            else
            {
                vignette.intensity.overrideState = true;
                initialVignetteIntensity = 0.25f;
            }
        }
        
        if (postProcessVolume.profile.TryGet(out ChromaticAberration chromaticEffect))
        {
            chromaticAberration = chromaticEffect;
            if (chromaticAberration.intensity.overrideState)
            {
                initialChromaticAberration = chromaticAberration.intensity.value;
            }
            else
            {
                chromaticAberration.intensity.overrideState = true;
                initialChromaticAberration = 0f;
            }
        }
        
        if (postProcessVolume.profile.TryGet(out ColorAdjustments colorEffect))
        {
            colorAdjustments = colorEffect;
            if (colorAdjustments.saturation.overrideState)
            {
                initialSaturation = colorAdjustments.saturation.value;
            }
            else
            {
                colorAdjustments.saturation.overrideState = true;
                initialSaturation = 0f;
            }
        }
        
        if (postProcessVolume.profile.TryGet(out MotionBlur motionBlurEffect))
        {
            motionBlur = motionBlurEffect;
            if (motionBlur.intensity.overrideState)
            {
                initialMotionBlurIntensity = motionBlur.intensity.value;
            }
            else
            {
                motionBlur.intensity.overrideState = true;
                initialMotionBlurIntensity = 0f;
            }
            
            Debug.Log("Usando versión de Motion Blur compatible con intensity, clamp y quality");
        }
        else
        {
            Debug.LogWarning("No se encontró el efecto Motion Blur en el perfil de post-procesado. Puede que necesites añadirlo al perfil.");
        }
        
        if (postProcessVolume.profile.TryGet(out Bloom bloomEffect))
        {
            bloom = bloomEffect;
            if (bloom.intensity.overrideState)
            {
                initialBloomIntensity = bloom.intensity.value;
            }
            else
            {
                bloom.intensity.overrideState = true;
                initialBloomIntensity = 0f;
            }
            Debug.Log("Efecto Bloom inicializado correctamente");
        }
        else
        {
            Debug.LogWarning("No se encontró el efecto Bloom en el perfil de post-procesado. Puede que necesites añadirlo al perfil.");
        }
    }

    private void Update()
    {
        if (hasStartedSipping)
        {
            timeSinceLastSip += Time.deltaTime;
            
            if (!isDrinking && currentDrunkennessLevel > 0)
            {
                currentDrunkennessLevel -= sobrietyRecoveryRate * Time.deltaTime;
                currentDrunkennessLevel = Mathf.Max(0, currentDrunkennessLevel);
                
                if (!isDrinking)
                {
                    UpdateDrunkPostProcessingEffects();
                }
            }
        }
        
        if (isBeingHeld)
        {
            CheckDrinking();
            
            if (isDrinking)
            {
                if (whiskeyAmount > 0)
                {
                    whiskeyAmount -= drinkingSpeed * Time.deltaTime;
                    whiskeyAmount = Mathf.Clamp01(whiskeyAmount);
                    
                    if (whiskeyAmount <= 0)
                    {
                        StopDrinking();
                    }
                }
            }
        }
        
        if (currentDrunkennessLevel > 0 && playerEffectState != null && playerEffectState.currentActiveItem == gameObject)
        {
            ApplyDrunkSwayEffect();
        }
    }
    
    private void ApplyDrunkSwayEffect()
    {
        if (!isBeingHeld) return;
        
        float swayFactor = baseSwayAmount * (currentDrunkennessLevel / maxDrunkennessLevel);
        
        float swayX = Mathf.Sin(Time.time * 1.2f) * swayFactor;
        float swayY = Mathf.Sin(Time.time * 0.9f) * swayFactor;
        float swayZ = Mathf.Sin(Time.time * 0.7f) * swayFactor;
    }
    
    private void UpdateDrunkPostProcessingEffects()
    {
        if (postProcessVolume == null) return;
        
        float intensityFactor = Mathf.Clamp01(currentDrunkennessLevel / maxDrunkennessLevel);
        
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(
                initialVignetteIntensity, 
                drinkingVignetteIntensity, 
                intensityFactor
            );
        }
        
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(
                initialChromaticAberration,
                drinkingChromaticAberration,
                intensityFactor
            );
        }
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = Mathf.Lerp(
                initialSaturation,
                drinkingColorAdjustment,
                intensityFactor
            );
        }
        
        if (motionBlur != null)
        {
            motionBlur.intensity.value = Mathf.Lerp(
                initialMotionBlurIntensity,
                drinkingMotionBlurIntensity,
                intensityFactor
            );
        }
        
        if (bloom != null)
        {
            bloom.intensity.value = Mathf.Lerp(
                initialBloomIntensity,
                drinkingBloomIntensity,
                intensityFactor
            );
        }
    }

    private void CheckDrinking()
    {
        float currentAngle = Vector3.Angle(transform.up, Vector3.up);
        bool isAngleCorrectForDrinking = currentAngle >= drinkingAngleThreshold;
        
        bool isNearMouth = false;
        if (playerEffectState != null)
        {
            isNearMouth = playerEffectState.IsObjectNearMouth(transform);
        }
        
        if (isAngleCorrectForDrinking && !isDrinking && whiskeyAmount > 0 && (isNearMouth || playerEffectState == null))
        {
            StartDrinking();
        }
        else if ((!isAngleCorrectForDrinking || whiskeyAmount <= 0 || (playerEffectState != null && !isNearMouth)) && isDrinking)
        {
            StopDrinking();
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isBeingHeld = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isBeingHeld = false;
        if (isDrinking)
        {
            StopDrinking();
        }
    }
    
    private void StartDrinking()
    {
        if (whiskeyAmount <= 0) return;
        
        isDrinking = true;
        hasStartedSipping = true;
        timeSinceLastSip = 0f;
        
        currentDrunkennessLevel += drunkennessPerSip * (whiskeyAmount * 0.5f + 0.5f);
        currentDrunkennessLevel = Mathf.Min(currentDrunkennessLevel, maxDrunkennessLevel);
        
        if (audioSource != null && drinkingSound != null)
        {
            audioSource.clip = drinkingSound;
            audioSource.volume = drinkingSoundVolume;
            audioSource.Play();
            Debug.Log("Reproduciendo sonido de beber whisky");
        }
        
        TransitionPostProcessing(true);
        
        if (playerEffectState != null)
        {
            playerEffectState.SetDrinkingState(true, gameObject);
        }
        
        Debug.Log("¡Comienza a beber whisky! Nivel de embriaguez: " + currentDrunkennessLevel.ToString("F1") + "/" + maxDrunkennessLevel);
    }

    private void StopDrinking()
    {
        isDrinking = false;
        
        TransitionPostProcessing(false);
        
        if (playerEffectState != null)
        {
            playerEffectState.SetDrinkingState(false);
        }
        
        Debug.Log("Deja de beber whisky.");
    }
    
    private void TransitionPostProcessing(bool startDrinking)
    {
        if (postProcessTransitionCoroutine != null)
        {
            StopCoroutine(postProcessTransitionCoroutine);
        }
        
        postProcessTransitionCoroutine = StartCoroutine(TransitionPostProcessingCoroutine(startDrinking));
    }
    
    private IEnumerator TransitionPostProcessingCoroutine(bool startDrinking)
    {
        float elapsedTime = 0f;
        float duration = fadeOutTime;
        
        float drunkennessFactor = Mathf.Clamp01(currentDrunkennessLevel / maxDrunkennessLevel);
        
        float startVignetteIntensity = vignette != null ? vignette.intensity.value : 0f;
        float targetVignetteIntensity;
        
        float startChromaticAberrationValue = chromaticAberration != null ? chromaticAberration.intensity.value : 0f;
        float targetChromaticAberrationValue;
        
        float startSaturationValue = colorAdjustments != null ? colorAdjustments.saturation.value : 0f;
        float targetSaturationValue;
        
        float startMotionBlurValue = motionBlur != null ? motionBlur.intensity.value : 0f;
        float targetMotionBlurValue;
        
        float startBloomValue = bloom != null ? bloom.intensity.value : 0f;
        float targetBloomValue;
        
        if (startDrinking) {
            targetVignetteIntensity = Mathf.Lerp(initialVignetteIntensity, drinkingVignetteIntensity, drunkennessFactor);
            targetChromaticAberrationValue = Mathf.Lerp(initialChromaticAberration, drinkingChromaticAberration, drunkennessFactor);
            targetSaturationValue = Mathf.Lerp(initialSaturation, drinkingColorAdjustment, drunkennessFactor);
            targetMotionBlurValue = Mathf.Lerp(initialMotionBlurIntensity, drinkingMotionBlurIntensity, drunkennessFactor);
            targetBloomValue = Mathf.Lerp(initialBloomIntensity, drinkingBloomIntensity, drunkennessFactor);
            
            float temporaryBoostMultiplier = 1.2f;
            targetVignetteIntensity *= temporaryBoostMultiplier;
            targetChromaticAberrationValue *= temporaryBoostMultiplier;
            targetMotionBlurValue *= temporaryBoostMultiplier;
            targetBloomValue *= temporaryBoostMultiplier;
        } else {
            targetVignetteIntensity = Mathf.Lerp(initialVignetteIntensity, drinkingVignetteIntensity, drunkennessFactor * 0.8f);
            targetChromaticAberrationValue = Mathf.Lerp(initialChromaticAberration, drinkingChromaticAberration, drunkennessFactor * 0.8f);
            targetSaturationValue = Mathf.Lerp(initialSaturation, drinkingColorAdjustment, drunkennessFactor * 0.8f);
            targetMotionBlurValue = Mathf.Lerp(initialMotionBlurIntensity, drinkingMotionBlurIntensity, drunkennessFactor * 0.8f);
            targetBloomValue = Mathf.Lerp(initialBloomIntensity, drinkingBloomIntensity, drunkennessFactor * 0.8f);
        }
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(startVignetteIntensity, targetVignetteIntensity, t);
            }
            
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(startChromaticAberrationValue, targetChromaticAberrationValue, t);
            }
            
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = Mathf.Lerp(startSaturationValue, targetSaturationValue, t);
            }
            
            if (motionBlur != null)
            {
                motionBlur.intensity.value = Mathf.Lerp(startMotionBlurValue, targetMotionBlurValue, t);
            }
            
            if (bloom != null)
            {
                bloom.intensity.value = Mathf.Lerp(startBloomValue, targetBloomValue, t);
            }
            
            elapsedTime += Time.deltaTime * postProcessTransitionSpeed;
            yield return null;
        }
        
        if (vignette != null)
        {
            vignette.intensity.value = targetVignetteIntensity;
        }
        
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = targetChromaticAberrationValue;
        }
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = targetSaturationValue;
        }
        
        if (motionBlur != null)
        {
            motionBlur.intensity.value = targetMotionBlurValue;
        }
        
        if (bloom != null)
        {
            bloom.intensity.value = targetBloomValue;
        }
        
        postProcessTransitionCoroutine = null;
    }
    
    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
        
        if (postProcessTransitionCoroutine != null)
        {
            StopCoroutine(postProcessTransitionCoroutine);
        }
        
        if (vignette != null)
        {
            vignette.intensity.value = initialVignetteIntensity;
        }
        
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = initialChromaticAberration;
        }
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = initialSaturation;
        }
        
        if (motionBlur != null)
        {
            motionBlur.intensity.value = initialMotionBlurIntensity;
        }
        
        if (bloom != null)
        {
            bloom.intensity.value = initialBloomIntensity;
        }
    }
}
