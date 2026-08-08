using UnityEngine;

public class PickupBlink : MonoBehaviour
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

        timer = 0f;
        visibleState = true;
        SetRenderers(true);

        if (!isBlinking)
        {
            return;
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

    private void SetRenderers(bool value)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = value;
        }
    }

    private void OnDisable()
    {
        SetRenderers(true);
    }
}
