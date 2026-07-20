using UnityEngine;

namespace NavKeypad
{
    public class KeypadInteractionMobile : MonoBehaviour
    {
        private Camera cam;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null)
                cam = Camera.main;

#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                HandlePress(Input.GetTouch(0).position);
            }
#else
            if (Input.GetMouseButtonDown(0))
            {
                HandlePress(Input.mousePosition);
            }
#endif
        }

        private void HandlePress(Vector2 screenPos)
{
    if (KeypadInteractionController.Instance == null)
        return;

    if (!KeypadInteractionController.Instance.CanUseKeypad())
        return;

    Ray ray = cam.ScreenPointToRay(screenPos);

    if (Physics.Raycast(ray, out RaycastHit hit, 10f))
    {
        KeypadButton button = hit.collider.GetComponent<KeypadButton>();

        if (button == null)
            button = hit.collider.GetComponentInParent<KeypadButton>();

        if (button == null)
            button = hit.collider.GetComponentInChildren<KeypadButton>();

        if (button != null)
        {
            button.PressButton();
        }
    }
}
            }
        }
    