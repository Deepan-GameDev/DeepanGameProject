using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("References")]
    public Image logo;
    public Image tapToContinue;

    public CanvasGroup logoGroup;
    public CanvasGroup tapGroup;

    public GameObject menuPanel;
    public CanvasGroup menuGroup;
    public RectTransform menuRect;

    [Header("Menu Position")]
    public Vector2 hiddenPosition;
    public Vector2 visiblePosition;

    private bool menuOpened = false;

    // Blink Variables
    private bool isBlinking = true;
    private float blinkSpeed = 1.5f;

    void Start()
    {
        // Later use pannuvom
    }

    void Update()
    {
        if (isBlinking)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            tapGroup.alpha = Mathf.Lerp(0.2f, 1f, alpha);
        }

        if (!menuOpened && Input.GetMouseButtonDown(0))
        {
            menuOpened = true;
            isBlinking = false;

            StartCoroutine(OpenMenu());
        }
    }

    IEnumerator OpenMenu()
    {
        yield return null;
    }
}