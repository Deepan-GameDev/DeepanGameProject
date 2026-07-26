using UnityEngine;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
    public Button continueButton;

    void Start()
    {
        bool hasSave = PlayerPrefs.GetInt("HasSave", 0) == 1;

        continueButton.interactable = hasSave;
    }
}