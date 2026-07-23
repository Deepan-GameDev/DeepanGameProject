using UnityEngine;

public class FogMovement : MonoBehaviour
{
    public RectTransform fogRect;
    public float speed = 5f;

    Vector2 startPos;

    void Start()
    {
        startPos = fogRect.anchoredPosition;
    }

    void Update()
    {
        fogRect.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        if (fogRect.anchoredPosition.x > 200)
        {
            fogRect.anchoredPosition =
                new Vector2(-200, startPos.y);
        }

        if (fogRect.anchoredPosition.x < -200)
        {
            fogRect.anchoredPosition =
                new Vector2(200, startPos.y);
        }
    }
}