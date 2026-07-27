using UnityEngine;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
    public Button continueButton;
    [SerializeField] private string sceneToLoad = "level 1";

    void Start()
    {
        bool hasSave = PlayerPrefs.GetInt("HasSave", 0) == 1;

        if (continueButton == null)
            return;

        continueButton.interactable = hasSave;

        continueButton.onClick.RemoveListener(ContinueGame);
        continueButton.onClick.AddListener(ContinueGame);
    }

    private void ContinueGame()
    {
        if (SceneTransition.Instance == null)
            return;

        SceneTransition.Instance.ContinueGame(sceneToLoad);
    }
}
