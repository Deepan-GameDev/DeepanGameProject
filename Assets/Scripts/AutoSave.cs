using UnityEngine;

public class AutoSave : MonoBehaviour
{
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        // Save every 5 seconds
        if (timer >= 5f)
        {
            timer = 0f;

            PlayerPrefs.SetFloat("PlayerX", transform.position.x);
            PlayerPrefs.SetFloat("PlayerY", transform.position.y);
            PlayerPrefs.SetFloat("PlayerZ", transform.position.z);

            PlayerPrefs.Save();
        }
    }
}