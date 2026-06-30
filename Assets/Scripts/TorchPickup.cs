using UnityEngine;

public class TorchPickup : MonoBehaviour, IInteractable
{
    public GameObject playerTorchLight;

    public void Interact()
    {
        playerTorchLight.SetActive(true);
        Destroy(gameObject);
    }
}