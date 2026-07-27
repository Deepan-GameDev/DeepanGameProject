using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Fade")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.8f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Scene starts with black screen
        fadeGroup.alpha = 1f;
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        fadeGroup.blocksRaycasts = true;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 0f;
        fadeGroup.blocksRaycasts = false;
    }

    public void LoadScene(string sceneName)
{
    if (sceneName == "level 1")
    {
        SaveManager.DeleteSave();
    }

    LoadingManager.sceneToLoad = sceneName;
    StartCoroutine(FadeOutAndLoad());
}

    public void ContinueGame(string sceneName)
{
    if (PlayerPrefs.GetInt("HasSave", 0) == 0)
        return;

    LoadingManager.sceneToLoad = sceneName;
    StartCoroutine(FadeOutAndLoad());
}

    public IEnumerator FadeOut()
    {
        fadeGroup.blocksRaycasts = true;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 1f;

        SceneManager.LoadScene("Loading Scene");
    }

    IEnumerator FadeOutAndLoad()
{
    yield return StartCoroutine(FadeOut());

    SceneManager.LoadScene("Loading Scene");
}
}
