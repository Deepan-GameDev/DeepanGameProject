using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightController : MonoBehaviour
{
    public GameObject flashlight;

    [Header("Battery Settings")]
    public float maxBattery = 100f;
    public float currentBattery = 100f;
    public float batteryDrainRate = 2f;

    [Header("Battery UI")]
    public Slider batterySlider;
    public TMP_Text batteryText;

    private bool isOn = false;
    private bool hasFlashlight = false;

    void Start()
    {
        flashlight.SetActive(false);

        currentBattery = maxBattery;

        batterySlider.maxValue = maxBattery;

        UpdateBatteryUI();
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

            UpdateBatteryUI();
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

        UpdateBatteryUI();
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

    void UpdateBatteryUI()
    {
        if (batterySlider != null)
        {
            batterySlider.value = currentBattery;
        }

        if (batteryText != null)
        {
            batteryText.text = Mathf.CeilToInt(currentBattery) + "%";
        }
    }
}