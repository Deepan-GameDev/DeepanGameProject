using UnityEngine;

public class NoteReader : MonoBehaviour
{
    [Header("UI")]
    public GameObject notePanel;

    public void OpenNote()
    {
        notePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void CloseNote()
    {
        notePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}