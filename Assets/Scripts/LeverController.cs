using System.Collections;
using UnityEngine;

public class LeverController : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Transform leverHandle;
    public PlayerInventory playerInventory;
    public GameMessageUI gameMessageUI;

    [Tooltip("Handle inside the lever box")]
    public GameObject leverHandleObject;

    [Header("Movement")]
    public float moveDistance = 0.08f;
    public float moveSpeed = 4f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip leverSound;
    public AudioClip attachSound;
    public Transform handleSocket;
    public float attachSpeed = 5f;

    private bool isAttaching;

    private bool handleAttached = false;
    private bool activated = false;
    private Vector3 originalHandleLocalPosition;

    private void Start()
    {
        originalHandleLocalPosition = leverHandle.localPosition;
        if (leverHandleObject != null)
            leverHandleObject.SetActive(false);
    }

    public void Interact()
{
    if (activated || isAttaching)
        return;

    if (!handleAttached)
    {
        if (!playerInventory.HasLeverHandle())
        {
            gameMessageUI.ShowMessage("LEVER HANDLE REQUIRED");
            return;
        }

        AttachHandle();
        return;
    }

    ActivateLever();
}

    private void AttachHandle()
{
    StartCoroutine(AttachRoutine());
}

private IEnumerator AttachRoutine()
{
    isAttaching = true;

    playerInventory.UseLeverHandle();

    leverHandleObject.SetActive(true);

    while (Vector3.Distance(
        leverHandleObject.transform.position,
        handleSocket.position) > 0.01f)
    {
        leverHandleObject.transform.position =
            Vector3.MoveTowards(
                leverHandleObject.transform.position,
                handleSocket.position,
                attachSpeed * Time.deltaTime);

        leverHandleObject.transform.rotation =
            Quaternion.RotateTowards(
                leverHandleObject.transform.rotation,
                handleSocket.rotation,
                360f * Time.deltaTime);

        yield return null;
    }

    leverHandleObject.transform.SetParent(transform);
    leverHandleObject.transform.localRotation = Quaternion.identity;
    leverHandleObject.transform.position = handleSocket.position;
    leverHandleObject.transform.rotation = handleSocket.rotation;

    handleAttached = true;
    isAttaching = false;

    if (audioSource != null && attachSound != null)
        audioSource.PlayOneShot(attachSound);

    if (gameMessageUI != null)
        gameMessageUI.ShowMessage("HANDLE ATTACHED");
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
        Vector3 startPos = originalHandleLocalPosition;

        Vector3 targetPos =
        originalHandleLocalPosition + new Vector3(0f, 0f, moveDistance);

        leverHandle.localPosition = startPos;

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
            audioSource.PlayOneShot(leverSound);

        if (PowerManager.Instance != null)
            PowerManager.Instance.RestorePower();

        Debug.Log("POWER RESTORED");
    }
}