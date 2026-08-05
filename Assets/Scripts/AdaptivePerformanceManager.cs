using UnityEngine;

/// <summary>
/// Selects and maintains the most appropriate Android graphics tier without exposing
/// a player-facing setting. Quality assets own URP settings; this component only
/// selects between them and reacts slowly enough to avoid visible quality oscillation.
/// </summary>
[DefaultExecutionOrder(-10000)]
public sealed class AdaptivePerformanceManager : MonoBehaviour
{
    private const int LowTier = 0;
    private const int MediumTier = 1;
    private const int HighTier = 2;

    private const float SampleInterval = 2f;
    private const float DowngradeCooldown = 15f;
    private const float UpgradeCooldown = 60f;

    private static AdaptivePerformanceManager instance;

    // Kept serialized so existing scene instances retain valid data after this upgrade.
    [Header("Frame-rate targets")]
    [SerializeField] private int lowFPS = 30;
    [SerializeField] private int mediumFPS = 45;
    [SerializeField] private int highFPS = 60;

    private int maximumTier;
    private int activeTier;
    private float sampleElapsed;
    private float accumulatedFrameTime;
    private int sampledFrames;
    private float lastTierChangeTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateBeforeFirstScene()
    {
        if (instance != null)
            return;

        var manager = new GameObject(nameof(AdaptivePerformanceManager));
        manager.AddComponent<AdaptivePerformanceManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        maximumTier = DetectMaximumTier();
        ApplyTier(maximumTier);
    }

    private void Update()
    {
        // Legacy menu scripts may still call SetQualityLevel. The adaptive tier is
        // authoritative so those calls cannot disable device adaptation.
        if (QualitySettings.GetQualityLevel() != activeTier)
            QualitySettings.SetQualityLevel(activeTier, true);

        float frameTime = Time.unscaledDeltaTime;
        if (frameTime <= 0f)
            return;

        accumulatedFrameTime += frameTime;
        sampledFrames++;
        sampleElapsed += frameTime;

        if (sampleElapsed < SampleInterval)
            return;

        float averageFps = sampledFrames / accumulatedFrameTime;
        sampleElapsed = 0f;
        accumulatedFrameTime = 0f;
        sampledFrames = 0;

        float now = Time.unscaledTime;
        int targetFps = GetTargetFps(activeTier);

        // A sustained miss is required before reducing quality, preventing a load
        // spike, GC collection, or scene transition from changing presentation.
        if (averageFps < targetFps * 0.88f && activeTier > LowTier && now - lastTierChangeTime >= DowngradeCooldown)
        {
            ApplyTier(activeTier - 1);
            return;
        }

        // Only recover quality after a long stable period and never exceed the
        // hardware-derived ceiling selected at boot.
        if (averageFps >= targetFps - 1f && activeTier < maximumTier && now - lastTierChangeTime >= UpgradeCooldown)
            ApplyTier(activeTier + 1);
    }

    private int DetectMaximumTier()
    {
        int memoryMb = SystemInfo.systemMemorySize;
        int cores = SystemInfo.processorCount;
        int apiLevel = GetAndroidApiLevel();
        int longEdge = Mathf.Max(Screen.width, Screen.height);

        int tier;
        if (memoryMb > 0 && memoryMb <= 3072)
            tier = LowTier;
        else if (memoryMb > 0 && memoryMb <= 6144)
            tier = MediumTier;
        else
            tier = HighTier;

        if (cores > 0 && cores <= 4)
            tier = Mathf.Min(tier, MediumTier);
        if (apiLevel > 0 && apiLevel < 28)
            tier = Mathf.Min(tier, MediumTier);
        if (longEdge >= 2560 && memoryMb > 0 && memoryMb < 8192)
            tier = Mathf.Min(tier, MediumTier);

        string gpu = SystemInfo.graphicsDeviceName.ToLowerInvariant();
        if (gpu.Contains("adreno 3") || gpu.Contains("mali-4") || gpu.Contains("mali-t") || gpu.Contains("powervr"))
            tier = LowTier;

        return Mathf.Clamp(tier, LowTier, HighTier);
    }

    private void ApplyTier(int tier)
    {
        activeTier = Mathf.Clamp(tier, LowTier, maximumTier);
        if (QualitySettings.names.Length > activeTier)
            QualitySettings.SetQualityLevel(activeTier, true);

        Application.targetFrameRate = GetTargetFps(activeTier);
        lastTierChangeTime = Time.unscaledTime;
    }

    private int GetTargetFps(int tier)
    {
        switch (tier)
        {
            case LowTier: return lowFPS;
            case MediumTier: return mediumFPS;
            default: return highFPS;
        }
    }

    private static int GetAndroidApiLevel()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            return version.GetStatic<int>("SDK_INT");
#else
        return 0;
#endif
    }
}
