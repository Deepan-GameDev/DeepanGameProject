using System.Collections;
using UnityEngine;

public class LeverController : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Transform leverHandle;
    public Transform onPosition;

    [Header("Animation")]
    public float moveSpeed = 3f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip leverSound;

    private bool activated = false;

    public void Interact()
    {
        ActivateLever();
    }

    public void ActivateLever()
    {
        if (activated)
            return;

        activated = true;

        StartCoroutine(LeverRoutine());
    }

    private IEnumerator LeverRoutine()
    {
        Vector3 startPos = leverHandle.localPosition;
        Vector3 targetPos = onPosition.localPosition;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            leverHandle.localPosition =
                Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        leverHandle.localPosition = targetPos;

        if (audioSource != null && leverSound != null)
        {
            audioSource.PlayOneShot(leverSound);
        }

        if (PowerManager.Instance != null)
        {
            PowerManager.Instance.RestorePower();
        }

        Debug.Log("Lever Activated");
    }
}