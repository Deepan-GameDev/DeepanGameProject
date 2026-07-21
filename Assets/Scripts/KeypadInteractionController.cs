using UnityEngine;

public class KeypadInteractionController : MonoBehaviour
{
    public static KeypadInteractionController Instance;

    [Header("References")]
    public Player player;
    public Transform playerCamera;
    public Transform keypadCameraPoint;
    public Transform defaultView;

    [Header("Settings")]
    public float moveSpeed = 8f;

    private bool inKeypadMode;
    private bool moving;
    private bool returningToGameplay;
    private bool keypadInputEnabled;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Transform originalCameraParent;
    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;

    private void Awake()
    {
        Instance = this;

        if (GetComponent<NavKeypad.KeypadInteractionMobile>() == null)
            gameObject.AddComponent<NavKeypad.KeypadInteractionMobile>();
    }

    private void LateUpdate()
    {
        if (!moving)
            return;

        playerCamera.position = Vector3.Lerp(
            playerCamera.position,
            targetPosition,
            Time.deltaTime * moveSpeed);

        playerCamera.rotation = Quaternion.Slerp(
            playerCamera.rotation,
            targetRotation,
            Time.deltaTime * moveSpeed);

        if (Vector3.Distance(playerCamera.position, targetPosition) < 0.01f &&
            Quaternion.Angle(playerCamera.rotation, targetRotation) < 0.5f)
        {
            playerCamera.position = targetPosition;
            playerCamera.rotation = targetRotation;
            moving = false;

            if (returningToGameplay)
                FinishExitKeypad();
            else
                keypadInputEnabled = true;
        }
    }

    public void EnterKeypad()
    {
        if (inKeypadMode)
            return;

        if (player == null || playerCamera == null || keypadCameraPoint == null)
        {
            Debug.LogWarning("KeypadInteractionController is missing a required reference.");
            return;
        }

        inKeypadMode = true;
        returningToGameplay = false;
        keypadInputEnabled = false;

        originalCameraParent = playerCamera.parent;
        originalCameraLocalPosition = playerCamera.localPosition;
        originalCameraLocalRotation = playerCamera.localRotation;

        playerCamera.SetParent(null, true);
        player.BeginExternalMovement();
        targetPosition = keypadCameraPoint.position;
        targetRotation = keypadCameraPoint.rotation;
        moving = true;
    }

    public void ExitKeypad()
    {
        if (!inKeypadMode)
            return;

        inKeypadMode = false;
        returningToGameplay = true;
        keypadInputEnabled = false;

        if (defaultView != null)
        {
            targetPosition = defaultView.position;
            targetRotation = defaultView.rotation;
        }
        else if (originalCameraParent != null)
        {
            targetPosition = originalCameraParent.TransformPoint(originalCameraLocalPosition);
            targetRotation = originalCameraParent.rotation * originalCameraLocalRotation;
        }
        else
        {
            targetPosition = originalCameraLocalPosition;
            targetRotation = originalCameraLocalRotation;
        }

        moving = true;
    }

    private void FinishExitKeypad()
    {
        returningToGameplay = false;

        if (originalCameraParent != null)
        {
            playerCamera.SetParent(originalCameraParent, true);
            playerCamera.localPosition = originalCameraLocalPosition;
            playerCamera.localRotation = originalCameraLocalRotation;
        }

        if (player != null)
            player.EndExternalMovement();
    }

    public bool IsInKeypadMode()
    {
        return inKeypadMode;
    }

    public bool CanUseKeypad()
    {
        return inKeypadMode && keypadInputEnabled;
    }

    public Camera GetKeypadCamera()
    {
        if (playerCamera == null)
            return null;

        Camera keypadCamera = playerCamera.GetComponent<Camera>();
        if (keypadCamera != null)
            return keypadCamera;

        keypadCamera = playerCamera.GetComponentInChildren<Camera>();
        if (keypadCamera != null)
            return keypadCamera;

        keypadCamera = playerCamera.GetComponentInParent<Camera>();
        if (keypadCamera != null)
            return keypadCamera;

        return Camera.main;
    }
}
