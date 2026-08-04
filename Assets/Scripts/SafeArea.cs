using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panel;

    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;
    private Vector2Int lastResolution = Vector2Int.zero;

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void OnEnable()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        if (Screen.safeArea != lastSafeArea ||
            Screen.orientation != lastOrientation ||
            lastResolution.x != Screen.width ||
            lastResolution.y != Screen.height)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        lastSafeArea = Screen.safeArea;
        lastOrientation = Screen.orientation;
        lastResolution = new Vector2Int(Screen.width, Screen.height);

        Vector2 anchorMin = lastSafeArea.position;
        Vector2 anchorMax = lastSafeArea.position + lastSafeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;

        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;

        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
    }
}