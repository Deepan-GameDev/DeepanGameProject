using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public enum DoorKeyType
    {
        None,
        Room1,
        Room2,
        Room3,
        Room4,
        Room5
    }

    [Header("Door Settings")]
    public DoorKeyType requiredKey = DoorKeyType.None;
    public PlayerInventory playerInventory;

    public bool open;
    public float smooth = 1f;
    public float openAngle = -90f;
    public AudioClip openDoor;
    public AudioClip closeDoor;
    public bool canZombieOpenLockedDoor = false;

    private AudioSource audioSource;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    void Update()
    {
        Quaternion targetRotation = open ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * 5f * smooth);
    }

    public void Interact()
    {
        if (!HasRequiredKey())
            return;

        OpenDoor();
    }

    bool HasRequiredKey()
    {
        if (requiredKey == DoorKeyType.None)
            return true;

        if (playerInventory == null)
            return false;

        switch (requiredKey)
        {
            case DoorKeyType.Room1:
                return playerInventory.HasRoom1Key();

            case DoorKeyType.Room2:
                return playerInventory.HasRoom2Key();

            case DoorKeyType.Room3:
                return playerInventory.HasRoom3Key();

            case DoorKeyType.Room4:
                return playerInventory.HasRoom4Key();

            case DoorKeyType.Room5:
                return playerInventory.HasRoom5Key();
        }

        return false;
    }

    void OpenDoor()
    {
        open = !open;

        if (audioSource != null)
        {
            AudioClip clip = open ? openDoor : closeDoor;

            if (clip != null)
                audioSource.PlayOneShot(clip);
        }
    }

    public void OpenForZombie()
    {
        if (open || (requiredKey != DoorKeyType.None && !canZombieOpenLockedDoor))
            return;

        open = true;

        if (audioSource != null && openDoor != null)
            audioSource.PlayOneShot(openDoor);
    }
}
