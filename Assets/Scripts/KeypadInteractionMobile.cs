using UnityEngine;

namespace NavKeypad
{
    public class KeypadInteractionMobile : MonoBehaviour
    {
        private Camera cam;

        private void Start()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null)
                cam = Camera.main;

#if UNITY_EDITOR || UNITY_STANDALONE

            if (Input.GetMouseButtonDown(0))
            {
                PressButton(Input.mousePosition);
            }

#endif

#if UNITY_ANDROID || UNITY_IOS

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    PressButton(touch.position);
                }
            }

#endif
        }

        private void PressButton(Vector2 screenPosition)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                KeypadButton button = hit.collider.GetComponent<KeypadButton>();

                if (button != null)
                {
                    button.PressButton();
                }
            }
        }
    }
}