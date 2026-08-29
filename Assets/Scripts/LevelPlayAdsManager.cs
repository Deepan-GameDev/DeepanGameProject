using System;
using UnityEngine;
using Unity.Services.LevelPlay;

public class LevelPlayAdsManager : MonoBehaviour
{
    public static LevelPlayAdsManager Instance { get; private set; }

    [Header("LevelPlay App Key")]
    [SerializeField] private string appKey = "27d8152f5";

    [Header("Android Ad Unit IDs")]
    [SerializeField] private string bannerAdUnitId = "ucoc4n1f73x6vost";
    [SerializeField] private string interstitialAdUnitId = "fvk1g07xlogdezny";
    [SerializeField] private string rewardedAdUnitId = "mnsdxu9i5f0n8y3f";

    [Header("Placement Names")]
    [SerializeField] private string interstitialPlacement = "Interstitial_Android";
    [SerializeField] private string rewardedPlacement = "Rewarded_Android";

    [Header("Banner Settings")]
    [SerializeField] private bool loadBannerOnStart = true;
    [SerializeField] private bool showBannerOnStart = true;

    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedAd;

    private bool isInitialized;
    private bool isInitializing;

    // This callback is fired when a rewarded ad is successfully completed.
    public event Action OnRewardedAdCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeLevelPlay();
    }

    private void InitializeLevelPlay()
    {
        if (isInitialized || isInitializing)
            return;

        if (string.IsNullOrWhiteSpace(appKey))
        {
            Debug.LogError("[LevelPlay] App Key is missing.");
            return;
        }

        isInitializing = true;

        LevelPlay.OnInitSuccess += OnLevelPlayInitialized;
        LevelPlay.OnInitFailed += OnLevelPlayInitializationFailed;

        Debug.Log("[LevelPlay] Initializing SDK...");

        LevelPlay.Init(appKey);
    }

    private void OnLevelPlayInitialized(LevelPlayConfiguration configuration)
    {
        isInitialized = true;
        isInitializing = false;

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitializationFailed;

        Debug.Log("[LevelPlay] SDK initialized successfully.");

        CreateInterstitial();
        CreateRewarded();
        CreateBanner();

        if (loadBannerOnStart)
        {
            LoadBanner();
        }
    }

    private void OnLevelPlayInitializationFailed(LevelPlayInitError error)
    {
        isInitialized = false;
        isInitializing = false;

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitializationFailed;

        Debug.LogError($"[LevelPlay] SDK initialization failed: {error}");
    }

    // ============================================================
    // BANNER
    // ============================================================

    private void CreateBanner()
    {
        if (bannerAd != null)
            return;

        var config = new LevelPlayBannerAd.Config.Builder()
            .SetSize(LevelPlayAdSize.BANNER)
            .SetPosition(LevelPlayBannerPosition.BottomCenter)
            .SetDisplayOnLoad(false)
            .SetRespectSafeArea(true)
            .SetPlacementName("")
            .Build();

        bannerAd = new LevelPlayBannerAd(bannerAdUnitId, config);

        bannerAd.OnAdLoaded += OnBannerLoaded;
        bannerAd.OnAdLoadFailed += OnBannerLoadFailed;
        bannerAd.OnAdDisplayed += OnBannerDisplayed;
        bannerAd.OnAdDisplayFailed += OnBannerDisplayFailed;
        bannerAd.OnAdClicked += OnBannerClicked;
        bannerAd.OnAdCollapsed += OnBannerCollapsed;
        bannerAd.OnAdExpanded += OnBannerExpanded;
        bannerAd.OnAdLeftApplication += OnBannerLeftApplication;

        Debug.Log("[LevelPlay] Banner created.");
    }

    public void LoadBanner()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[LevelPlay] Cannot load banner. SDK is not initialized.");
            return;
        }

        if (bannerAd == null)
        {
            CreateBanner();
        }

        Debug.Log("[LevelPlay] Loading banner...");
        bannerAd.LoadAd();
    }

    public void ShowBanner()
    {
        if (bannerAd == null)
        {
            Debug.LogWarning("[LevelPlay] Banner is not created.");
            return;
        }

        bannerAd.ShowAd();

        Debug.Log("[LevelPlay] Banner shown.");
    }

    public void HideBanner()
    {
        if (bannerAd == null)
            return;

        bannerAd.HideAd();

        Debug.Log("[LevelPlay] Banner hidden.");
    }

    public void DestroyBanner()
    {
        if (bannerAd == null)
            return;

        bannerAd.DestroyAd();
        bannerAd = null;

        Debug.Log("[LevelPlay] Banner destroyed.");
    }

    private void OnBannerLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Banner loaded.");

        if (showBannerOnStart)
        {
            ShowBanner();
        }
    }

    private void OnBannerLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning($"[LevelPlay] Banner load failed: {error}");
    }

    private void OnBannerDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Banner displayed.");
    }

    private void OnBannerDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogWarning($"[LevelPlay] Banner display failed: {error}");
    }

    private void OnBannerClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Banner clicked.");
    }

    private void OnBannerCollapsed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Banner collapsed.");
    }

    private void OnBannerExpanded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Banner expanded.");
    }

    private void OnBannerLeftApplication(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] User left application from banner.");
    }

    // ============================================================
    // INTERSTITIAL
    // ============================================================

    private void CreateInterstitial()
    {
        if (interstitialAd != null)
            return;

        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

        interstitialAd.OnAdLoaded += OnInterstitialLoaded;
        interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
        interstitialAd.OnAdDisplayed += OnInterstitialDisplayed;
        interstitialAd.OnAdDisplayFailed += OnInterstitialDisplayFailed;
        interstitialAd.OnAdClicked += OnInterstitialClicked;
        interstitialAd.OnAdClosed += OnInterstitialClosed;
        interstitialAd.OnAdInfoChanged += OnInterstitialInfoChanged;

        Debug.Log("[LevelPlay] Interstitial created.");

        LoadInterstitial();
    }

    public void LoadInterstitial()
    {
        if (!isInitialized || interstitialAd == null)
            return;

        if (interstitialAd.IsAdReady())
        {
            Debug.Log("[LevelPlay] Interstitial already ready.");
            return;
        }

        Debug.Log("[LevelPlay] Loading interstitial...");
        interstitialAd.LoadAd();
    }

    public bool IsInterstitialReady()
    {
        if (interstitialAd == null)
            return false;

        return interstitialAd.IsAdReady();
    }

    public void ShowInterstitial()
    {
        if (!isInitialized || interstitialAd == null)
        {
            Debug.LogWarning("[LevelPlay] Interstitial is not initialized.");
            return;
        }

        if (!interstitialAd.IsAdReady())
        {
            Debug.LogWarning("[LevelPlay] Interstitial is not ready.");
            LoadInterstitial();
            return;
        }

        if (LevelPlayInterstitialAd.IsPlacementCapped(interstitialPlacement))
        {
            Debug.LogWarning("[LevelPlay] Interstitial placement is capped.");
            return;
        }

        Debug.Log("[LevelPlay] Showing interstitial...");

        interstitialAd.ShowAd(interstitialPlacement);
    }

    private void OnInterstitialLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Interstitial loaded.");
    }

    private void OnInterstitialLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning($"[LevelPlay] Interstitial load failed: {error}");
    }

    private void OnInterstitialDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Interstitial displayed.");
    }

    private void OnInterstitialDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogWarning($"[LevelPlay] Interstitial display failed: {error}");

        LoadInterstitial();
    }

    private void OnInterstitialClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Interstitial clicked.");
    }

    private void OnInterstitialClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Interstitial closed.");

        // Prepare the next interstitial.
        LoadInterstitial();
    }

    private void OnInterstitialInfoChanged(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Interstitial info changed.");
    }

    // ============================================================
    // REWARDED
    // ============================================================

    private void CreateRewarded()
    {
        if (rewardedAd != null)
            return;

        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);

        rewardedAd.OnAdLoaded += OnRewardedLoaded;
        rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
        rewardedAd.OnAdDisplayed += OnRewardedDisplayed;
        rewardedAd.OnAdDisplayFailed += OnRewardedDisplayFailed;
        rewardedAd.OnAdRewarded += OnRewardedCompleted;
        rewardedAd.OnAdClosed += OnRewardedClosed;
        rewardedAd.OnAdClicked += OnRewardedClicked;
        rewardedAd.OnAdInfoChanged += OnRewardedInfoChanged;

        Debug.Log("[LevelPlay] Rewarded created.");

        LoadRewarded();
    }

    public void LoadRewarded()
    {
        if (!isInitialized || rewardedAd == null)
            return;

        if (rewardedAd.IsAdReady())
        {
            Debug.Log("[LevelPlay] Rewarded already ready.");
            return;
        }

        Debug.Log("[LevelPlay] Loading rewarded...");

        rewardedAd.LoadAd();
    }

    public bool IsRewardedReady()
    {
        if (rewardedAd == null)
            return false;

        return rewardedAd.IsAdReady();
    }

    public void ShowRewarded()
    {
        if (!isInitialized || rewardedAd == null)
        {
            Debug.LogWarning("[LevelPlay] Rewarded is not initialized.");
            return;
        }

        if (!rewardedAd.IsAdReady())
        {
            Debug.LogWarning("[LevelPlay] Rewarded is not ready.");
            LoadRewarded();
            return;
        }

        if (LevelPlayRewardedAd.IsPlacementCapped(rewardedPlacement))
        {
            Debug.LogWarning("[LevelPlay] Rewarded placement is capped.");
            return;
        }

        Debug.Log("[LevelPlay] Showing rewarded...");

        rewardedAd.ShowAd(rewardedPlacement);
    }

    private void OnRewardedLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Rewarded loaded.");
    }

    private void OnRewardedLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning($"[LevelPlay] Rewarded load failed: {error}");
    }

    private void OnRewardedDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Rewarded displayed.");
    }

    private void OnRewardedDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogWarning($"[LevelPlay] Rewarded display failed: {error}");

        LoadRewarded();
    }

    private void OnRewardedCompleted(
        LevelPlayAdInfo adInfo,
        LevelPlayReward reward)
    {
        Debug.Log(
            $"[LevelPlay] Reward earned: {reward.Name} x {reward.Amount}"
        );

        // Notify the game that the player earned the reward.
        OnRewardedAdCompleted?.Invoke();
    }

    private void OnRewardedClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Rewarded closed.");

        // Prepare the next rewarded ad.
        LoadRewarded();
    }

    private void OnRewardedClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Rewarded clicked.");
    }

    private void OnRewardedInfoChanged(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Rewarded info changed.");
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitializationFailed;

        if (bannerAd != null)
        {
            bannerAd.OnAdLoaded -= OnBannerLoaded;
            bannerAd.OnAdLoadFailed -= OnBannerLoadFailed;
            bannerAd.OnAdDisplayed -= OnBannerDisplayed;
            bannerAd.OnAdDisplayFailed -= OnBannerDisplayFailed;
            bannerAd.OnAdClicked -= OnBannerClicked;
            bannerAd.OnAdCollapsed -= OnBannerCollapsed;
            bannerAd.OnAdExpanded -= OnBannerExpanded;
            bannerAd.OnAdLeftApplication -= OnBannerLeftApplication;

            bannerAd.DestroyAd();
        }

        if (interstitialAd != null)
        {
            interstitialAd.OnAdLoaded -= OnInterstitialLoaded;
            interstitialAd.OnAdLoadFailed -= OnInterstitialLoadFailed;
            interstitialAd.OnAdDisplayed -= OnInterstitialDisplayed;
            interstitialAd.OnAdDisplayFailed -= OnInterstitialDisplayFailed;
            interstitialAd.OnAdClicked -= OnInterstitialClicked;
            interstitialAd.OnAdClosed -= OnInterstitialClosed;
            interstitialAd.OnAdInfoChanged -= OnInterstitialInfoChanged;
        }

        if (rewardedAd != null)
        {
            rewardedAd.OnAdLoaded -= OnRewardedLoaded;
            rewardedAd.OnAdLoadFailed -= OnRewardedLoadFailed;
            rewardedAd.OnAdDisplayed -= OnRewardedDisplayed;
            rewardedAd.OnAdDisplayFailed -= OnRewardedDisplayFailed;
            rewardedAd.OnAdRewarded -= OnRewardedCompleted;
            rewardedAd.OnAdClosed -= OnRewardedClosed;
            rewardedAd.OnAdClicked -= OnRewardedClicked;
            rewardedAd.OnAdInfoChanged -= OnRewardedInfoChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}