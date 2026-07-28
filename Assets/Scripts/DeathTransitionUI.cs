using System.Collections;
using TMPro;
using UnityEngine;

public class DeathTransitionUI : MonoBehaviour
{
    public static DeathTransitionUI Instance;

    [Header("References")]
    public CanvasGroup blackScreen;
    public TextMeshProUGUI messageText;

    private void Awake()
    {
        Instance = this;

        blackScreen.alpha = 0;
        gameObject.SetActive(false);

        if (messageText != null)
            messageText.alpha = 0;
    }

    public IEnumerator PlaySecondChance()
    {
        gameObject.SetActive(true);

        // Fade Black
        while (blackScreen.alpha < 1)
        {
            blackScreen.alpha += Time.deltaTime;
            yield return null;
        }

        blackScreen.alpha = 1;

        yield return new WaitForSeconds(4f);

        // Show Text
        if (messageText != null)
        {
            while (messageText.alpha < 1)
            {
                messageText.alpha += Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(2f);

        // Hide Text
        if (messageText != null)
        {
            while (messageText.alpha > 0)
            {
                messageText.alpha -= Time.deltaTime;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1f);

        // Fade Out Black
        while (blackScreen.alpha > 0)
        {
            blackScreen.alpha -= Time.deltaTime;
            yield return null;
        }

        blackScreen.alpha = 0;

        gameObject.SetActive(false);
    }
}