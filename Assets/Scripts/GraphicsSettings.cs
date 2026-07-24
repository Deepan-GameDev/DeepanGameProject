using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicsSettings : MonoBehaviour
{
    [Header("Buttons")]
    public Button mediumButton;
    public Button highButton;

    [Header("Button Text")]
    public TMP_Text mediumText;
    public TMP_Text highText;

    [Header("Colors")]
    public Color selectedColor = Color.white;
    public Color normalColor = Color.gray;

    [Header("Scale")]
    public float selectedScale = 1.08f;
    public float normalScale = 1f;

    private void Start()
    {
        mediumButton.onClick.AddListener(SetMedium);
        highButton.onClick.AddListener(SetHigh);

        int quality = PlayerPrefs.GetInt("GraphicsQuality", 0);

        if (quality == 0)
            SetMedium();
        else
            SetHigh();
    }

    public void SetMedium()
    {
        QualitySettings.SetQualityLevel(0, true);

        PlayerPrefs.SetInt("GraphicsQuality", 0);
        PlayerPrefs.Save();

        UpdateVisuals(true);
    }

    public void SetHigh()
    {
        QualitySettings.SetQualityLevel(1, true);

        PlayerPrefs.SetInt("GraphicsQuality", 1);
        PlayerPrefs.Save();

        UpdateVisuals(false);
    }

    void UpdateVisuals(bool mediumSelected)
    {
        mediumText.color = mediumSelected ? selectedColor : normalColor;
        highText.color = mediumSelected ? normalColor : selectedColor;

        mediumButton.transform.localScale =
            mediumSelected ? Vector3.one * selectedScale : Vector3.one * normalScale;

        highButton.transform.localScale =
            mediumSelected ? Vector3.one * normalScale : Vector3.one * selectedScale;
    }
}