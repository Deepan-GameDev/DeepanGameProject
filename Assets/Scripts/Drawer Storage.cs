using UnityEngine;

public class DrawerStorage : MonoBehaviour
{
    [SerializeField] private DrawerSlide drawer;

    public bool CanPickup()
    {
        return drawer != null && drawer.IsOpen();
    }
}