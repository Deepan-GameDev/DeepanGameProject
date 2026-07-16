using UnityEngine;

public class CupboardDoor : MonoBehaviour, IInteractable
{
    [Header("Door Pivots")]
    public Transform leftDoorPivot;
    public Transform rightDoorPivot;

    [Header("Settings")]
    public float openAngle = 90f;
    public float openSpeed = 3f;

    private bool isOpen;

    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;

    private Quaternion leftOpenRotation;
    private Quaternion rightOpenRotation;

    private void Start()
    {
        leftClosedRotation = leftDoorPivot.localRotation;
        rightClosedRotation = rightDoorPivot.localRotation;

        // Reverse direction-na +/- maathunga
        leftOpenRotation = leftClosedRotation * Quaternion.Euler(0, -openAngle, 0);
        rightOpenRotation = rightClosedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    private void Update()
    {
        leftDoorPivot.localRotation = Quaternion.Slerp(
            leftDoorPivot.localRotation,
            isOpen ? leftOpenRotation : leftClosedRotation,
            Time.deltaTime * openSpeed);

        rightDoorPivot.localRotation = Quaternion.Slerp(
            rightDoorPivot.localRotation,
            isOpen ? rightOpenRotation : rightClosedRotation,
            Time.deltaTime * openSpeed);
    }

    public void Interact()
    {
        isOpen = !isOpen;
    }
}