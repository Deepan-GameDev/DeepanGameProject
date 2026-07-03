using UnityEngine;

public class BatteryPickup : MonoBehaviour, IInteractable
{
    public FlashlightController flashlightController;

    [Header("Battery Settings")]
    public float rechargeAmount = 25f;

    [Header("Audio")]
    public AudioClip pickupSound;

    public void Interact()
    {
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