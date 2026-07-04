using UnityEngine;
using TMPro;
using System.Collections;

public class GameMessageUI : MonoBehaviour
{
    public TMP_Text messageText;
    public float messageDuration = 2f;

    private Coroutine messageCoroutine;

    void Start()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(
            ShowMessageRoutine(message)
        );
    }

    IEnumerator ShowMessageRoutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        messageText.gameObject.SetActive(false);

        messageCoroutine = null;
    }
}