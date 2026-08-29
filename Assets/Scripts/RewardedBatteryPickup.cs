using UnityEngine;

public class RewardedBatteryPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlashlightController flashlightController;

    [Header("Battery Reward")]
    [SerializeField] private float rechargeAmount = 25f;

    public void GiveBatteryReward()
    {
        if (flashlightController == null)
        {
            Debug.LogWarning("[RewardedBatteryPickup] FlashlightController is missing.");
            return;
        }

        flashlightController.AddBattery(rechargeAmount);

        Debug.Log("[RewardedBatteryPickup] Rewarded battery pickup collected.");
    }
}