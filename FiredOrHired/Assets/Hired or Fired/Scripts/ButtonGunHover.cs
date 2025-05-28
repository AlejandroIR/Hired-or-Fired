using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonGunHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject gunIcon;
    public AudioSource shotSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gunIcon != null)
            gunIcon.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (gunIcon != null)
            gunIcon.SetActive(false);
    }

    public void OnClickShot()
    {
        if (shotSound != null)
            shotSound.PlayOneShot(shotSound.clip);
    }
}