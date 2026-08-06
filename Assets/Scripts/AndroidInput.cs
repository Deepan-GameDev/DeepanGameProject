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

    public void RunButton()
    {
        player.ToggleRun();
    }

public void ToggleCrouch()
{
    player.ToggleCrouch();
}
}