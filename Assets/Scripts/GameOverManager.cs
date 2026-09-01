using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    public GameObject gameOverPanel;
    public CanvasGroup gameOverCanvasGroup;
    public Player player;
    public GameObject gameplayUI;

    [Header("Game Over Fade")]
    public float fadeDelay = 0.5f;
    public float fadeDuration = 2f;

    private bool isGameOver = false;
    private bool waitingForInterstitial = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
        }

        SubscribeToInterstitialEvent();
    }

    private void SubscribeToInterstitialEvent()
    {
        if (LevelPlayAdsManager.Instance != null)
        {
            LevelPlayAdsManager.Instance.OnInterstitialClosedEvent -= OnInterstitialClosed;
            LevelPlayAdsManager.Instance.OnInterstitialClosedEvent += OnInterstitialClosed;
        }
    }

    private void UnsubscribeFromInterstitialEvent()
    {
        if (LevelPlayAdsManager.Instance != null)
        {
            LevelPlayAdsManager.Instance.OnInterstitialClosedEvent -= OnInterstitialClosed;
        }
    }

    // ============================================================
    // FINAL GAME OVER
    // ============================================================

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        Debug.Log("[GameOver] FINAL DEATH - Game Over started.");

        // Stop player input immediately.
        if (player != null)
        {
            player.enabled = false;
        }

        // Hide gameplay UI.
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        // Make sure game is running while the ad is shown.
        Time.timeScale = 1f;

        // Try to show interstitial first.
        if (TryShowInterstitial())
        {
            waitingForInterstitial = true;

            Debug.Log(
                "[GameOver] Interstitial requested. " +
                "Waiting for ad to close."
            );

            return;
        }

        // If ad is unavailable, do NOT block the player.
        Debug.Log(
            "[GameOver] Interstitial unavailable. " +
            "Showing Game Over panel directly."
        );

        StartCoroutine(GameOverRoutine());
    }

    private bool TryShowInterstitial()
    {
        if (LevelPlayAdsManager.Instance == null)
        {
            Debug.LogWarning(
                "[GameOver] LevelPlayAdsManager not found."
            );

            return false;
        }

        if (!LevelPlayAdsManager.Instance.IsInterstitialReady())
        {
            Debug.LogWarning(
                "[GameOver] Interstitial is not ready."
            );

            // Ask LevelPlay to prepare the next ad.
            LevelPlayAdsManager.Instance.LoadInterstitial();

            return false;
        }

        Debug.Log(
            "[GameOver] Interstitial is ready. Showing ad."
        );

        LevelPlayAdsManager.Instance.ShowInterstitial();

        return true;
    }

    // ============================================================
    // INTERSTITIAL CLOSED
    // ============================================================

    private void OnInterstitialClosed()
    {
        if (!waitingForInterstitial)
            return;

        waitingForInterstitial = false;

        Debug.Log(
            "[GameOver] Interstitial finished. " +
            "Showing Game Over panel."
        );

        StartCoroutine(GameOverRoutine());
    }

    // ============================================================
    // GAME OVER PANEL
    // ============================================================

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSecondsRealtime(fadeDelay);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverCanvasGroup == null)
        {
            Time.timeScale = 0f;
            yield break;
        }

        gameOverCanvasGroup.alpha = 0f;
        gameOverCanvasGroup.interactable = false;
        gameOverCanvasGroup.blocksRaycasts = false;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            gameOverCanvasGroup.alpha = Mathf.Lerp(
                0f,
                1f,
                timer / fadeDuration
            );

            yield return null;
        }

        gameOverCanvasGroup.alpha = 1f;
        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;

        Time.timeScale = 0f;

        Debug.Log("[GameOver] Game Over panel displayed.");
    }

    // ============================================================
    // RESTART
    // ============================================================

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        UnsubscribeFromInterstitialEvent();
    }
}