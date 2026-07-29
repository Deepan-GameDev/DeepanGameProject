using System.Collections;
using TMPro;
using UnityEngine;

public class DeathTransitionUI : MonoBehaviour
{
    public static DeathTransitionUI Instance;

    [Header("References")]
    public CanvasGroup blackScreen;
    public TextMeshProUGUI messageText;

    [Header("Respawn Messages")]
    [TextArea(2, 4)]
    public string[] respawnMessages =
    {
        "WAKE UP...\nIT IS NOT OVER YET",
        "ONE LAST CHANCE...\nDON'T GIVE UP"
    };

    private void Awake()
    {
        Instance = this;

        blackScreen.alpha = 0;

        if (messageText != null)
            messageText.alpha = 0;
    }

    public IEnumerator PlaySecondChance()
    {
        yield return StartCoroutine(PlaySecondChance(2));
    }

    public IEnumerator PlaySecondChance(int chanceNumber)
    {
        // DeathScreen starts inactive in the scene, so make it visible before the first fade.
        gameObject.SetActive(true);
        blackScreen.alpha = 0f;

        if (messageText != null)
        {
            messageText.text = GetRespawnMessage(chanceNumber);
            messageText.alpha = 0f;
        }

        // Fade Black
        while (blackScreen.alpha < 1)
        {
            blackScreen.alpha += Time.deltaTime;
            yield return null;
        }

        blackScreen.alpha = 1;

        yield return new WaitForSeconds(8f);

        // Show Text
        if (messageText != null)
        {
            while (messageText.alpha < 1)
            {
                messageText.alpha += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(7f);

        // Hide Text
        if (messageText != null)
        {
            while (messageText.alpha > 0)
            {
                messageText.alpha -= Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(4f);
    }

    public IEnumerator FadeBackToGameplay()
    {
        // Fade Out Black
        while (blackScreen.alpha > 0)
        {
            blackScreen.alpha -= Time.deltaTime;
            yield return null;
        }

        blackScreen.alpha = 0;

        if (messageText != null)
            messageText.alpha = 0f;

        // Return the overlay to its initial inactive state so every death starts identically.
        gameObject.SetActive(false);
    }

    private string GetRespawnMessage(int chanceNumber)
    {
        if (respawnMessages == null || respawnMessages.Length == 0)
        {
            return string.Empty;
        }

        int messageIndex = Mathf.Clamp(chanceNumber - 2, 0, respawnMessages.Length - 1);
        return respawnMessages[messageIndex];
    }
}
