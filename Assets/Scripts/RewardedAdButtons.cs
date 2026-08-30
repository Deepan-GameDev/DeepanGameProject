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
        // Hide buttons at start.
        if (reviveButton != null)
            reviveButton.gameObject.SetActive(false);

        if (rechargeButton != null)
        {
            rechargeButton.gameObject.SetActive(false);
            rechargeButton.interactable = true;
        }

        // Subscribe to rewarded ad completion.
        if (LevelPlayAdsManager.Instance != null)
        {
            LevelPlayAdsManager.Instance.OnRewardedAdCompleted += OnRewardedAdCompleted;
        }

        // IMPORTANT:
        // We intentionally do NOT use RemoveAllListeners().
        // This prevents accidentally removing other button events.
        if (rechargeButton != null)
        {
            rechargeButton.onClick.RemoveListener(OnRechargeButtonClicked);
            rechargeButton.onClick.AddListener(OnRechargeButtonClicked);
        }
    }

    private void Update()
    {
        if (flashlightController == null || rechargeButton == null)
            return;

        // Show recharge button ONLY when:
        // - Battery is completely empty
        // - Recharge has not already been used
        // - No recharge ad is currently waiting for reward
        if (!rechargeUsed &&
            flashlightController.currentBattery <= 0f &&
            !waitingForRechargeReward)
        {
            if (!rechargeButton.gameObject.activeSelf)
            {
                rechargeButton.gameObject.SetActive(true);
            }

            rechargeButton.interactable = true;
        }
        else
        {
            if (rechargeButton.gameObject.activeSelf &&
                (rechargeUsed ||
                 flashlightController.currentBattery > 0f ||
                 waitingForRechargeReward))
            {
                rechargeButton.gameObject.SetActive(false);
            }
        }
    }

    // ============================================================
    // RECHARGE
    // ============================================================

    // PUBLIC METHOD:
    // Can also be assigned directly from Button -> On Click().
    public void ShowRechargeAd()
    {
        OnRechargeButtonClicked();
    }

    private void OnRechargeButtonClicked()
    {
        if (rechargeUsed)
        {
            Debug.Log("[Recharge] Recharge already used.");
            return;
        }

        if (waitingForRechargeReward)
        {
            Debug.Log("[Recharge] Recharge ad is already pending.");
            return;
        }

        if (LevelPlayAdsManager.Instance == null)
        {
            Debug.LogWarning("[Recharge] LevelPlayAdsManager not found.");
            return;
        }

        if (!LevelPlayAdsManager.Instance.IsRewardedReady())
        {
            Debug.LogWarning("[Recharge] Rewarded ad is not ready. Loading...");

            LevelPlayAdsManager.Instance.LoadRewarded();

            return;
        }

        Debug.Log("[Recharge] Showing rewarded ad for flashlight recharge.");

        waitingForRechargeReward = true;

        // Disable button while ad is being shown.
        rechargeButton.interactable = false;

        LevelPlayAdsManager.Instance.ShowRewarded();
    }

    // ============================================================
    // REWARDED CALLBACK
    // ============================================================

    private void OnRewardedAdCompleted()
    {
        // Ignore rewards that were not requested for recharge.
        if (!waitingForRechargeReward)
            return;

        Debug.Log("[Recharge] Reward received.");

        waitingForRechargeReward = false;

        // ONE TIME USE.
        rechargeUsed = true;

        // Recharge flashlight to 100%.
        if (flashlightController != null)
        {
            flashlightController.AddBattery(
                flashlightController.maxBattery
            );

            Debug.Log(
                "[Recharge] Flashlight recharged to " +
                flashlightController.currentBattery + "%"
            );
        }

        // Permanently hide recharge button.
        if (rechargeButton != null)
        {
            rechargeButton.interactable = false;
            rechargeButton.gameObject.SetActive(false);
        }

        Debug.Log("[Recharge] Recharge used successfully.");
    }

    private void OnDestroy()
    {
        if (LevelPlayAdsManager.Instance != null)
        {
            LevelPlayAdsManager.Instance.OnRewardedAdCompleted -= OnRewardedAdCompleted;
        }
    }
}