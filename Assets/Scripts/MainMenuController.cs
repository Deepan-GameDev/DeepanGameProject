using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("References")]
    public Image logo;
    public Image tapToContinue;

    public CanvasGroup logoGroup;
    public RectTransform logoRect;

[Header("Logo Position")]
public Vector2 menuLogoPosition;
    public CanvasGroup tapGroup;

    public GameObject menuPanel;
    public CanvasGroup menuGroup;
    public RectTransform menuRect;

    [Header("Menu Position")]
    public Vector2 hiddenPosition;
    public Vector2 visiblePosition;

    [Header("Audio")]
    public AudioSource voiceSource;

    [Header("Buttons")]
    public CanvasGroup newGameGroup;
    public CanvasGroup continueGroup;
    public CanvasGroup settingsGroup;
    public CanvasGroup exitGroup;

    public RectTransform newGameRect;
    public RectTransform continueRect;
    public RectTransform settingsRect;
    public RectTransform exitRect;

    private bool menuOpened = false;

    // Blink
    private bool isBlinking = true;
    private float blinkSpeed = 1.5f;

    void Start()
    {
        menuPanel.SetActive(false);
    }

    void Update()
    {
        // Tap To Continue Blink
        if (isBlinking)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            tapGroup.alpha = Mathf.Lerp(0.2f, 1f, alpha);
        }

        // Detect Mouse / Touch
        if (!menuOpened &&
            (
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            ))
        {
            menuOpened = true;
            isBlinking = false;

            // Play voice immediately
            if (voiceSource != null)
            {
                voiceSource.Play();
            }

            StartCoroutine(OpenMenu());
        }
    }

    IEnumerator OpenMenu()
    {
        // Stop blink
        tapGroup.alpha = 1f;

        // Small cinematic delay
        yield return new WaitForSeconds(0.15f);

        // Fade Tap To Continue
        while (tapGroup.alpha > 0)
        {
            tapGroup.alpha -= Time.deltaTime * 1.2f;
            yield return null;
        }

        tapGroup.alpha = 0f;

        // Fade Out Logo
while (logoGroup.alpha > 0)
{
    logoGroup.alpha -= Time.deltaTime * 1.2f;
    yield return null;
}

logoGroup.alpha = 0f;

// Move logo above menu
logoRect.anchoredPosition = menuLogoPosition;

yield return new WaitForSeconds(0.15f);

// Fade In Logo
while (logoGroup.alpha < 1)
{
    logoGroup.alpha += Time.deltaTime * 1.2f;
    yield return null;
}

logoGroup.alpha = 1f;

// Small Pause
yield return new WaitForSeconds(0.15f);

        // Show Menu Panel
        menuPanel.SetActive(true);

        menuGroup.alpha = 0f;
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;

        menuRect.anchoredPosition = hiddenPosition;

        while (Vector2.Distance(menuRect.anchoredPosition, visiblePosition) > 1f)
        {
            menuRect.anchoredPosition = Vector2.Lerp(
                menuRect.anchoredPosition,
                visiblePosition,
                Time.deltaTime * 3f);

            menuGroup.alpha += Time.deltaTime * 1.5f;

            yield return null;
        }

        menuRect.anchoredPosition = visiblePosition;

        menuGroup.alpha = 1f;
        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;

        // Show buttons one by one
        yield return StartCoroutine(ShowButton(newGameGroup, newGameRect));
        yield return new WaitForSeconds(0.08f);

        yield return StartCoroutine(ShowButton(continueGroup, continueRect));
        yield return new WaitForSeconds(0.08f);

        yield return StartCoroutine(ShowButton(settingsGroup, settingsRect));
        yield return new WaitForSeconds(0.08f);

        yield return StartCoroutine(ShowButton(exitGroup, exitRect));
    }

    IEnumerator ShowButton(CanvasGroup group, RectTransform rect)
    {
        group.alpha = 0f;

        Vector2 target = rect.anchoredPosition;
        rect.anchoredPosition = target + Vector2.left * 40f;

        while (group.alpha < 1f)
        {
            group.alpha += Time.deltaTime * 4f;

            rect.anchoredPosition = Vector2.Lerp(
                rect.anchoredPosition,
                target,
                Time.deltaTime * 10f);

            yield return null;
        }

        group.alpha = 1f;
        rect.anchoredPosition = target;

        group.interactable = true;
        group.blocksRaycasts = true;
    }
}