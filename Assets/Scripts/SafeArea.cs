using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private void OnEnable()
    {
        SafeAreaManager.Register(this);
    }

    private void OnDisable()
    {
        SafeAreaManager.Unregister(this);
    }

    internal void Apply(Rect safeArea)
    {
        RectTransform panel = (RectTransform)transform;
        float width = Mathf.Max(Screen.width, 1);
        float height = Mathf.Max(Screen.height, 1);

        panel.anchorMin = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
        panel.anchorMax = new Vector2(safeArea.xMax / width, safeArea.yMax / height);
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
    }
}
