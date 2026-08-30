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
    public GameObject batteryUI;

    [Header("Recharge Rewarded Ad")]
    public GameObject rechargeButton;

    // One recharge chance only for this game session.
    private bool rechargeUsed = false;

    [Header("Low Battery UI")]
    public GameObject lowBatteryText;
    public float warningBlinkSpeed = 0.5f;

    private Coroutine warningCoroutine;

    [Header("Low Battery Flicker")]
    public float lowBatteryLevel = 20f;
    public float minFlickerDelay = 0.05f;
    public float maxFlickerDelay = 0.2f;
    public float flickerInterval = 2f;
    public int minFlickerCount = 2;
    public int maxFlickerCount = 5;

    [Header("Flicker Audio")]
    public AudioSource flickerAudioSource;
    public AudioClip flickerSound;

    [Header("Flashlight Toggle Audio")]
    public AudioSource toggleAudioSource;
    public AudioClip flashlightOnSound;
    public AudioClip flashlightOffSound;

    private bool isOn = false;
    private bool hasFlashlight = false;
    private bool isFlickering = false;
    private int displayedBatteryPercent = int.MinValue;

    void Start()
    {
        flashlight.SetActive(false);

        currentBattery = maxBattery;

        if (batterySlider != null)
        {
            batterySlider.maxValue = maxBattery;
        }

        if (batteryUI != null)
        {
            batteryUI.SetActive(false);
        }

        // Recharge button must be hidden at the beginning.
        if (rechargeButton != null)
        {
            rechargeButton.SetActive(false);
        }

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

                if (flashlight != null)
                {
                    flashlight.SetActive(false);
                }

                UpdateBatteryUI();

                // Show recharge button only when battery reaches 0.
                ShowRechargeButton();
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

        if (batteryUI != null)
        {
            batteryUI.SetActive(true);
        }

        if (currentBattery > 0)
        {
            isOn = true;

            if (flashlight != null)
            {
                flashlight.SetActive(true);
            }
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

        if (flashlight != null)
        {
            flashlight.SetActive(isOn);
        }

        if (toggleAudioSource != null)
        {
            if (isOn && flashlightOnSound != null)
            {
                toggleAudioSource.PlayOneShot(flashlightOnSound);
            }
            else if (!isOn && flashlightOffSound != null)
            {
                toggleAudioSource.PlayOneShot(flashlightOffSound);
            }
        }
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

    // ============================================================
    // RECHARGE REWARDED AD
    // ============================================================

    private void ShowRechargeButton()
    {
        if (rechargeButton == null)
            return;

        // Never show again after the one chance has been used.
        if (rechargeUsed)
            return;

        rechargeButton.SetActive(true);
    }

    // Called after the rewarded ad is successfully completed.
    public void RechargeBatteryFromReward()
    {
        if (rechargeUsed)
        {
            Debug.LogWarning("[Flashlight] Recharge reward already used.");
            return;
        }

        rechargeUsed = true;

        // Give full battery.
        currentBattery = maxBattery;

        // Turn flashlight back on if player has already collected it.
        if (hasFlashlight)
        {
            isOn = true;

            if (flashlight != null)
            {
                flashlight.SetActive(true);
            }
        }

        // Hide recharge button permanently for this game session.
        if (rechargeButton != null)
        {
            rechargeButton.SetActive(false);
        }

        UpdateBatteryUI();

        Debug.Log("[Flashlight] Recharge reward applied. Battery restored to 100%.");
    }

    // ============================================================
    // BATTERY UI
    // ============================================================

    void UpdateBatteryUI()
    {
        if (batterySlider != null)
        {
            batterySlider.value = currentBattery;
        }

        if (batteryText != null)
        {
            int batteryPercent = Mathf.CeilToInt(currentBattery);

            if (displayedBatteryPercent != batteryPercent)
            {
                batteryText.text = batteryPercent + "%";
                displayedBatteryPercent = batteryPercent;
            }
        }

        UpdateLowBatteryWarning();
    }

    // ============================================================
    // FLASHLIGHT FLICKER
    // ============================================================

    IEnumerator FlickerFlashlight()
    {
        isFlickering = true;

        int flickerCount = Random.Range(
            minFlickerCount,
            maxFlickerCount + 1
        );

        if (flickerAudioSource != null && flickerSound != null)
        {
            flickerAudioSource.PlayOneShot(flickerSound);
        }

        for (int i = 0; i < flickerCount; i++)
        {
            if (!isOn || currentBattery <= 0)
            {
                if (flashlight != null)
                {
                    flashlight.SetActive(false);
                }

                break;
            }

            flashlight.SetActive(false);

            yield return new WaitForSeconds(
                Random.Range(minFlickerDelay, maxFlickerDelay)
            );

            if (!isOn || currentBattery <= 0)
            {
                flashlight.SetActive(false);
                break;
            }

            flashlight.SetActive(true);

            yield return new WaitForSeconds(
                Random.Range(minFlickerDelay, maxFlickerDelay)
            );
        }

        if (isOn && currentBattery > 0)
        {
            flashlight.SetActive(true);
        }

        yield return new WaitForSeconds(flickerInterval);

        isFlickering = false;
    }

    // ============================================================
    // LOW BATTERY WARNING
    // ============================================================

    IEnumerator BlinkLowBatteryWarning()
    {
        while (true)
        {
            lowBatteryText.SetActive(true);

            yield return new WaitForSeconds(warningBlinkSpeed);

            if (lowBatteryText.activeSelf)
            {
                lowBatteryText.SetActive(false);
            }

            yield return new WaitForSeconds(warningBlinkSpeed);
        }
    }

    void UpdateLowBatteryWarning()
    {
        if (lowBatteryText == null)
            return;

        if (hasFlashlight &&
            currentBattery <= lowBatteryLevel &&
            currentBattery > 0)
        {
            if (warningCoroutine == null)
            {
                warningCoroutine = StartCoroutine(
                    BlinkLowBatteryWarning()
                );
            }
        }
        else
        {
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }

            lowBatteryText.SetActive(false);
        }
    }
}