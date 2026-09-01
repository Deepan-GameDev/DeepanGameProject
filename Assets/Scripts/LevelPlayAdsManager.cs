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

    // Used when button is clicked before rewarded ad is ready.
    private bool pendingRewardedShow;
    private bool rewardedShowInProgress;
    private bool rewardedGrantedForCurrentShow;

    public event Action OnRewardedAdCompleted;
    public event Action OnRewardedAdRequestFailed;
    public event Action OnInterstitialClosedEvent;

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

    // ============================================================
    // INITIALIZATION
    // ============================================================

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

        Debug.Log("[LevelPlay] SDK initialization started");

        LevelPlay.Init(appKey);
    }

    private void OnLevelPlayInitialized(LevelPlayConfiguration configuration)
    {
        isInitialized = true;
        isInitializing = false;

        LevelPlay.OnInitSuccess -= OnLevelPlayInitialized;
        LevelPlay.OnInitFailed -= OnLevelPlayInitializationFailed;

        Debug.Log("[LevelPlay] SDK initialization SUCCESS");

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

        Debug.LogError("[LevelPlay] SDK initialization FAILED: " + error);
        FailPendingRewardedRequest();
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

        bannerAd = new LevelPlayBannerAd(
            bannerAdUnitId,
            config
        );

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
            Debug.LogWarning("[LevelPlay] Cannot load banner. SDK not initialized.");
            return;
        }

        if (bannerAd == null)
            CreateBanner();

        Debug.Log("[LevelPlay] Loading banner...");

        bannerAd.LoadAd();
    }

    public void ShowBanner()
    {
        if (bannerAd == null)
            return;

        bannerAd.ShowAd();

        Debug.Log("[LevelPlay] Banner shown.");
    }

    public void HideBanner()
    {
        if (bannerAd == null)
            return;

        bannerAd.HideAd();
    }

    private void OnBannerLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Banner loaded.");

        if (showBannerOnStart)
            ShowBanner();
    }

    private void OnBannerLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning("[LevelPlay] Banner load failed: " + error);
    }

    private void OnBannerDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Banner displayed.");
    }

    private void OnBannerDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogWarning("[LevelPlay] Banner display failed: " + error);
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
        Debug.Log("[LevelPlay] Banner left application.");
    }

    // ============================================================
    // INTERSTITIAL
    // ============================================================

    private void CreateInterstitial()
    {
        if (interstitialAd != null)
            return;

        interstitialAd =
            new LevelPlayInterstitialAd(interstitialAdUnitId);

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
            return;

        Debug.Log("[LevelPlay] Loading interstitial...");

        interstitialAd.LoadAd();
    }

    public bool IsInterstitialReady()
    {
        return interstitialAd != null &&
               interstitialAd.IsAdReady();
    }

    public void ShowInterstitial()
    {
        if (!isInitialized || interstitialAd == null)
        {
            Debug.LogWarning("[LevelPlay] Interstitial not initialized.");
            return;
        }

        if (!interstitialAd.IsAdReady())
        {
            Debug.LogWarning("[LevelPlay] Interstitial not ready.");
            LoadInterstitial();
            return;
        }

        if (LevelPlayInterstitialAd.IsPlacementCapped(
            interstitialPlacement))
        {
            Debug.LogWarning("[LevelPlay] Interstitial placement capped.");
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
        Debug.LogWarning("[LevelPlay] Interstitial load failed: " + error);
    }

    private void OnInterstitialDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Interstitial displayed.");
    }

    private void OnInterstitialDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogWarning("[LevelPlay] Interstitial display failed: " + error);

        LoadInterstitial();
    }

    private void OnInterstitialClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Interstitial clicked.");
    }

        private void OnInterstitialClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Interstitial CLOSED");

        // Tell GameOverManager that the ad has finished.
        OnInterstitialClosedEvent?.Invoke();

        // Prepare next interstitial.
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

        rewardedAd =
            new LevelPlayRewardedAd(rewardedAdUnitId);

        rewardedAd.OnAdLoaded += OnRewardedLoaded;
        rewardedAd.OnAdLoadFailed += OnRewardedLoadFailed;
        rewardedAd.OnAdDisplayed += OnRewardedDisplayed;
        rewardedAd.OnAdDisplayFailed += OnRewardedDisplayFailed;
        rewardedAd.OnAdRewarded += OnRewardedCompleted;
        rewardedAd.OnAdClosed += OnRewardedClosed;
        rewardedAd.OnAdClicked += OnRewardedClicked;
        rewardedAd.OnAdInfoChanged += OnRewardedInfoChanged;

        Debug.Log("[LevelPlay] Rewarded object created");

        LoadRewarded();
    }

    public void LoadRewarded()
    {
        if (!isInitialized || rewardedAd == null)
        {
            Debug.LogError(
                "[LevelPlay] Rewarded LoadAd cannot run. " +
                "SDK initialized=" + isInitialized +
                ", rewarded object exists=" + (rewardedAd != null));
            return;
        }

        if (rewardedAd.IsAdReady())
        {
            Debug.Log("[LevelPlay] Rewarded already READY.");
            return;
        }

        Debug.Log("[LevelPlay] Rewarded LoadAd requested");

        rewardedAd.LoadAd();
    }

    public bool IsRewardedReady()
    {
        return rewardedAd != null &&
               rewardedAd.IsAdReady();
    }

    // ============================================================
    // THIS IS THE IMPORTANT PART
    // ============================================================

    public void ShowRewarded()
    {
        Debug.Log("[LevelPlay] Rewarded request received");

        if (rewardedShowInProgress || pendingRewardedShow)
        {
            Debug.LogWarning("[LevelPlay] Rewarded request ignored; a request is already in progress.");
            return;
        }

        if (!isInitialized)
        {
            if (isInitializing)
            {
                pendingRewardedShow = true;
                Debug.Log("[LevelPlay] SDK is still initializing; rewarded request will continue after initialization.");
                return;
            }

            Debug.LogError("[LevelPlay] Rewarded request failed: SDK is not initialized.");
            FailPendingRewardedRequest();
            return;
        }

        if (rewardedAd == null)
        {
            Debug.LogError("[LevelPlay] Rewarded request failed: rewarded object is NULL.");
            FailPendingRewardedRequest();
            return;
        }

        if (LevelPlayRewardedAd.IsPlacementCapped(
            rewardedPlacement))
        {
            Debug.LogError("[LevelPlay] Rewarded request failed: placement is capped: " + rewardedPlacement);
            FailPendingRewardedRequest();
            return;
        }

        bool isReady = rewardedAd.IsAdReady();
        Debug.Log("[LevelPlay] Rewarded IsAdReady = " + (isReady ? "TRUE" : "FALSE"));

        if (isReady)
        {
            ShowRewardedNow();
            return;
        }

        pendingRewardedShow = true;
        Debug.Log("[LevelPlay] Rewarded is not ready; loading and waiting to show.");
        LoadRewarded();
    }

    private void OnRewardedLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Rewarded LOADED");

        if (pendingRewardedShow)
        {
            Debug.Log("[LevelPlay] Pending rewarded request detected; showing now.");
            ShowRewardedNow();
        }
    }

    private void OnRewardedLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError("[LevelPlay] Rewarded LOAD FAILED: " + FormatAdError(error));
        FailPendingRewardedRequest();
    }

    private void OnRewardedDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Rewarded DISPLAYED");
    }

    private void OnRewardedDisplayFailed(
        LevelPlayAdInfo adInfo,
        LevelPlayAdError error)
    {
        Debug.LogError("[LevelPlay] Rewarded DISPLAY FAILED: " + FormatAdError(error));
        FailPendingRewardedRequest();

        LoadRewarded();
    }

    private void OnRewardedCompleted(
    LevelPlayAdInfo adInfo,
    LevelPlayReward reward)
{
    rewardedGrantedForCurrentShow = true;

    Debug.Log(
        "[LevelPlay] ============================="
    );

    Debug.Log(
        "[LevelPlay] REWARD CALLBACK RECEIVED"
    );

    Debug.Log(
        "[LevelPlay] Reward Name: " + reward.Name
    );

    Debug.Log(
        "[LevelPlay] Reward Amount: " + reward.Amount
    );

    Debug.Log(
        "[LevelPlay] Placement: " + adInfo.PlacementName
    );

    Debug.Log(
        "[LevelPlay] ============================="
    );

    OnRewardedAdCompleted?.Invoke();
}

    private void OnRewardedClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Rewarded CLOSED");

        rewardedShowInProgress = false;

        // IMPORTANT:
        // Do NOT treat OnAdClosed as "no reward".
        // OnAdRewarded and OnAdClosed are asynchronous.
        // Rewarded callback is the only place where reward is granted.

        if (!rewardedGrantedForCurrentShow)
        {
            Debug.Log(
                "[LevelPlay] Rewarded closed. " +
                "Waiting for reward callback if it arrives asynchronously."
            );
        }

        // Prepare next rewarded ad.
        LoadRewarded();
    }

    private void ShowRewardedNow()
    {
        if (rewardedAd == null)
        {
            Debug.LogError("[LevelPlay] Rewarded show failed: rewarded object is NULL.");
            FailPendingRewardedRequest();
            return;
        }

        if (LevelPlayRewardedAd.IsPlacementCapped(rewardedPlacement))
        {
            Debug.LogError("[LevelPlay] Rewarded show failed: placement is capped: " + rewardedPlacement);
            FailPendingRewardedRequest();
            return;
        }

        if (!rewardedAd.IsAdReady())
        {
            Debug.LogError("[LevelPlay] Rewarded show failed: IsAdReady became FALSE before ShowAd.");
            pendingRewardedShow = true;
            LoadRewarded();
            return;
        }

        pendingRewardedShow = false;
        rewardedShowInProgress = true;
        rewardedGrantedForCurrentShow = false;
        Debug.Log("[LevelPlay] Rewarded ShowAd requested");
        rewardedAd.ShowAd(rewardedPlacement);
    }

    private void FailPendingRewardedRequest()
    {
        pendingRewardedShow = false;
        rewardedShowInProgress = false;
        OnRewardedAdRequestFailed?.Invoke();
    }

    private static string FormatAdError(LevelPlayAdError error)
    {
        if (error == null)
            return "LevelPlayAdError was null.";

        return "Code=" + error.ErrorCode +
               ", Message=" + error.ErrorMessage +
               ", AdUnitId=" + error.AdUnitId +
               ", AdId=" + error.AdId +
               ", Raw=" + error;
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
            Instance = null;
    }
}
