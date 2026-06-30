using UnityEngine;

public class TorchPickup : MonoBehaviour, IInteractable
{
    public FlashlightController flashlightController;
    public GameObject flashlightButton;
        public GameObject playerTorchLight;
        public AudioClip pickupSound;


    public void Interact()
    {
        playerTorchLight.SetActive(true);
        AudioSource.PlayClipAtPoint(pickupSound, transform.position);


        flashlightController.EnableFlashlight();

        flashlightButton.SetActive(true);

        Destroy(gameObject);
    }
}
