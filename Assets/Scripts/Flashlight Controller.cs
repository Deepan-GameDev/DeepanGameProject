using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public GameObject flashlight;

    [Header("Battery Settings")]
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float batteryDrainRate = 2f;

    private bool isOn = false;
    private bool hasFlashlight = false;

    void Start()
    {
        flashlight.SetActive(false);
        currentBattery = maxBattery;
    }

    void Update()
    {
        if (hasFlashlight && isOn && currentBattery > 0)
        {
            currentBattery -= batteryDrainRate * Time.deltaTime;

            if (currentBattery <= 0)
            {
                currentBattery = 0;
                isOn = false;
                flashlight.SetActive(false);
            }
        }
    }

    public void EnableFlashlight()
    {
        hasFlashlight = true;

        if (currentBattery > 0)
        {
            isOn = true;
            flashlight.SetActive(true);
        }
    }

    public void ToggleFlashlight()
    {
        if (!hasFlashlight)
            return;

        if (currentBattery <= 0)
            return;

        isOn = !isOn;
        flashlight.SetActive(isOn);
    }
}