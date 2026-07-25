using TMPro;
using UnityEngine;

public class NoteInteractable : MonoBehaviour, IInteractable
{
    [Header("UI")]
    public GameObject notePanel;
    public TMP_Text noteText;

    [TextArea(5,15)]
    public string message;

    public void Interact()
    {
        NoteManager.Instance.OpenNote(message);
    }

    public void CloseNote()
    {
        notePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}