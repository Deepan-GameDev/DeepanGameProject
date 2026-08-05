using UnityEngine;

public class AdaptivePerformanceManager : MonoBehaviour
{
    [Header("FPS")]
    public int lowFPS = 30;
    public int mediumFPS = 45;
    public int highFPS = 60;

    [Header("Quality Levels")]
    public int lowQualityLevel = 0;
    public int mediumQualityLevel = 1;
    public int highQualityLevel = 2;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        SetupPerformance();
    }

    private void SetupPerformance()
    {
        int ram = SystemInfo.systemMemorySize;

        if (ram <= 3000)
        {
            QualitySettings.SetQualityLevel(lowQualityLevel);
            Application.targetFrameRate = lowFPS;
            Debug.Log("LOW END DEVICE");
        }
        else if (ram <= 6000)
        {
            QualitySettings.SetQualityLevel(mediumQualityLevel);
            Application.targetFrameRate = mediumFPS;
            Debug.Log("MID END DEVICE");
        }
        else
        {
            QualitySettings.SetQualityLevel(highQualityLevel);
            Application.targetFrameRate = highFPS;
            Debug.Log("HIGH END DEVICE");
        }
    }
}