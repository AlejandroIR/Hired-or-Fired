using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CocaineSnif : MonoBehaviour
{
    [Header("Referencias")]
    public Volume postProcessVolume;
    public AudioClip sniffingSound;
    
    [Header("Configuración de Proximidad")]
    public float proximityThreshold = 1.5f;
    public Transform playerTransform;
    
    [Header("Configuración de Efectos")]
    public float effectDuration = 5f;
    public float cooldownTime = 3f;
    
    [Range(0f, 1f)]
    public float sniffingSoundVolume = 0.7f;
    
    [Header("Efectos de Post-procesado")]
    [Range(0f, 1f)]
    public float cocaineVignetteIntensity = 0.6f;
    
    [Range(0f, 1f)]
    public float cocaineChromaticAberration = 0.8f;
    
    [Range(-100f, 100f)]
    public float cocaineColorAdjustment = 30f;
    
    [Range(0f, 1f)]
    public float cocaineMotionBlurIntensity = 0.7f;
    
    [Range(0f, 200f)]
    public float cocaineBloomIntensity = 3.0f;
    
    [Range(0.1f, 5f)]
    public float postProcessTransitionSpeed = 2.0f;
    
    private PlayerEffectState playerEffectState;
    private AudioSource audioSource;
    private bool isPlayerNear = false;
    private bool isOnCooldown = false;
    private bool effectsActive = false;
    
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
    
    private Coroutine effectCoroutine;
    private Coroutine cooldownCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sniffingSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
            audioSource.volume = sniffingSoundVolume;
            audioSource.loop = false;
        }
    }

    private void Start()
    {
        playerEffectState = FindObjectOfType<PlayerEffectState>();
        
        if (playerTransform == null && playerEffectState != null)
        {
            playerTransform = playerEffectState.transform;
        }
        
        if (playerTransform == null)
        {
            Debug.LogError("No se encontró referencia al jugador para CocaineSnif en " + gameObject.name);
        }
        
        InitializePostProcessing();
    }
    
    private void InitializePostProcessing()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<Volume>();
            if (postProcessVolume == null)
            {
                Debug.LogWarning("No se encontró ningún volumen de post-procesado en la escena.");
                return;
            }
        }
        
        if (postProcessVolume.profile.TryGet(out Vignette vignetteEffect))
        {
            vignette = vignetteEffect;
            initialVignetteIntensity = vignette.intensity.overrideState ? vignette.intensity.value : 0.25f;
            vignette.intensity.overrideState = true;
        }
        
        if (postProcessVolume.profile.TryGet(out ChromaticAberration chromaticEffect))
        {
            chromaticAberration = chromaticEffect;
            initialChromaticAberration = chromaticAberration.intensity.overrideState ? chromaticAberration.intensity.value : 0f;
            chromaticAberration.intensity.overrideState = true;
        }
        
        if (postProcessVolume.profile.TryGet(out ColorAdjustments colorEffect))
        {
            colorAdjustments = colorEffect;
            initialSaturation = colorAdjustments.saturation.overrideState ? colorAdjustments.saturation.value : 0f;
            colorAdjustments.saturation.overrideState = true;
        }
        
        if (postProcessVolume.profile.TryGet(out MotionBlur motionBlurEffect))
        {
            motionBlur = motionBlurEffect;
            initialMotionBlurIntensity = motionBlur.intensity.overrideState ? motionBlur.intensity.value : 0f;
            motionBlur.intensity.overrideState = true;
        }
        
        if (postProcessVolume.profile.TryGet(out Bloom bloomEffect))
        {
            bloom = bloomEffect;
            initialBloomIntensity = bloom.intensity.overrideState ? bloom.intensity.value : 0f;
            bloom.intensity.overrideState = true;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool currentlyNear = distance <= proximityThreshold;
        
        if (currentlyNear && !isPlayerNear && !isOnCooldown && !effectsActive)
        {
            TriggerCocaineEffect();
        }
        
        isPlayerNear = currentlyNear;
    }
    
    private void TriggerCocaineEffect()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        
        effectCoroutine = StartCoroutine(CocaineEffectCoroutine());
        
        if (audioSource != null && sniffingSound != null)
        {
            audioSource.clip = sniffingSound;
            audioSource.volume = sniffingSoundVolume;
            audioSource.Play();
        }
        
        if (playerEffectState != null)
        {
            playerEffectState.SetCocaineState(true, gameObject);
        }
        
        Debug.Log("¡Efecto de cocaína activado!");
    }
    
    private IEnumerator CocaineEffectCoroutine()
    {
        effectsActive = true;
        
        // Aplicar efectos inmediatamente
        yield return StartCoroutine(TransitionPostProcessing(true, 0.5f));
        
        // Mantener efectos durante la duración especificada
        yield return new WaitForSeconds(effectDuration - 1f); // -1f para la transición de salida
        
        // Quitar efectos gradualmente
        yield return StartCoroutine(TransitionPostProcessing(false, 0.5f));
        
        effectsActive = false;
        
        if (playerEffectState != null)
        {
            playerEffectState.SetCocaineState(false);
        }
        
        // Iniciar cooldown
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
        cooldownCoroutine = StartCoroutine(CooldownCoroutine());
        
        Debug.Log("Efecto de cocaína terminado.");
    }
    
    private IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
        Debug.Log("Cooldown de cocaína terminado.");
    }
    
    private IEnumerator TransitionPostProcessing(bool activate, float transitionDuration)
    {
        float elapsedTime = 0f;
        
        float startVignette = vignette != null ? vignette.intensity.value : 0f;
        float startChromatic = chromaticAberration != null ? chromaticAberration.intensity.value : 0f;
        float startSaturation = colorAdjustments != null ? colorAdjustments.saturation.value : 0f;
        float startMotionBlur = motionBlur != null ? motionBlur.intensity.value : 0f;
        float startBloom = bloom != null ? bloom.intensity.value : 0f;
        
        float targetVignette = activate ? cocaineVignetteIntensity : initialVignetteIntensity;
        float targetChromatic = activate ? cocaineChromaticAberration : initialChromaticAberration;
        float targetSaturation = activate ? cocaineColorAdjustment : initialSaturation;
        float targetMotionBlur = activate ? cocaineMotionBlurIntensity : initialMotionBlurIntensity;
        float targetBloom = activate ? cocaineBloomIntensity : initialBloomIntensity;
        
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            
            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(startVignette, targetVignette, t);
            
            if (chromaticAberration != null)
                chromaticAberration.intensity.value = Mathf.Lerp(startChromatic, targetChromatic, t);
            
            if (colorAdjustments != null)
                colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, targetSaturation, t);
            
            if (motionBlur != null)
                motionBlur.intensity.value = Mathf.Lerp(startMotionBlur, targetMotionBlur, t);
            
            if (bloom != null)
                bloom.intensity.value = Mathf.Lerp(startBloom, targetBloom, t);
            
            elapsedTime += Time.deltaTime * postProcessTransitionSpeed;
            yield return null;
        }
        
        // Asegurar valores finales
        if (vignette != null) vignette.intensity.value = targetVignette;
        if (chromaticAberration != null) chromaticAberration.intensity.value = targetChromatic;
        if (colorAdjustments != null) colorAdjustments.saturation.value = targetSaturation;
        if (motionBlur != null) motionBlur.intensity.value = targetMotionBlur;
        if (bloom != null) bloom.intensity.value = targetBloom;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, proximityThreshold);
    }
    
    private void OnDestroy()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
        
        // Restaurar valores iniciales
        if (vignette != null) vignette.intensity.value = initialVignetteIntensity;
        if (chromaticAberration != null) chromaticAberration.intensity.value = initialChromaticAberration;
        if (colorAdjustments != null) colorAdjustments.saturation.value = initialSaturation;
        if (motionBlur != null) motionBlur.intensity.value = initialMotionBlurIntensity;
        if (bloom != null) bloom.intensity.value = initialBloomIntensity;
    }
}
