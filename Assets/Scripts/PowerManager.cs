using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public static PowerManager Instance;

    [SerializeField] private PowerLight[] powerLights;

    public bool PowerOn { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void RestorePower()
    {
        if (PowerOn)
            return;

        PowerOn = true;

        foreach (PowerLight light in powerLights)
        {
            if (light != null)
                light.TurnOn();
        }

        Debug.Log("POWER RESTORED");
    }
}