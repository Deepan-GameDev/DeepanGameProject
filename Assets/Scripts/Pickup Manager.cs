using UnityEngine;
using UnityEngine.UI;

public class PickupManager : MonoBehaviour
{
    public Camera playerCamera;
    public float pickupDistance = 2.5f;
    public float pickupRadius = 0.35f;
    public Button pickupButton;

    private IPickup currentPickup;

    private void Update()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
        {
            SetCurrentPickup(null);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        SetCurrentPickup(FindPickup(ray));
    }

    public void Pickup()
    {
        currentPickup?.Pickup();
    }

    private void SetCurrentPickup(IPickup pickup)
    {
        currentPickup = pickup;

        if (pickupButton != null)
            pickupButton.gameObject.SetActive(currentPickup != null);
    }
    private void Start()
{
    if (pickupButton != null)
        pickupButton.gameObject.SetActive(false);
}

    private IPickup FindPickup(Ray ray)
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            ray,
            pickupRadius,
            pickupDistance,
            ~0,
            QueryTriggerInteraction.Ignore);

        IPickup closest = null;
        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            IPickup pickup = hit.collider.GetComponentInParent<IPickup>();

            if (pickup != null && hit.distance < closestDistance)
            {
                closest = pickup;
                closestDistance = hit.distance;
            }
        }

        return closest;
    }
}