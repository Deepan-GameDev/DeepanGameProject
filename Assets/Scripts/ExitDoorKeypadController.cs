using UnityEngine;
using NavKeypad;

public class ExitDoorKeypadController : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Transform door;
    public Transform cameraPoint;
    public Keypad keypadComponent;
    public KeypadInteractionController keypadInteractionController;
    public GameMessageUI gameMessageUI;

    [Header("Door Settings")]
    public float openAngle = -90f;
    public float openSpeed = 2f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool unlocked;
    private bool opening;

    private void Start()
    {
        closedRotation = door.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        if (keypadInteractionController == null)
            keypadInteractionController = KeypadInteractionController.Instance;

        if (keypadComponent == null)
            keypadComponent = GetComponentInChildren<Keypad>(true);

        if (keypadComponent != null)
            keypadComponent.OnAccessGranted.AddListener(OpenDoor);
    }

    public void Interact()
    {
        if (keypadInteractionController == null)
            keypadInteractionController = KeypadInteractionController.Instance;

        if (keypadInteractionController != null &&
            keypadInteractionController.IsInKeypadMode())
        {
            keypadInteractionController.ExitKeypad();
            return;
        }

        if (unlocked)
            return;

            if (PowerManager.Instance != null && !PowerManager.Instance.PowerOn)
{
    if (gameMessageUI != null)
        gameMessageUI.ShowMessage("POWER REQUIRED");

    return;
}

        keypadInteractionController.EnterKeypad(cameraPoint);
    }

    public void OpenDoor()
    {
        if (unlocked)
            return;

        unlocked = true;

        if (keypadInteractionController != null &&
            keypadInteractionController.IsInKeypadMode())
        {
            keypadInteractionController.ExitKeypad();
        }

        opening = true;
    }

    private void Update()
    {
        if (!opening)
            return;

        door.localRotation = Quaternion.Slerp(
            door.localRotation,
            openRotation,
            Time.deltaTime * openSpeed);

        if (Quaternion.Angle(door.localRotation, openRotation) < 0.5f)
        {
            door.localRotation = openRotation;
            opening = false;
        }
    }
}