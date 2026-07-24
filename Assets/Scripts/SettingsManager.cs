using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Sensitivity")]
    public Slider sensitivitySlider;

    public static float Sensitivity = 1f;

    private void Start()
    {
        // Load saved sensitivity
        Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);

        sensitivitySlider.value = Sensitivity;

        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    public void SetSensitivity(float value)
    {
        Sensitivity = value;

        PlayerPrefs.SetFloat("Sensitivity", value);
        PlayerPrefs.Save();
    }
}