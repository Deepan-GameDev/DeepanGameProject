using UnityEngine;

public class TorchPickup : MonoBehaviour, IInteractable
{
    public FlashlightController flashlightController;
    public GameObject flashlightButton;
    public GameObject playerTorchModel;
    public AudioClip pickupSound;
    public ObjectiveManager objectiveManager;

    public void Interact()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position
            );
        }

        flashlightController.EnableFlashlight();

        flashlightButton.SetActive(true);

        if (playerTorchModel != null)
        {
            playerTorchModel.SetActive(true);
        }

        if (objectiveManager != null)
{
    objectiveManager.CompleteTorchObjective();
}

        Destroy(gameObject);
    }
}