using UnityEngine;

public class LeverHandlePickup : MonoBehaviour, IPickup
{
    public PlayerInventory playerInventory;
    public GameMessageUI gameMessageUI;

    public AudioClip pickupSound;

    public void Pickup()
    {
        if (playerInventory == null)
            return;

        playerInventory.AddLeverHandle();

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position);
        }

        if (gameMessageUI != null)
        {
            gameMessageUI.ShowMessage("LEVER HANDLE FOUND");
        }

        Destroy(gameObject);
    }
}