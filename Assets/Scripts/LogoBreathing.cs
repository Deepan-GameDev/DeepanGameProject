using UnityEngine;

public class LogoBreathing : MonoBehaviour
{
    [Header("Scale")]
    public float minScale = 1f;
    public float maxScale = 1.03f;

    [Header("Speed")]
    public float speed = 0.5f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float scale = Mathf.Lerp(minScale, maxScale,
            (Mathf.Sin(Time.time * speed * Mathf.PI * 2f) + 1f) * 0.5f);

        transform.localScale = originalScale * scale;
    }
}