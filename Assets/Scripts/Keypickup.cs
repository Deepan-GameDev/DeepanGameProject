using UnityEngine;

public class KeyPickup : MonoBehaviour, IPickup
{
    public enum KeyType
    {
        Room1,
        Room2,
        Room3,
        Room4,
        Room5,
        ExitDoor
    }
    [Header("Zombie Spawn")]
public GameObject zombie;

    [Header("Key Settings")]
    public KeyType keyType;

    public PlayerInventory playerInventory;
    public GameMessageUI gameMessageUI;
    public AudioClip pickupSound;
    public ObjectiveManager objectiveManager;

    public void Pickup()
    {
        DrawerSlide drawer = GetComponentInParent<DrawerSlide>();

        if (drawer != null && !drawer.IsOpen())
            return;

        if (playerInventory == null)
            return;

        switch (keyType)
        {
            case KeyType.Room1:

    playerInventory.AddRoom1Key();

    if (zombie != null)
    {
        zombie.SetActive(true);
    }

    break;

            case KeyType.Room2:
                playerInventory.AddRoom2Key();
                break;

            case KeyType.Room3:
                playerInventory.AddRoom3Key();
                break;

            case KeyType.Room4:
                playerInventory.AddRoom4Key();
                break;

            case KeyType.Room5:
                playerInventory.AddRoom5Key();
                break;

            case KeyType.ExitDoor:
                playerInventory.AddExitDoorKey();
                break;         
        }

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        if (gameMessageUI != null)
        {
            if (gameMessageUI != null)
{
    string message = "";

    switch (keyType)
    {
        case KeyType.Room1:
            message = "ROOM 1 KEY FOUND";
            break;

        case KeyType.Room2:
            message = "ROOM 2 KEY FOUND";
            break;

        case KeyType.Room3:
            message = "ROOM 3 KEY FOUND";
            break;

        case KeyType.Room4:
            message = "ROOM 4 KEY FOUND";
            break;

        case KeyType.Room5:
            message = "ROOM 5 KEY FOUND";
            break;

        case KeyType.ExitDoor:
            message = "EXITDOOR KEY FOUND";
            break;
    }

    gameMessageUI.ShowMessage(message);
}
        }
        
        if (objectiveManager != null)
        {
            objectiveManager.CompleteKeyObjective();
        }

        Destroy(gameObject);
    }

}