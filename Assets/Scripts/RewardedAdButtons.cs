using UnityEngine;
using UnityEngine.UI;

public class RewardedAdButtons : MonoBehaviour
{
    [Header("Recharge Button")]
    [SerializeField] private Button rechargeButton;

    [Header("Flashlight")]
    [SerializeField] private FlashlightController flashlightController;

    private bool rechargeUsed;
    private bool rechargePending;
    private LevelPlayAdsManager subscribedManager;

    private void Start()
    {
        if (rechargeButton != null)
        {
            rechargeButton.gameObject.SetActive(false);
            rechargeButton.interactable = true;
        }

        SubscribeToAdsManager();
    }

    private void Update()
    {
        if (rechargeButton == null ||
            flashlightController == null)
            return;

        SubscribeToAdsManager();

        if (rechargeUsed)
        {
            rechargeButton.gameObject.SetActive(false);
            return;
        }

        if (flashlightController.currentBattery <= 0f)
        {
            if (!rechargePending)
            {
                rechargeButton.gameObject.SetActive(true);
                rechargeButton.interactable = true;
            }
        }
        else
        {
            rechargeButton.gameObject.SetActive(false);
        }
    }

    // ============================================================
    // BUTTON ON CLICK
    // ============================================================

    public void ShowRechargeAd()
    {
        Debug.Log("[Recharge] Recharge button clicked");

        if (rechargeUsed)
        {
            Debug.Log("[Recharge] Already used.");
            return;
        }

        if (rechargePending)
        {
            Debug.Log("[Recharge] Already waiting for reward.");
            return;
        }

        SubscribeToAdsManager();

        if (subscribedManager == null)
        {
            Debug.LogError("[Recharge] LevelPlayAdsManager NOT FOUND.");
            return;
        }

        rechargePending = true;

        rechargeButton.interactable = false;

        subscribedManager.ShowRewarded();
    }

    // ============================================================
    // REWARD
    // ============================================================

    private void OnRechargeRewarded()
    {
        if (!rechargePending)
            return;

        Debug.Log("[Recharge] REWARD RECEIVED.");

        rechargePending = false;
        rechargeUsed = true;

        if (flashlightController != null)
        {
            flashlightController.RechargeBatteryFromReward();
        }

        if (rechargeButton != null)
        {
            rechargeButton.interactable = false;
            rechargeButton.gameObject.SetActive(false);
        }
    }

    private void OnRechargeAdRequestFailed()
    {
        if (!rechargePending || rechargeUsed)
            return;

        rechargePending = false;
        Debug.Log("[Recharge] Rewarded request ended without a reward; button is available again.");

        if (rechargeButton != null)
        {
            rechargeButton.interactable = true;
            rechargeButton.gameObject.SetActive(
                flashlightController != null && flashlightController.currentBattery <= 0f);
        }
    }

    private void SubscribeToAdsManager()
    {
        LevelPlayAdsManager manager = LevelPlayAdsManager.Instance;

        if (subscribedManager == manager)
            return;

        if (subscribedManager != null)
        {
            subscribedManager.OnRewardedAdCompleted -= OnRechargeRewarded;
            subscribedManager.OnRewardedAdRequestFailed -= OnRechargeAdRequestFailed;
        }

        subscribedManager = manager;

        if (subscribedManager != null)
        {
            subscribedManager.OnRewardedAdCompleted += OnRechargeRewarded;
            subscribedManager.OnRewardedAdRequestFailed += OnRechargeAdRequestFailed;
        }
    }

    private void OnDestroy()
    {
        if (subscribedManager != null)
        {
            subscribedManager.OnRewardedAdCompleted -=
                OnRechargeRewarded;
            subscribedManager.OnRewardedAdRequestFailed -=
                OnRechargeAdRequestFailed;
        }
    }
}
