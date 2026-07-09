using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasKey = false;
    private bool hasBoatKey = false;

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

    public void AddBoatKey()
{
    hasBoatKey = true;
}

public bool HasBoatKey()
{
    return hasBoatKey;
}

public void UseBoatKey()
{
    hasBoatKey = false;
}
}