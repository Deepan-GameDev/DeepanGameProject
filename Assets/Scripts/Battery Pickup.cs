using UnityEngine;

public class BatteryPickup : MonoBehaviour, IPickup
{
    public FlashlightController flashlightController;

    [Header("Battery Settings")]
    public float rechargeAmount = 25f;

    [Header("Audio")]
    public AudioClip pickupSound;

    public void Pickup()
    {
        DrawerSlide drawer = GetComponentInParent<DrawerSlide>();

if(drawer != null && !drawer.IsOpen())
    return;
        if (flashlightController == null)
            return;

        flashlightController.AddBattery(rechargeAmount);

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position
            );
        }

        Destroy(gameObject);
    }
}