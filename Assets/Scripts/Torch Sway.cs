using UnityEngine;

public class TorchSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.08f;
    public float maxSwayAmount = 0.15f;
    public float smoothSpeed = 8f;
    public float returnSpeed = 6f;

    private Vector3 initialPosition;
    private Vector2 swayInput;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        Vector3 swayOffset = new Vector3(
            -swayInput.x * swayAmount,
            -swayInput.y * swayAmount,
            0f
        );

        swayOffset.x = Mathf.Clamp(
            swayOffset.x,
            -maxSwayAmount,
            maxSwayAmount
        );

        swayOffset.y = Mathf.Clamp(
            swayOffset.y,
            -maxSwayAmount,
            maxSwayAmount
        );

        Vector3 targetPosition = initialPosition + swayOffset;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        swayInput = Vector2.Lerp(
            swayInput,
            Vector2.zero,
            returnSpeed * Time.deltaTime
        );
    }

    public void SetSwayInput(Vector2 input)
    {
        swayInput = input;
    }
}