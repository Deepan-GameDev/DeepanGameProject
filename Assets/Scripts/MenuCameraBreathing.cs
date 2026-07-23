using UnityEngine;

public class MenuCameraBreathing : MonoBehaviour
{
    public float moveAmount = 8f;
    public float speed = 0.15f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * speed) * moveAmount;
        float y = Mathf.Cos(Time.time * speed * 0.7f) * (moveAmount * 0.5f);

        transform.localPosition = startPos + new Vector3(x, y, 0f);
    }
}