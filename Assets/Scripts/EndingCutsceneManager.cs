using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Owns only the post-escape ending cinematic. It does not alter gameplay systems.
/// </summary>
public class EndingCutsceneManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera endingCamera;
    [SerializeField, Min(0.01f)] private float fadeIntoEndingCameraDuration = 1f;
    [SerializeField, Min(0f)] private float blackCameraSwitchDelay = 0.15f;

    [Header("Cinematic Zombie")]
    [SerializeField] private ZombieAI zombieAI;
    [SerializeField] private GameObject zombie;
    [SerializeField] private Transform zombieStartPoint;
    [SerializeField] private Transform zombieStopPoint;
    [SerializeField] private Animator zombieAnimator;
    [SerializeField] private AudioSource zombieAudioSource;
    [SerializeField] private AudioClip zombieScreamClip;
    [SerializeField, Min(0.01f)] private float zombieWalkDuration = 2.8f;
    [SerializeField, Min(0.01f)] private float zombieTurnToCameraDuration = 0.55f;
    [SerializeField, Min(0f)] private float revealPause = 0.25f;
    [SerializeField, Min(0f)] private float pauseBeforeScream = 0.5f;
    [SerializeField, Min(0f)] private float pauseAfterScream = 0.35f;
    [SerializeField, Min(0.01f)] private float fallbackScreamDuration = 2.5f;

    [Header("Ending UI")]
    [SerializeField] private CanvasGroup endingFadeCanvasGroup;
    [SerializeField] private TextMeshProUGUI toBeContinuedText;
    [SerializeField, Min(0f)] private float escapedPanelVisibleDuration = 5f;
    [SerializeField, Min(0.01f)] private float fadeToBlackDuration = 0.7f;
    [SerializeField, Min(0.01f)] private float continuationFadeInDuration = 0.7f;
    [SerializeField, Min(0f)] private float continuationHoldDuration = 3f;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Gameplay Input")]
