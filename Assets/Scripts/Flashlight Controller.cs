using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Low Battery Flicker")]
    public float lowBatteryLevel = 20f;
    public float minFlickerDelay = 0.05f;
    public float maxFlickerDelay = 0.2f;
    public float flickerInterval = 2f;

    private bool isOn = false;
    private bool hasFlashlight = false;
    private bool isFlickering = false;

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

            if (currentBattery <= lowBatteryLevel && !isFlickering)
            {
                StartCoroutine(FlickerFlashlight());
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

    public void AddBattery(float amount)
    {
        currentBattery += amount;

        if (currentBattery > maxBattery)
        {
            currentBattery = maxBattery;
        }

        UpdateBatteryUI();
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

    IEnumerator FlickerFlashlight()
    {
        isFlickering = true;

        flashlight.SetActive(false);

        yield return new WaitForSeconds(
            Random.Range(minFlickerDelay, maxFlickerDelay)
        );

        if (isOn && currentBattery > 0)
        {
            flashlight.SetActive(true);
        }

        yield return new WaitForSeconds(flickerInterval);

        isFlickering = false;
    }
}