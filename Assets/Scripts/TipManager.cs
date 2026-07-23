using UnityEngine;
using TMPro;

public class TipManager : MonoBehaviour
{
    public TextMeshProUGUI tipText;

    [TextArea]
    public string[] tips =
    {
        "Stay quiet.\nThey can hear every sound.",
        "Don't waste your flashlight battery.",
        "Sometimes hiding is better than running.",
        "Every locked door has a reason.",
        "Listen carefully... danger has a sound.",
        "Not everything in the dark wants to kill you.",
        "If you hear footsteps... stop moving.",
        "Some places are safer than others."
    };

    private void Start()
    {
        if (tips.Length > 0)
        {
            tipText.text = tips[Random.Range(0, tips.Length)];
        }
    }
}