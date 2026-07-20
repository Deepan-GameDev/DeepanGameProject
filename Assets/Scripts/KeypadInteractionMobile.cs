using UnityEngine;
using UnityEngine.InputSystem;

namespace NavKeypad
{
    public class KeypadInteractionMobile : MonoBehaviour
    {
        private Camera cam;

        private void Update()
        {
            KeypadInteractionController controller = KeypadInteractionController.Instance;
            if (controller == null || !controller.CanUseKeypad())
                return;

            cam = controller.GetKeypadCamera();
            if (cam == null)
                return;

#if UNITY_ANDROID || UNITY_IOS

            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;

                if (touch.press.wasPressedThisFrame)
                {
                    HandlePress(touch.position.ReadValue());
                }
            }

#else

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandlePress(Mouse.current.position.ReadValue());
            }

#endif
        }

        private void HandlePress(Vector2 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);

            RaycastHit[] hits = Physics.RaycastAll(
                ray,
                10f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);

            if (hits.Length == 0)
                return;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                KeypadButton button = hit.collider.GetComponent<KeypadButton>();

                if (button == null)
                    button = hit.collider.GetComponentInParent<KeypadButton>();

                if (button == null)
                    button = hit.collider.GetComponentInChildren<KeypadButton>();

                if (button != null)
                {
                    button.PressButton();
                    return;
                }
            }
        }
    }
}