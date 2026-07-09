using UnityEngine;

public class BoatFloat : MonoBehaviour
{
    [Header("Floating")]
    public float floatHeight = 0.15f;
    public float floatSpeed = 1.2f;

    [Header("Rocking")]
    public float rockAngleX = 2f;
    public float rockAngleZ = 3f;
    public float rockSpeed = 0.8f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        Vector3 currentPosition = transform.position;
        currentPosition.y = startPosition.y + yOffset;
        transform.position = currentPosition;

        float xRock = Mathf.Sin(Time.time * rockSpeed) * rockAngleX;
        float zRock = Mathf.Cos(Time.time * rockSpeed * 0.8f) * rockAngleZ;

        transform.rotation = startRotation * Quaternion.Euler(xRock, 0f, zRock);
    }
}