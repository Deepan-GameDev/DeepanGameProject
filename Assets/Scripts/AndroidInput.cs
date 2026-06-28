using UnityEngine;
using PinePie.SimpleJoystick;

public class AndroidInput : MonoBehaviour
{
    public Player player;
    public JoystickController joystick;

   void Update()
    {
        player.SetMoveInput(joystick.InputDirection);
    }

    public void RunDown()
    {
        player.SetRun(true);
    }

    public void RunUp()
    {
        player.SetRun(false);
    }

    public void CrouchDown()
    {
        player.SetCrouch(true);
    }

    public void CrouchUp()
    {
        player.SetCrouch(false);
    }
}