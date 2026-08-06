using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudioSplash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 1.5f;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float fadeOutTime = 1.5f;

    [Header("Scene")]
    [SerializeField] private string nextScene = "MainMenu";

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        StartCoroutine(SplashRoutine());
    }

    private IEnumerator SplashRoutine()
    {
        // Fade In
        yield return StartCoroutine(Fade(0f, 1f, fadeInTime));

        // Hold
        yield return new WaitForSecondsRealtime(holdTime);

        // Fade Out
        yield return StartCoroutine(Fade(1f, 0f, fadeOutTime));

        // Load Scene
        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            canvasGroup.alpha = Mathf.Lerp(start, end, time / duration);

            yield return null;
        }

        canvasGroup.alpha = end;
    }
}