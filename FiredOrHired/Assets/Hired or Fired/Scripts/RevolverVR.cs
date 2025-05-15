using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class RevolverVR : MonoBehaviour
{
    [Header("Configuration")]
    public Transform muzzleTransform;
    public GameObject bulletPrefab;
    public float bulletSpeed = 50f;

    [Header("Cargador")]
    public Transform cylinderTransform;
    public float rotationDuration = 0.2f;
    public float rotationAngle = 60f;
    public GameObject[] bulletMeshes = new GameObject[6];

    [Header("Trigger")]
    public Transform hammer;
    public float hammerAngle = 35f;
    public Transform trigger;
    public float triggerAngle = 15f;

    [Header("Recarga")]
    public float reloadSpinSpeed = 360f;

    [Header("Recoil")]
    public float recoilDistance = 0.05f;
    public float recoilDuration = 0.1f;
    public float recoilRotation = 3f;

    [Header("Visual Efects")]
    public ParticleSystem muzzleFlash;
    public AudioClip gunshotSound;
    public AudioClip hammerPullSound;
    public AudioClip triggerClickSound;
    public AudioClip cylinderSpinSound;
    public AudioClip reloadSound;
    private AudioSource audioSource;

    private bool isHammerPulled = false;
    private bool isFiring = false;
    private bool isReloading = false;
    private bool isRotating = false;

    private int currentChamber = 0;
    private float currentRotation = 0f;

    void Awake()
    {
        //audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f;
    }

    private void Update()
    {
        //Para debug disparar con ESPACIO y luego E
        //TODO: Cambiar para mandos VR
        if (Keyboard.current.spaceKey.wasPressedThisFrame) PullHammer();
        if (Keyboard.current.eKey.wasPressedThisFrame) PullTrigger();
    }

    // ==================== DISPARO ====================
    public void PullHammer()
    {
        if (!isHammerPulled && !isFiring)
        {
            //visual effectes
            audioSource.PlayOneShot(hammerPullSound);

            StartCoroutine(AnimateHammer(hammerAngle, 0.2f));
            isHammerPulled = true;
        }
    }

    public void PullTrigger()
    {
        if (isHammerPulled && !isFiring)
        {
            StartCoroutine(TriggerPullSequence());
        }
    }

    public void Fire()
    {
        if (!bulletMeshes[currentChamber].activeSelf) return;

        // VisualEfects
        if (muzzleFlash != null) muzzleFlash.Play();
        if (gunshotSound != null) audioSource.PlayOneShot(gunshotSound);

        bulletMeshes[currentChamber].SetActive(false);
        InstantiateBullet();
        StartCoroutine(RecoilAnimation());
        RotateCylinder();
        currentChamber = (currentChamber + 1) % 6;
    }

    // ==================== SISTEMA DE CARGADOR ====================
    private void RotateCylinder()
    {
        if (!isRotating) StartCoroutine(RotateCylinderCoroutine());
    }

    private IEnumerator RotateCylinderCoroutine()
    {
        //visualEffects
        audioSource.PlayOneShot(cylinderSpinSound);

        isRotating = true;
        float targetRotation = currentRotation + rotationAngle;

        Quaternion startRot = cylinderTransform.localRotation;
        Quaternion targetRot = Quaternion.Euler(0, 0, targetRotation);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            cylinderTransform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cylinderTransform.localRotation = targetRot;
        currentRotation = targetRotation % 360;
        isRotating = false;

        if (CheckEmptyChamber()) StartCoroutine(ReloadAnimation());
    }

    // ==================== SISTEMA DE RECARGA ====================
    private IEnumerator ReloadAnimation()
    {
        isReloading = true;

        //visual effects
        audioSource.PlayOneShot(reloadSound);

        yield return StartCoroutine(SpinCylinder());
        ReloadMagazine();

        isReloading = false;
    }

    private IEnumerator SpinCylinder()
    {
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cylinderTransform.Rotate(0, 0, reloadSpinSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // ==================== ANIMACIONES ====================
    private IEnumerator RecoilAnimation()
    {
        Vector3 initialPosition = transform.position;
        Quaternion initialRotation = transform.rotation;

        Vector3 recoilOffset = -muzzleTransform.forward * recoilDistance;
        Quaternion recoilRot = Quaternion.Euler(-recoilRotation, 0, 0);

        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            transform.position = Vector3.Lerp(initialPosition + recoilOffset, initialPosition, elapsed / recoilDuration);
            transform.rotation = Quaternion.Lerp(initialRotation * recoilRot, initialRotation, elapsed / recoilDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator TriggerPullSequence()
    {
        isFiring = true;


        yield return StartCoroutine(AnimateTrigger(triggerAngle, 0.1f));

        if (isHammerPulled) Fire();
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(AnimateHammer(0, 0.15f));
        yield return StartCoroutine(AnimateTrigger(0, 0.1f));

        isHammerPulled = false;
        isFiring = false;
    }

    private IEnumerator AnimateTrigger(float angle, float duration)
    {
        //visual effects
        audioSource.PlayOneShot(triggerClickSound); 

        Quaternion startRot = trigger.localRotation;
        Quaternion targetRot = Quaternion.Euler(angle, 0, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            trigger.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimateHammer(float angle, float duration)
    {
        Quaternion startRot = hammer.localRotation;
        Quaternion targetRot = Quaternion.Euler(-angle, 0, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            hammer.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // ==================== UTILIDADES ====================
    private void InstantiateBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation);
        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = muzzleTransform.forward * bulletSpeed;
        }
    }

    private bool CheckEmptyChamber()
    {
        foreach (GameObject bullet in bulletMeshes)
        {
            if (bullet.activeSelf) return false;
        }
        return true;
    }

    private void ReloadMagazine()
    {
        currentChamber = 0;
        currentRotation = 0f;
        cylinderTransform.localRotation = Quaternion.identity;

        foreach (GameObject bullet in bulletMeshes)
        {
            bullet.SetActive(true);
        }
    }
}