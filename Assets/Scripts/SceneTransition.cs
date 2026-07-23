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

    public void LoadScene(string sceneName)
{
    LoadingManager.sceneToLoad = sceneName;
    StartCoroutine(FadeAndLoad());
}

    IEnumerator FadeAndLoad()
{
    fadeGroup.blocksRaycasts = true;

    float t = 0;

    while (t < fadeDuration)
    {
        t += Time.deltaTime;

        fadeGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);

        yield return null;
    }

    fadeGroup.alpha = 1;

    SceneManager.LoadScene("Loading Scene");
}
}