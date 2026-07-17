using UnityEngine;

public class CupboardDoor : MonoBehaviour, IInteractable
{
    [Header("Door Pivot")]
    public Transform doorPivot;

    [Header("Settings")]
    public float openAngle = 90f;
    public float openSpeed = 3f;
    public bool reverseDirection = false;

    private bool isOpen;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private void Start()
    {
        closedRotation = doorPivot.localRotation;

        float angle = reverseDirection ? -openAngle : openAngle;

        openRotation = closedRotation * Quaternion.Euler(0, angle, 0);
    }

    private void Update()
    {
        doorPivot.localRotation = Quaternion.Slerp(
            doorPivot.localRotation,
            isOpen ? openRotation : closedRotation,
            Time.deltaTime * openSpeed);
    }

    public void Interact()
{
    isOpen = !isOpen;

    if (isOpen)
        audioSource.PlayOneShot(openSound);
    else
        audioSource.PlayOneShot(closeSound);
}
}