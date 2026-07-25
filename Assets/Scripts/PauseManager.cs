using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class PauseManager : MonoBehaviour
{
    public GameObject settingsPanel;
    [Header("UI")]
    public GameObject pausePanel;

    public SceneTransition sceneTransition;

    private bool isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        pausePanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void OpenSettings()
{
    pausePanel.SetActive(false);
    settingsPanel.SetActive(true);
}

public void CloseSettings()
{
    settingsPanel.SetActive(false);
    pausePanel.SetActive(true);
}
   

public void GoToMainMenu()
{
    Time.timeScale = 1f;

    sceneTransition.LoadScene("Main Menu");
}

public void RestartGame()
{
    Time.timeScale = 1f;

    SceneTransition.Instance.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
}

IEnumerator LoadMainMenu()
{
    Time.timeScale = 1f;

    yield return StartCoroutine(sceneTransition.FadeOut());

    SceneManager.LoadScene("Main Menu");
}

}