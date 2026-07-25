using UnityEngine;
using System.Collections;

public class LockedDoor : MonoBehaviour, IInteractable
{
    public enum DoorKeyType
    {
        Room1,
        Room2,
        Room3,
        Room4,
        Room5,
        ExitDoor
    }

    [Header("References")]
    public PlayerInventory playerInventory;
    public GameMessageUI gameMessageUI;
    public ObjectiveManager objectiveManager;

    [Header("Required Key")]
    public DoorKeyType requiredKey;

    [Header("Door Settings")]
    public float openAngle = -90f;
    public float openSpeed = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lockedSound;
    public AudioClip unlockSound;
    public AudioClip openSound;

    private bool isLocked = true;
    private bool isOpen = false;
    private bool isMoving = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    public void Interact()
    {
        if (isMoving)
            return;

        if (isLocked)
        {
            TryUnlockDoor();
            return;
        }

        ToggleDoor();
    }

    private void TryUnlockDoor()
    {
        if (playerInventory == null)
            return;

        if (!HasRequiredKey())
        {
            if (audioSource != null && lockedSound != null)
                audioSource.PlayOneShot(lockedSound);

            if (gameMessageUI != null)
                gameMessageUI.ShowMessage("KEY REQUIRED");

            return;
        }

        isLocked = false;

        if (audioSource != null && unlockSound != null)
            audioSource.PlayOneShot(unlockSound);
            if (objectiveManager != null)
            objectiveManager.CompleteDoorObjective();

        StartCoroutine(OpenAfterUnlock());
    }

    private bool HasRequiredKey()
    {
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

            case DoorKeyType.ExitDoor:
                return playerInventory.HasExitDoorKey();    
        }

        return false;
    }

    IEnumerator OpenAfterUnlock()
    {
        yield return new WaitForSeconds(0.4f);
        ToggleDoor();
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;

        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        StopAllCoroutines();
        StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
    }

    IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isMoving = true;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                openSpeed * Time.deltaTime);

            yield return null;
        }

        transform.localRotation = targetRotation;
        isMoving = false;
    }
}