using UnityEngine;

public class DrawerSlide : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public float slideDistance = 0.35f;
    public float slideSpeed = 4f;

    [Tooltip("Choose the direction the drawer should slide.")]
    public Vector3 slideDirection = Vector3.forward;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool isOpen;

    private void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + slideDirection.normalized * slideDistance;
    }

    private void Update()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            isOpen ? openPosition : closedPosition,
            Time.deltaTime * slideSpeed);
    }

    public void Interact()
    {
        Debug.Log("Drawer Interacted");
        isOpen = !isOpen;
    }
}