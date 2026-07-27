using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Door door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Zombie"))
        {
            door.OpenForZombie();
        }
    }
}