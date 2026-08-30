using UnityEngine;
using UnityEngine.UI;

public class RewardedAdButtons : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button reviveButton;
    [SerializeField] private Button rechargeButton;

    [Header("Game References")]
    [SerializeField] private FlashlightController flashlightController;

    private bool rechargeUsed = false;
    private bool waitingForRechargeReward = false;

    private void Start()
    {
        // Initially hide both buttons.
        if (reviveButton != null)
            reviveButton.gameObject.SetActive(false);

        if (rechargeButton != null)
            rechargeButton.gameObject.SetActive(false);

        // Subscribe to LevelPlay rewarded event.
        if (LevelPlayAdsManager.Instance != null)
        {
            LevelPlayAdsManager.Instance.OnRewardedAdCompleted += OnRewardedAdCompleted;
        }

        // Recharge button click.
        if (rechargeButton != null)
        {
            rechargeButton.onClick.RemoveAllListeners();
            rechargeButton.onClick.AddListener(OnRechargeButtonClicked);
        }
    }

    private void Update()
    {
        // Recharge can only appear when:
        // 1. Battery is 0
        // 2. Recharge has never been used
        // 3. We are not currently waiting for an ad reward

        if (flashlightController == null || rechargeButton == null)
            return;

        if (!rechargeUsed &&
            flashlightController.currentBattery <= 0f &&
            !waitingForRechargeReward)
        {
            if (!rechargeButton.gameObject.activeSelf)
            {
                rechargeButton.gameObject.SetActive(true);
            }
        }
        else
        {
            if (rechargeButton.gameObject.activeSelf &&
                (rechargeUsed || flashlightController.currentBattery > 0f))
            {
                rechargeButton.gameObject.SetActive(false);
            }
        }
    }

    private void OnRechargeButtonClicked()
    {
        if (rechargeUsed)
            return;

        if (waitingForRechargeReward)
            return;

        if (LevelPlayAdsManager.Instance == null)
        {
            Debug.LogWarning("[Recharge] LevelPlayAdsManager not found.");
            return;
        }

        if (!LevelPlayAdsManager.Instance.IsRewardedReady())
        {
            Debug.LogWarning("[Recharge] Rewarded ad is not ready.");

            // Try loading another rewarded ad.
            LevelPlayAdsManager.Instance.LoadRewarded();

            return;
        }

        Debug.Log("[Recharge] Showing rewarded ad for flashlight recharge.");

        waitingForRechargeReward = true;

        // Disable button temporarily to prevent multiple clicks.
        rechargeButton.interactable = false;

        LevelPlayAdsManager.Instance.ShowRewarded();
    }

    private void OnRewardedAdCompleted()
    {
        if (!waitingForRechargeReward)
            return;

        Debug.Log("[Recharge] Reward received. Recharging flashlight to 100%.");

        waitingForRechargeReward = false;

        // ONE TIME USE
        rechargeUsed = true;

        // Recharge flashlight completely.
        if (flashlightController != null)
        {
            flashlightController.AddBattery(
                flashlightController.maxBattery
            );
        }

        // Permanently hide recharge button for this game session.
        if (rechargeButton != null)
        {
            rechargeButton.interactable = false;
            rechargeButton.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (LevelPlayAdsManager.Instance != null)
        {
            LevelPlayAdsManager.Instance.OnRewardedAdCompleted -= OnRewardedAdCompleted;
        }
    }
}