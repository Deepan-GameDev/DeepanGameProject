using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");


        Vector3 move = new Vector3(horizontal, 0, vertical);
        transform.Translate(move * walkSpeed * Time.deltaTime);
    }
}