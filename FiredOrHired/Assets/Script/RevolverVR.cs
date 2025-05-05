using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class RevolverVR : MonoBehaviour
{
    [Header("References")]
    public Transform muzzleTransform;
    public GameObject bulletPrefab;

    [Header("Shooting")]
    public float bulletSpeed = 50f;
    public bool hasFired = false;

    private void Update()
    {
        // disparar tecla E
        if (!hasFired && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Fire();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed && !hasFired)
            Fire();
    }

    private void Fire()
    {
        hasFired = true;

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
    }
}
