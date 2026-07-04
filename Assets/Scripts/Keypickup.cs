using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    public PlayerInventory playerInventory;
    public AudioClip pickupSound;

    public void Interact()
    {
        if (playerInventory == null)
            return;

        playerInventory.AddKey();

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