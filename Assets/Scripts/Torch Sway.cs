using UnityEngine;

public class TorchSway : MonoBehaviour
{
    [Header("References")]
    public Player player;

    [Header("Sway Settings")]
    public float swayAmount = 0.08f;
    public float maxSwayAmount = 0.15f;
    public float smoothSpeed = 8f;
    public float returnSpeed = 6f;

    [Header("Walk Bob Settings")]
    public float walkBobSpeed = 8f;
    public float walkBobAmountX = 0.015f;
    public float walkBobAmountY = 0.025f;

    [Header("Run Bob Settings")]
    public float runBobSpeed = 12f;
    public float runBobAmountX = 0.025f;
    public float runBobAmountY = 0.045f;

    [Header("Crouch Bob Settings")]
    public float crouchBobSpeed = 5f;
    public float crouchBobAmountX = 0.008f;
    public float crouchBobAmountY = 0.012f;

    [Header("Footstep Sync")]
    public float walkStepKick = 0.015f;
    public float runStepKick = 0.03f;
    public float stepReturnSpeed = 8f;

    private Vector3 initialPosition;
    private Vector2 swayInput;
    private float bobTimer;
    private float footstepKick;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        UpdateTorchMovement();
    }

    void UpdateTorchMovement()
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

        Vector3 bobOffset = Vector3.zero;

        if (player != null && player.GetIsMoving())
        {
            float bobSpeed;
            float bobAmountX;
            float bobAmountY;

            if (player.GetIsCrouching())
            {
                bobSpeed = crouchBobSpeed;
                bobAmountX = crouchBobAmountX;
                bobAmountY = crouchBobAmountY;
            }
            else if (player.GetIsRunning())
            {
                bobSpeed = runBobSpeed;
                bobAmountX = runBobAmountX;
                bobAmountY = runBobAmountY;
            }
            else
            {
                bobSpeed = walkBobSpeed;
                bobAmountX = walkBobAmountX;
                bobAmountY = walkBobAmountY;
            }

            bobTimer += Time.deltaTime * bobSpeed;

            bobOffset.x = Mathf.Cos(bobTimer * 0.5f) * bobAmountX;
            bobOffset.y = Mathf.Sin(bobTimer) * bobAmountY;
        }
        else
        {
            bobTimer = 0f;
        }

        footstepKick = Mathf.Lerp(
    footstepKick,
    0f,
    stepReturnSpeed * Time.deltaTime
);

Vector3 footstepOffset = new Vector3(
    0f,
    -footstepKick,
    0f
);

Vector3 targetPosition =
    initialPosition
    + swayOffset
    + bobOffset
    + footstepOffset;

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

    public void OnFootstep(bool isRunning)
{
    footstepKick = isRunning
        ? runStepKick
        : walkStepKick;
}
}