using UnityEngine;
using System.Collections;

public class BoatEscape : MonoBehaviour, IInteractable
{
    [Header("References")]
    public GameMessageUI gameMessageUI;
    public Player player;
    public GameObject gameplayUI;
    public Camera playerCamera;
    public Camera escapeCamera;
    public PlayerInventory playerInventory;

    [Header("Boat Audio")]
    public AudioSource boatAudioSource;
    public AudioClip boatStartSound;

    [Header("Escape UI")]
    public CanvasGroup escapeCanvasGroup;

    [Header("Boat Movement")]
    public float boatSpeed = 4f;
    public float escapeDuration = 8f;

    [Header("Fade")]
    public float fadeDuration = 2f;

    [Header("Ending Cutscene")]
    [SerializeField] private EndingCutsceneManager endingCutscene;

    private bool hasEscaped;
    private bool isMoving;

    public void Interact()
    {
        if (hasEscaped)
            return;

        if (playerInventory == null || !playerInventory.HasBoatKey())
        {
            if (gameMessageUI != null)
            {
                gameMessageUI.ShowMessage("BOAT KEY REQUIRED");
            }

            return;
        }

        hasEscaped = true;

        StartCoroutine(StartEscape());
    }

    private IEnumerator StartEscape()
    {
        if (gameMessageUI != null)
        {
            gameMessageUI.ShowMessage("ESCAPING...");
        }

        // Disable player
        if (player != null)
        {
            player.enabled = false;

            Rigidbody playerRb = player.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.isKinematic = true;
            }

            player.transform.SetParent(transform);
        }

        // Hide gameplay UI
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        // Boat sound
        if (boatAudioSource != null && boatStartSound != null)
        {
            boatAudioSource.PlayOneShot(boatStartSound);
        }

        // Switch camera
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }

        if (escapeCamera != null)
        {
            escapeCamera.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        // Start boat movement
        isMoving = true;

        yield return new WaitForSeconds(escapeDuration);

        // Stop boat
        isMoving = false;

        // Existing YOU ESCAPED UI
        yield return StartCoroutine(FadeEscapePanel());

        // The existing escape panel remains visible while the dedicated ending
        // controller begins the immediate camera transition.
        if (endingCutscene != null)
        {
            endingCutscene.Begin(escapeCamera, escapeCanvasGroup);
        }
    }

    private void Update()
    {
        if (!isMoving)
            return;

        transform.position +=
            transform.forward *
            boatSpeed *
            Time.deltaTime;
    }

    private IEnumerator FadeEscapePanel()
    {
        if (escapeCanvasGroup == null)
            yield break;

        escapeCanvasGroup.alpha = 0f;
        escapeCanvasGroup.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            escapeCanvasGroup.alpha = Mathf.Lerp(
                0f,
                1f,
                timer / fadeDuration
            );

            yield return null;
        }

        escapeCanvasGroup.alpha = 1f;
        escapeCanvasGroup.interactable = true;
        escapeCanvasGroup.blocksRaycasts = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}
