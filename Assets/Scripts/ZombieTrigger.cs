using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZombieTrigger : MonoBehaviour
{
    [SerializeField] private CoffinController coffin;
    [SerializeField] private Transform player;
    private bool activated;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null) trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated || coffin == null || !IsPlayer(other)) return;
        activated = true;
        coffin.OpenCoffin();
        gameObject.SetActive(false);
    }

    private bool IsPlayer(Collider other)
    {
        Transform root = other.transform.root;
        if (player == null && coffin != null && coffin.Zombie != null)
            player = coffin.Zombie.PlayerTransform;

        // The serialized reference is authoritative; the tag remains a safe fallback
        // for a correctly tagged player prefab.
        return player != null ? root == player : root.CompareTag("Player");
    }
}
