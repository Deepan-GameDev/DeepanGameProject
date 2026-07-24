using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLook : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Player player;
    public Transform cameraTransform;
    public TorchSway torchSway;

    [Header("Look Settings")]
public float defaultSensitivity = 0.2f;

private float sensitivity;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public float minLookDelta = 0.01f;
    public float maxLookDelta = 80f;

    private float pitch = 0f;
    private int activePointerId = int.MinValue;
    private bool lookLocked;

    public void SetLookLocked(bool locked)
    {
        lookLocked = locked;

        if (lookLocked)
        {
            activePointerId = int.MinValue;
        }
    }

    void Start()
    {
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);

    if (cameraTransform == null)
    {
        return;
    }
        if (cameraTransform == null)
        {
            return;
        }

        pitch = cameraTransform.localEulerAngles.x;

        if (pitch > 180f)
            pitch -= 360f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (lookLocked)
        {
            return;
        }

        activePointerId = eventData.pointerId;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == activePointerId)
        {
            activePointerId = int.MinValue;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (lookLocked || eventData.pointerId != activePointerId || player == null || cameraTransform == null)
        {
            return;
        }

        Vector2 delta = eventData.delta;
        if (delta.sqrMagnitude < minLookDelta * minLookDelta)
        {
            return;
        }

        delta = Vector2.ClampMagnitude(delta, maxLookDelta);
        
        if (torchSway != null)
       {
        torchSway.SetSwayInput(delta);
    }

        // Horizontal look
        player.AddYawInput(delta.x * sensitivity);

        // Vertical look
        pitch -= delta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
    public void SetSensitivity(float value)
{
    sensitivity = value;

    PlayerPrefs.SetFloat("Sensitivity", value);
    PlayerPrefs.Save();
}
}
