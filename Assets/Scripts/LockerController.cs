using UnityEngine;
using NavKeypad;

public class LockerController : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Transform lockerDoor;
    public GameObject leverHandle;
    public GameObject keypad;
    public KeypadInteractionController keypadInteractionController;
    public Keypad keypadComponent;

    [Header("Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool unlocked = false;
    private bool opening = false;

    void Start()
    {
        closedRotation = lockerDoor.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        if (leverHandle != null)
            leverHandle.SetActive(false);

        if (keypad != null)
            keypad.SetActive(true);

        if (keypadInteractionController == null)
            keypadInteractionController = KeypadInteractionController.Instance;

        if (keypadComponent == null && keypad != null)
            keypadComponent = keypad.GetComponentInChildren<Keypad>(true);

        if (keypadComponent != null)
            keypadComponent.OnAccessGranted.AddListener(OpenLocker);
    }

    public void Interact()
    {
        if (keypadInteractionController == null)
            keypadInteractionController = KeypadInteractionController.Instance;

        if (keypadInteractionController != null && keypadInteractionController.IsInKeypadMode())
        {
            keypadInteractionController.ExitKeypad();
            return;
        }

        if (unlocked)
            return;

        if (keypadInteractionController != null)
            keypadInteractionController.EnterKeypad();
    }

    public void OpenLocker()
    {
        if (unlocked)
            return;

        unlocked = true;

        if (leverHandle != null)
            leverHandle.SetActive(true);

        if (keypad != null)
            keypad.SetActive(true);

        if (keypadInteractionController == null)
            keypadInteractionController = KeypadInteractionController.Instance;

        if (keypadInteractionController != null && keypadInteractionController.IsInKeypadMode())
            keypadInteractionController.ExitKeypad();

        opening = true;
    }

    void Update()
    {
        if (!opening)
            return;

        lockerDoor.localRotation = Quaternion.Slerp(
            lockerDoor.localRotation,
            openRotation,
            Time.deltaTime * openSpeed);

        if (Quaternion.Angle(lockerDoor.localRotation, openRotation) < 0.5f)
        {
            lockerDoor.localRotation = openRotation;
            opening = false;
        }
    }
}
