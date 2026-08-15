using UnityEngine;

/// <summary>
/// Selects the most appropriate Android graphics tier before gameplay begins.
/// The selected quality asset owns the URP settings for the rest of the session.
/// </summary>
[DefaultExecutionOrder(-10000)]
public sealed class AdaptivePerformanceManager : MonoBehaviour
{
    private const int LowTier = 0;
    private const int MediumTier = 1;
    private const int HighTier = 2;

    private static AdaptivePerformanceManager instance;

    // Kept serialized so existing scene instances retain valid data after this upgrade.
    [Header("Frame-rate targets")]
    [SerializeField] private int lowFPS = 30;
    [SerializeField] private int mediumFPS = 45;
    [SerializeField] private int highFPS = 60;

    private int maximumTier;
    private int activeTier;
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
