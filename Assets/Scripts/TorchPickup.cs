using UnityEngine;

public class TorchPickup : MonoBehaviour, IInteractable
{
    public FlashlightController flashlightController;
    public GameObject flashlightButton;
    public AudioClip pickupSound;

    public void Interact()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        flashlightController.EnableFlashlight();
        flashlightButton.SetActive(true);

        Destroy(gameObject);
    }
}