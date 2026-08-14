using UnityEngine;

public class TorchPickup : MonoBehaviour, IPickup
{
    [Header("References")]
    public FlashlightController flashlightController;
    public GameObject flashlightButton;
    public GameObject playerTorchModel;
    public AudioClip pickupSound;
    public ObjectiveManager objectiveManager;

    public void Pickup()
    {
        // Play pickup sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position
            );
        }

        // Enable player's flashlight
        if (flashlightController != null)
        {
            flashlightController.EnableFlashlight();
        }

        // Show flashlight button
        if (flashlightButton != null)
        {
            flashlightButton.SetActive(true);
        }

        // Enable flashlight model on player
        if (playerTorchModel != null)
        {
            playerTorchModel.SetActive(true);
        }

        // Complete torch objective
        if (objectiveManager != null)
        {
            objectiveManager.CompleteTorchObjective();
        }

        // Destroy pickup object.
        // Any child Point Light will also be destroyed automatically.
        Destroy(gameObject);
    }
}