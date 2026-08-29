using UnityEngine;
using UnityEngine.Events;

public class RewardedAdButtons : MonoBehaviour
{
    [Header("Reward Actions")]
    [SerializeField] private UnityEvent onReviveRewarded;
    [SerializeField] private UnityEvent onRechargeRewarded;

    private enum RewardType
    {
        None,
        Revive,
        Recharge
    }

    private RewardType pendingReward = RewardType.None;

    private void OnEnable()
    {
        if (LevelPlayAdsManager.Instance != null)
        {
            LevelPlayAdsManager.Instance.OnRewardedAdCompleted += HandleRewardedAdCompleted;
        }
    }

    private void OnDisable()
    {
        if (LevelPlayAdsManager.Instance != null)
        {
            LevelPlayAdsManager.Instance.OnRewardedAdCompleted -= HandleRewardedAdCompleted;
        }
    }

    // Connect this to Revive Button
    public void ShowReviveAd()
    {
        if (LevelPlayAdsManager.Instance == null)
        {
            Debug.LogWarning("[RewardedAdButtons] LevelPlayAdsManager not found.");
            return;
        }

        if (!LevelPlayAdsManager.Instance.IsRewardedReady())
        {
            Debug.LogWarning("[RewardedAdButtons] Revive rewarded ad is not ready.");
            LevelPlayAdsManager.Instance.LoadRewarded();
            return;
        }

        pendingReward = RewardType.Revive;

        Debug.Log("[RewardedAdButtons] Showing Revive rewarded ad.");

        LevelPlayAdsManager.Instance.ShowRewarded();
    }

    // Connect this to Recharge Button
    public void ShowRechargeAd()
    {
        if (LevelPlayAdsManager.Instance == null)
        {
            Debug.LogWarning("[RewardedAdButtons] LevelPlayAdsManager not found.");
            return;
        }

        if (!LevelPlayAdsManager.Instance.IsRewardedReady())
        {
            Debug.LogWarning("[RewardedAdButtons] Recharge rewarded ad is not ready.");
            LevelPlayAdsManager.Instance.LoadRewarded();
            return;
        }

        pendingReward = RewardType.Recharge;

        Debug.Log("[RewardedAdButtons] Showing Recharge rewarded ad.");

        LevelPlayAdsManager.Instance.ShowRewarded();
    }

    private void HandleRewardedAdCompleted()
    {
        switch (pendingReward)
        {
            case RewardType.Revive:
                Debug.Log("[RewardedAdButtons] Revive reward granted.");
                onReviveRewarded?.Invoke();
                break;

            case RewardType.Recharge:
                Debug.Log("[RewardedAdButtons] Recharge reward granted.");
                onRechargeRewarded?.Invoke();
                break;

            default:
                Debug.LogWarning("[RewardedAdButtons] Reward completed but no reward action was pending.");
                break;
        }

        pendingReward = RewardType.None;
    }
}