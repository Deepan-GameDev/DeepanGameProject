using UnityEngine;
using UnityEngine.UI;

public class CenterDotController : MonoBehaviour
{
    public Image dotImage;

    [Header("Normal")]
    public float normalAlpha = 0.45f;
    public float normalScale = 1f;

    [Header("Interact")]
    public float interactAlpha = 1f;
    public float interactScale = 1.3f;

    public void SetInteracting(bool value)
    {
        Color c = dotImage.color;
        c.a = value ? interactAlpha : normalAlpha;
        dotImage.color = c;

        transform.localScale = value ?
            Vector3.one * interactScale :
            Vector3.one * normalScale;
    }
}