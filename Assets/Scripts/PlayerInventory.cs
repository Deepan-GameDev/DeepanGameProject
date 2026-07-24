using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Room Keys")]
    public bool hasRoom1Key;
    public bool hasRoom2Key;
    public bool hasRoom3Key;
    public bool hasRoom4Key;
    public bool hasRoom5Key;
    public bool hasExitDoorKey;


    [Header("Other Items")]
    private bool hasBoatKey = false;
    private bool hasLeverHandle = false;

    public void AddRoom1Key() => hasRoom1Key = true;
    public void AddRoom2Key() => hasRoom2Key = true;
    public void AddRoom3Key() => hasRoom3Key = true;
    public void AddRoom4Key() => hasRoom4Key = true;
    public void AddRoom5Key() => hasRoom5Key = true;
    public void AddExitDoorKey() => hasExitDoorKey = true;


    public bool HasRoom1Key() => hasRoom1Key;
    public bool HasRoom2Key() => hasRoom2Key;
    public bool HasRoom3Key() => hasRoom3Key;
    public bool HasRoom4Key() => hasRoom4Key;
    public bool HasRoom5Key() => hasRoom5Key;
    public bool HasExitDoorKey() => hasExitDoorKey;

    // ---------------- BOAT KEY ----------------

    public void AddBoatKey()
    {
        hasBoatKey = true;
    }

    public bool HasBoatKey()
    {
        return hasBoatKey;
    }

    // ---------------- LEVER HANDLE ----------------

    public void AddLeverHandle()
    {
        hasLeverHandle = true;
    }

    public void UseLeverHandle()
{
    hasLeverHandle = false;
}

    public bool HasLeverHandle()
    {
        return hasLeverHandle;
    }
}