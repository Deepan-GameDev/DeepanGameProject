using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public GameObject flashlight;

    private bool isOn = true;

    void Start()
    {
        flashlight.SetActive(false); // Start with no flashlight
    }

    public void EnableFlashlight()
    {
        isOn = true;
        flashlight.SetActive(true);
    }

    public void ToggleFlashlight()
    {
        isOn = !isOn;
        flashlight.SetActive(isOn);
    }
}