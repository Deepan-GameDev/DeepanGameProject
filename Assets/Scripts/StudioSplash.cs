using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StudioSplash : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    [SerializeField] private string nextSceneName = "MainMenu";

    private void Start()
    {
        StartCoroutine(PlaySplash());
    }

    private IEnumerator PlaySplash()
    {
        canvasGroup.alpha = 0f;

        // Fade In
        float time = 0f;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;

            canvasGroup.alpha =
                Mathf.Clamp01(time / fadeInDuration);

            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;

            canvasGroup.alpha =
                1f - Mathf.Clamp01(time / fadeOutDuration);

            yield return null;
        }

        canvasGroup.alpha = 0f;

        SceneManager.LoadScene(nextSceneName);
    }
}