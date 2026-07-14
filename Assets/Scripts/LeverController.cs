using System.Collections;
using UnityEngine;

public class LeverController : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Transform leverHandle;

    [Header("Animation")]
    public float rotateAngle = -90f;
    public float rotateSpeed = 4f;

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
        Quaternion startRotation = leverHandle.localRotation;
        Quaternion targetRotation = Quaternion.Euler(rotateAngle, 0f, 0f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed;

            leverHandle.localRotation =
                Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        leverHandle.localRotation = targetRotation;

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