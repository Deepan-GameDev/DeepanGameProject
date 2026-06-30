using UnityEngine;

public class TorchPickup : MonoBehaviour, IInteractable
{
    public GameObject playerTorchLight;
    public AudioClip pickupSound;

    public void Interact()
    {
        // Turn on flashlight
        playerTorchLight.SetActive(true);

        // Play pickup sound
        AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        // Remove torch
        Destroy(gameObject);
    }
}