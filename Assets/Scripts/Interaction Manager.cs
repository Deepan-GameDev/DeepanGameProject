using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    public CenterDotController centerDot;
    public Camera playerCamera;
    public float interactDistance = 2.5f;
    public float interactRadius = 0.35f;
    public Button interactButton;

    private IInteractable currentInteractable;

    void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            SetCurrentInteractable(null);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        SetCurrentInteractable(FindInteractable(ray));
    }

    public void Interact()
    {
        currentInteractable?.Interact();
    }

   private void SetCurrentInteractable(IInteractable interactable)
{
    // If the target has not changed, do nothing.
    // This is important because Update() calls this every frame.
    if (currentInteractable == interactable)
        return;

    // Stop blinking on previous Note
    if (currentInteractable != null)
    {
        MonoBehaviour previousObject = currentInteractable as MonoBehaviour;

        if (previousObject != null)
        {
            NoteBlink previousNote =
                previousObject.GetComponentInParent<NoteBlink>();

            if (previousNote != null)
            {
                previousNote.SetBlink(false);
            }
        }
    }

    // Set new interactable
    currentInteractable = interactable;

    bool hasInteractable = currentInteractable != null;

    // Start blinking only for Note objects
    if (currentInteractable != null)
    {
        MonoBehaviour newObject = currentInteractable as MonoBehaviour;

        if (newObject != null)
        {
            NoteBlink newNote =
                newObject.GetComponentInParent<NoteBlink>();

            if (newNote != null)
            {
                newNote.SetBlink(true);
            }
        }
    }

    // Existing UI behaviour
    if (interactButton != null)
    {
        interactButton.gameObject.SetActive(hasInteractable);
    }

    if (centerDot != null)
    {
        centerDot.SetInteracting(hasInteractable);
    }
}
    private IInteractable FindInteractable(Ray ray)
    {
        RaycastHit[] hits = Physics.SphereCastAll(ray, interactRadius, interactDistance, ~0, QueryTriggerInteraction.Ignore);
        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            IInteractable interactable = hits[i].collider.GetComponentInParent<IInteractable>();
            if (interactable != null && hits[i].distance < closestDistance)
            {
                closestInteractable = interactable;
                closestDistance = hits[i].distance;
            }
        }

        return closestInteractable;
    }
}
