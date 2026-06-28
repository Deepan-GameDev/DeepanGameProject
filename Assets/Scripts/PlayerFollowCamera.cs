using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public bool invertY;

    private Player player;
    private float xRotation;

    void Start()
    {
       // Cursor.lockState = CursorLockMode.Locked;
       // Cursor.visible = false;

        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }

        if (playerBody != null)
        {
            player = playerBody.GetComponent<Player>();
        }
    }

    void Update()
    {
        if (playerBody == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation += invertY ? mouseY : -mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (player != null)
        {
            player.AddYawInput(mouseX);
        }
        else
        {
            playerBody.Rotate(Vector3.up * mouseX, Space.World);
        }
    }
}
