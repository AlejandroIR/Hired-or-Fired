using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class CigaretteSmoke : MonoBehaviour
{    
    [Header("Referencias")]
    public ParticleSystem smokeParticleSystem;
    
    public AudioClip smokingSound;
    
    public Volume postProcessVolume;
    
    [Header("Configuración")]
    public float smokingEmissionRate = 50f;
    
    public float fadeOutTime = 1.0f;
    
    [Range(0f, 1f)]
    public float smokingSoundVolume = 0.5f;
      [Header("Efectos de Post-procesado")]
    [Range(0f, 1f)]
    public float smokingVignetteIntensity = 0.4f;
    
    [Range(-100f, 100f)]
    public float smokingColorAdjustment = -10f;
    
    [Range(0.1f, 5f)]
    public float postProcessTransitionSpeed = 1.0f;    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private PlayerEffectState playerEffectState;
    private bool isBeingHeld = false;
    private bool isSmoking = false;
    private ParticleSystem.EmissionModule emissionModule;
    private AudioSource audioSource;
    
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private float initialVignetteIntensity;
    private float initialSaturation;
    private Coroutine postProcessTransitionCoroutine;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (smokeParticleSystem == null)
        {
            smokeParticleSystem = GetComponentInChildren<ParticleSystem>();
            Debug.LogWarning("Se ha asignado automáticamente el sistema de partículas de humo en " + gameObject.name);
        }
        
        if (smokeParticleSystem != null)
        {
            emissionModule = smokeParticleSystem.emission;
            Debug.Log("Sistema de partículas de humo encontrado en " + gameObject.name);
            emissionModule.rateOverTime = 0f;
        }
        else
        {
            Debug.LogError("No se encontró ningún sistema de partículas para el humo en " + gameObject.name);
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && smokingSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
            audioSource.volume = smokingSoundVolume;
            audioSource.loop = false;
            Debug.Log("Se ha añadido un AudioSource al objeto " + gameObject.name);
        }
    }    private void Start()
    {
        playerEffectState = FindObjectOfType<PlayerEffectState>();
        
        if (playerEffectState == null)
        {
            Debug.LogWarning("No se encontró ningún PlayerEffectState en la escena. La detección de fumado no funcionará.");
        }
        
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
        
        InitializePostProcessing();
    }
    
    private void InitializePostProcessing()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<Volume>();
            if (postProcessVolume == null)
            {
                Debug.LogWarning("No se encontró ningún volumen de post-procesado en la escena. Los efectos visuales al fumar no funcionarán.");
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
    }

    private void Update()
    {
        if (isBeingHeld && playerEffectState != null)
        {
            bool isNearMouth = playerEffectState.IsObjectNearMouth(transform);
            
            if (isNearMouth && !isSmoking)
            {
                StartSmoking();
            }
            else if (!isNearMouth && isSmoking)
            {
                StopSmoking();
            }
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isBeingHeld = true;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isBeingHeld = false;
        if (isSmoking)
        {
            StopSmoking();
        }
    }    private void StartSmoking()
    {
        isSmoking = true;
        
        if (smokeParticleSystem != null)
        {
            emissionModule.rateOverTime = smokingEmissionRate;
            
            if (!smokeParticleSystem.isPlaying)
            {
                smokeParticleSystem.Play();
            }
        }
        
        if (audioSource != null && smokingSound != null)
        {
            audioSource.clip = smokingSound;
            audioSource.volume = smokingSoundVolume;
            audioSource.Play();
            Debug.Log("Reproduciendo sonido de fumar");
        }
        
        TransitionPostProcessing(true);
        
        if (playerEffectState != null)
        {
            playerEffectState.SetSmokingState(true, gameObject);
        }
        
        Debug.Log("¡Comienza a fumar!");
    }private void StopSmoking()
    {
        isSmoking = false;
        
        if (smokeParticleSystem != null)
        {
            emissionModule.rateOverTime = 0f;
        }
        
        TransitionPostProcessing(false);
        
        if (playerEffectState != null)
        {
            playerEffectState.SetSmokingState(false);
        }
        
        Debug.Log("Deja de fumar.");
    }
    
    private void TransitionPostProcessing(bool startSmoking)
    {
        if (postProcessTransitionCoroutine != null)
        {
            StopCoroutine(postProcessTransitionCoroutine);
        }
        
        postProcessTransitionCoroutine = StartCoroutine(TransitionPostProcessingCoroutine(startSmoking));
    }
    
    private IEnumerator TransitionPostProcessingCoroutine(bool startSmoking)
    {
        float elapsedTime = 0f;
        float duration = fadeOutTime;
        
        float startVignetteIntensity = vignette != null ? vignette.intensity.value : 0f;
        float targetVignetteIntensity = startSmoking ? smokingVignetteIntensity : initialVignetteIntensity;
        
        float startSaturationValue = colorAdjustments != null ? colorAdjustments.saturation.value : 0f;
        float targetSaturationValue = startSmoking ? smokingColorAdjustment : initialSaturation;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(startVignetteIntensity, targetVignetteIntensity, t);
            }
            
            if (colorAdjustments != null)
            {
                colorAdjustments.saturation.value = Mathf.Lerp(startSaturationValue, targetSaturationValue, t);
            }
            
            elapsedTime += Time.deltaTime * postProcessTransitionSpeed;
            yield return null;
        }
        
        if (vignette != null)
        {
            vignette.intensity.value = targetVignetteIntensity;
        }
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = targetSaturationValue;
        }
        
        postProcessTransitionCoroutine = null;
    }    private void OnDestroy()
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
        
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = initialSaturation;
        }
    }
}