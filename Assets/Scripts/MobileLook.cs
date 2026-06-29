using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLook : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Player player;
    public Transform cameraTransform;

    public float sensitivity = 0.2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private float pitch = 0f;
    private int activePointerId = int.MinValue;

    void Start()
    {
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
        if (eventData.pointerId != activePointerId || player == null || cameraTransform == null)
        {
            return;
        }

        Vector2 delta = eventData.delta;

        // Horizontal look
        player.AddYawInput(delta.x * sensitivity);

        // Vertical look
        pitch -= delta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
