using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace NavKeypad
{
    public class KeypadInteractionMobile : MonoBehaviour
    {
        [SerializeField] private float raycastDistance = 25f;

        private Camera cam;

        private void Update()
        {
            global::KeypadInteractionController controller = global::KeypadInteractionController.Instance;
            if (controller == null || !controller.CanUseKeypad())
                return;

            cam = controller.GetKeypadCamera();
            if (cam == null)
                return;

            if (Touchscreen.current != null)
            {
                foreach (TouchControl touch in Touchscreen.current.touches)
                {
                    if (touch.press.wasPressedThisFrame)
                    {
                        HandlePress(touch.position.ReadValue());
                        return;
                    }
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
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
                raycastDistance,
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
