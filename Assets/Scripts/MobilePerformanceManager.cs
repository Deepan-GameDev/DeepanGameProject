using UnityEngine;

public class MobilePerformanceManager : MonoBehaviour
{
    private void Awake()
    {
        // AdaptivePerformanceManager is created before the first scene and owns
        // frame pacing. This legacy component remains harmless for existing scenes.
        enabled = false;
    }
}