[SerializeField] private GameObject gameplayUI;

    private bool hasBegun;
    private CanvasGroup continuationTextGroup;

    private void Awake()
    {
        if (endingCamera != null)
            endingCamera.gameObject.SetActive(false);

        if (endingFadeCanvasGroup != null)
        {
            endingFadeCanvasGroup.alpha = 0f;
            endingFadeCanvasGroup.blocksRaycasts = false;
        }

        SetupContinuationText();
    }

    public void Begin(Camera escapeCamera, CanvasGroup escapeCanvasGroup)
    {
        if (hasBegun)
            return;

        hasBegun = true;
        StartCoroutine(PlayEnding(escapeCamera, escapeCanvasGroup));
    }

    private IEnumerator PlayEnding(Camera escapeCamera, CanvasGroup escapeCanvasGroup)
    {
        // Time is explicitly restored so every cinematic step and UI fade advances.
        Time.timeScale = 1f;

        if (gameplayUI != null)
    {
        gameplayUI.SetActive(false);
    }

        // The escape panel must remain untouched and fully visible before the ending begins.
        yield return new WaitForSecondsRealtime(escapedPanelVisibleDuration);

        yield return StartCoroutine(FadeToBlack());
        HideEscapePanel(escapeCanvasGroup);
        yield return StartCoroutine(TransitionToEndingCamera(escapeCamera));
        PrepareZombie();
        yield return new WaitForSecondsRealtime(blackCameraSwitchDelay);
        yield return StartCoroutine(FadeFromBlack());

        yield return new WaitForSecondsRealtime(revealPause);
        yield return new WaitForSecondsRealtime(pauseBeforeScream);

        SetZombieAnimation(isWalking: false, isScreaming: true, moveSpeed: 0f);
        if (zombieAudioSource != null && zombieScreamClip != null)
            zombieAudioSource.PlayOneShot(zombieScreamClip);

        yield return StartCoroutine(FadeToBlack());

        yield return StartCoroutine(FadeInContinuationText());
        yield return new WaitForSecondsRealtime(continuationHoldDuration);

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void PrepareZombie()
    {
        if (zombie == null)
            return;

        // The gameplay zombie is intentionally inactive until its normal spawn event.
        // The ending uses that same zombie, so it must be explicitly enabled here.
        zombie.SetActive(true);

        if (zombieAI != null)
        {
            zombieAI.PrepareForEndingCutscene();
            // The cinematic owns movement and rotation until the scene changes.
            zombieAI.enabled = false;
        }

        if (zombieAnimator != null)
            zombieAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // This occurs while the screen is black, before the ending camera is revealed.
        if (zombieStartPoint != null)
        {
            zombie.transform.SetPositionAndRotation(zombieStartPoint.position, zombieStartPoint.rotation);
        }

        SetZombieAnimation(isWalking: false, isScreaming: false, moveSpeed: 0f);
    }

    private IEnumerator TransitionToEndingCamera(Camera escapeCamera)
{
    // Make absolutely sure gameplay controls stay disabled.
    if (gameplayUI != null)
    {
        gameplayUI.SetActive(false);
    }

    if (escapeCamera != null)
    {
        escapeCamera.gameObject.SetActive(false);
    }

    if (endingCamera != null)
    {
        endingCamera.gameObject.SetActive(true);
    }

    yield return null;

    // Safety check after camera switch.
    if (gameplayUI != null)
    {
        gameplayUI.SetActive(false);
    }
}

    private void HideEscapePanel(CanvasGroup escapeCanvasGroup)
    {
        if (escapeCanvasGroup == null)
            return;

        escapeCanvasGroup.alpha = 0f;
        escapeCanvasGroup.interactable = false;
        escapeCanvasGroup.blocksRaycasts = false;
        escapeCanvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator WalkZombieToStopPoint()
    {
        if (zombie == null || zombieStopPoint == null)
            yield break;

        Transform zombieTransform = zombie.transform;
        Vector3 startPosition = zombieTransform.position;
        Vector3 targetPosition = zombieStopPoint.position;
        Vector3 direction = targetPosition - startPosition;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.0001f)
            zombieTransform.rotation = Quaternion.LookRotation(direction);

        SetZombieAnimation(isWalking: true, isScreaming: false, moveSpeed: 0.5f);

        float elapsed = 0f;
        while (elapsed < zombieWalkDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Smooth01(elapsed / zombieWalkDuration);
            Vector3 nextPosition = Vector3.LerpUnclamped(startPosition, targetPosition, t);
            Vector3 movement = nextPosition - zombieTransform.position;
            movement.y = 0f;
            if (movement.sqrMagnitude > 0.000001f)
                zombieTransform.rotation = Quaternion.Slerp(zombieTransform.rotation, Quaternion.LookRotation(movement), 0.25f);

            zombieTransform.position = nextPosition;
            yield return null;
        }

        zombieTransform.position = targetPosition;
        SetZombieAnimation(isWalking: false, isScreaming: false, moveSpeed: 0f);
    }

    private IEnumerator TurnZombieToCamera()
    {
        if (zombie == null || endingCamera == null)
            yield break;

        Transform zombieTransform = zombie.transform;
        Vector3 direction = endingCamera.transform.position - zombieTransform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            yield break;

        Quaternion fromRotation = zombieTransform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float elapsed = 0f;
        while (elapsed < zombieTurnToCameraDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            zombieTransform.rotation = Quaternion.Slerp(fromRotation, targetRotation, Smooth01(elapsed / zombieTurnToCameraDuration));
            yield return null;
        }

        zombieTransform.rotation = targetRotation;
    }

    private IEnumerator FadeToBlack()
    {
        if (endingFadeCanvasGroup == null)
            yield break;

        endingFadeCanvasGroup.gameObject.SetActive(true);
        endingFadeCanvasGroup.blocksRaycasts = true;
        float elapsed = 0f;
        while (elapsed < fadeToBlackDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            endingFadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeToBlackDuration);
            yield return null;
        }

        endingFadeCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeFromBlack()
    {
        if (endingFadeCanvasGroup == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < fadeIntoEndingCameraDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            endingFadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeIntoEndingCameraDuration);
            yield return null;
        }

        endingFadeCanvasGroup.alpha = 0f;
        endingFadeCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeInContinuationText()
    {
        if (continuationTextGroup == null)
            yield break;

        continuationTextGroup.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < continuationFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            continuationTextGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / continuationFadeInDuration);
            yield return null;
        }

        continuationTextGroup.alpha = 1f;
    }

    private void SetupContinuationText()
    {
        if (toBeContinuedText == null && endingFadeCanvasGroup != null)
        {
            GameObject textObject = new GameObject("To Be Continued", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(endingFadeCanvasGroup.transform, false);
            toBeContinuedText = textObject.GetComponent<TextMeshProUGUI>();
            RectTransform rect = toBeContinuedText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1200f, 140f);
            toBeContinuedText.font = TMP_Settings.defaultFontAsset;
            toBeContinuedText.fontSize = 54f;
            toBeContinuedText.alignment = TextAlignmentOptions.Center;
            toBeContinuedText.color = Color.white;
        }

        if (toBeContinuedText == null)
            return;

        toBeContinuedText.text = "TO BE CONTINUED...";
        continuationTextGroup = toBeContinuedText.GetComponent<CanvasGroup>();
        if (continuationTextGroup == null)
            continuationTextGroup = toBeContinuedText.gameObject.AddComponent<CanvasGroup>();
        continuationTextGroup.alpha = 0f;
        continuationTextGroup.blocksRaycasts = false;
        continuationTextGroup.interactable = false;
        continuationTextGroup.gameObject.SetActive(false);
    }

    private void SetZombieAnimation(bool isWalking, bool isScreaming, float moveSpeed)
    {
        if (zombieAnimator == null)
            return;

        zombieAnimator.SetBool("IsWalking", isWalking);
        zombieAnimator.SetBool("IsChasing", false);
        zombieAnimator.SetBool("IsScreaming", isScreaming);
        zombieAnimator.SetFloat("MoveSpeed", moveSpeed);
    }

    private static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }
}
