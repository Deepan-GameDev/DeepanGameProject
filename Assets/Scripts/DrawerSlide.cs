using UnityEngine;

public class DrawerSlide : MonoBehaviour, IInteractable
{
    [Header("Drawer Settings")]
    [SerializeField] private float slideDistance = 0.35f;
    [SerializeField] private float slideSpeed = 4f;
    [SerializeField] private Vector3 slideDirection = Vector3.forward;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool isOpen;
    private bool itemRevealed;

    private void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + slideDirection.normalized * slideDistance;

    }

    private void Update()
    {
        Vector3 target = isOpen ? openPosition : closedPosition;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            target,
            Time.deltaTime * slideSpeed);
    }

    public void Interact()
    {
        isOpen = !isOpen;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(isOpen ? openSound : closeSound);
        }

    }

    public bool IsOpen()
    {
        return isOpen;
    }
}