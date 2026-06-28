using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLook : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public Player player;
    public Transform cameraTransform;

    public float sensitivity = 0.2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    private float pitch = 0f;

    void Start()
    {
        pitch = cameraTransform.localEulerAngles.x;

        if (pitch > 180f)
            pitch -= 360f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta;

        // Horizontal look
        player.AddYawInput(delta.x * sensitivity);

        // Vertical look
        pitch -= delta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}