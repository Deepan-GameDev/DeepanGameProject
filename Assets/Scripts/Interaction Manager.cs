using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 2.5f;
    public Button interactButton;

    private IInteractable currentInteractable;

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            currentInteractable = hit.collider.GetComponent<IInteractable>();

            interactButton.gameObject.SetActive(currentInteractable != null);
        }
        else
        {
            currentInteractable = null;
            interactButton.gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        currentInteractable?.Interact();
    }
}