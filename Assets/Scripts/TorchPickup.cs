using UnityEngine;

public class TorchPickup : MonoBehaviour, IInteractable
{
    public FlashlightController flashlightController;
    public GameObject flashlightButton;

    public void Interact()
    {
        flashlightController.EnableFlashlight();

        flashlightButton.SetActive(true);

        Destroy(gameObject);
    }
}