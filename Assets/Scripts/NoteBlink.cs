using UnityEngine;

public class NoteBlink : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.25f;

    private Renderer[] renderers;
    private bool isBlinking;
    private float timer;
    private bool visibleState = true;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void SetBlink(bool value)
    {
        if (isBlinking == value)
            return;

        isBlinking = value;

        if (!isBlinking)
        {
            timer = 0f;
            visibleState = true;
            SetRenderers(true);
        }
    }

    private void Update()
    {
        if (!isBlinking)
            return;

        timer += Time.deltaTime;

        if (timer >= blinkInterval)
        {
            timer = 0f;

            visibleState = !visibleState;
            SetRenderers(visibleState);
        }
    }

    private void SetRenderers(bool state)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = state;
        }
    }

    private void OnDisable()
    {
        SetRenderers(true);
    }
}