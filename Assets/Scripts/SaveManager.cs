using UnityEngine;

public static class SaveManager
{
    private static readonly string[] GameplayKeys =
    {
        "HasSave",
        "PlayerX",
        "PlayerY",
        "PlayerZ",
        "PlayerRotY",
        "Room1Key",
        "Room2Key",
        "Room3Key",
        "Room4Key",
        "Room5Key",
        "ExitDoorKey",
        "Room 1",
        "Room 2",
        "Room 3",
        "Room 4",
        "Room 5",
        "Exit Door"
    };

    public static void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static int LoadInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public static float LoadFloat(string key, float defaultValue = 0)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    public static void SaveBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool LoadBool(string key)
    {
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public static void SaveCheckpoint(Transform player)
    {
        if (player == null) return;

        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.SetFloat("PlayerX", player.position.x);
        PlayerPrefs.SetFloat("PlayerY", player.position.y);
        PlayerPrefs.SetFloat("PlayerZ", player.position.z);
        PlayerPrefs.SetFloat("PlayerRotY", player.eulerAngles.y);
        PlayerPrefs.Save();
    }

    public static Vector3 LoadCheckpointPosition(Vector3 fallback)
    {
        return new Vector3(
            PlayerPrefs.GetFloat("PlayerX", fallback.x),
            PlayerPrefs.GetFloat("PlayerY", fallback.y),
            PlayerPrefs.GetFloat("PlayerZ", fallback.z));
    }

    public static Quaternion LoadCheckpointRotation(Quaternion fallback)
    {
        return Quaternion.Euler(
            fallback.eulerAngles.x,
            PlayerPrefs.GetFloat("PlayerRotY", fallback.eulerAngles.y),
            fallback.eulerAngles.z);
    }

    public static void DeleteSave()
    {
        for (int i = 0; i < GameplayKeys.Length; i++)
        {
            PlayerPrefs.DeleteKey(GameplayKeys[i]);
        }

        PlayerPrefs.Save();
    }
}
