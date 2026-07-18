using UnityEngine;

public class BoatKeyPickup : MonoBehaviour, IPickup
{
    [Header("References")]
    public PlayerInventory playerInventory;
    public GameMessageUI gameMessageUI;

    [Header("Audio")]
    public AudioClip pickupSound;

    public void Pickup()
    {
        if (playerInventory == null)
            return;

        playerInventory.AddBoatKey();

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position
            );
        }

        if (gameMessageUI != null)
        {
            gameMessageUI.ShowMessage("BOAT KEY FOUND");
        }

        Destroy(gameObject);
    }
}