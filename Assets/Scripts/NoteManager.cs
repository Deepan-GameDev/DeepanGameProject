using TMPro;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    [Header("UI")]
    public GameObject notePanel;
    public TMP_Text noteText;

    private void Awake()
    {
        Instance = this;
    }

    public void OpenNote(string message)
    {
        notePanel.SetActive(true);

        noteText.text = message;

        Time.timeScale = 0f;
    }

    public void CloseNote()
    {
        notePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}