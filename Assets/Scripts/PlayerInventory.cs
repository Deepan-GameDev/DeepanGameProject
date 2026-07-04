using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasKey = false;

    public void AddKey()
    {
        hasKey = true;

        Debug.Log("Key Picked Up");
    }

    public bool HasKey()
    {
        return hasKey;
    }

    public void UseKey()
    {
        hasKey = false;
    }
}