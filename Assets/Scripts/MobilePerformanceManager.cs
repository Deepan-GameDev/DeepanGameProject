using UnityEngine;

public class MobilePerformanceManager : MonoBehaviour
{
    [Header("Performance")]
    [SerializeField] private int targetFPS = 60;
    [SerializeField] private bool disableVSync = true;

    private void Awake()
    {
        if (disableVSync)
            QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = targetFPS;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}