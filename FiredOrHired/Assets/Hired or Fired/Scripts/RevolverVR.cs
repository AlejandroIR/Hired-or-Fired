using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RevolverVR : MonoBehaviour
{

    [Header("Configuration")]
    public Transform muzzleTransform;
    public GameObject bulletPrefab;
    public float bulletSpeed = 50f;

    [Header("Balas del tambor")]
    public GameObject[] bulletMeshes = new GameObject[6];

    private int maxAmmo;
    private int currentAmmo;
    private bool hasFired = false;

    private void Start()
    {
        maxAmmo = bulletMeshes.Length;
        ReloadMagazine();
    }

    private void Update()
    {
        // disparar tecla E
        if (!hasFired && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Fire();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed && !hasFired) Fire();
    }

    public void Fire()
    {
        hasFired = true;
        currentAmmo--;

        if (currentAmmo >= 0 && currentAmmo < bulletMeshes.Length)
        {
            bulletMeshes[currentAmmo].SetActive(false);
        }

        GameObject bulletGO = Instantiate(bulletPrefab, muzzleTransform.position, muzzleTransform.rotation);

        Bullet bulletScript = bulletGO.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.speed = bulletSpeed;
            bulletScript.Init(this);
        }
    }
    public void ResetFire()
    {
        hasFired = false;

        if (currentAmmo <= 0)
        {
            ReloadMagazine();
        }
    }

    private void ReloadMagazine()
    {
        currentAmmo = maxAmmo;
        for (int i = 0; i < bulletMeshes.Length; i++)
        {
            if (bulletMeshes[i] != null)
                bulletMeshes[i].SetActive(true);
        }
    }
}
