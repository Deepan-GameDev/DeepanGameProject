using UnityEngine;

public class ZombieTrigger : MonoBehaviour
{
    public CoffinController coffin;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        coffin.OpenCoffin();

        gameObject.SetActive(false);
    }
}