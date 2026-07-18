using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    public PlayerInventory playerInventory;
    public GameMessageUI gameMessageUI;
    public AudioClip pickupSound;
    public ObjectiveManager objectiveManager;

    public void Interact()
    {
        DrawerSlide drawer = GetComponentInParent<DrawerSlide>();

if(drawer != null && !drawer.IsOpen())
    return;
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

        if (gameMessageUI != null)
        {
            gameMessageUI.ShowMessage("KEY FOUND");
        }

        if (objectiveManager != null)
{
    objectiveManager.CompleteKeyObjective();
}

        Destroy(gameObject);
    }
    
}