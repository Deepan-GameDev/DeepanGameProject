using UnityEngine;
using System.Collections;

public class LightBlink : MonoBehaviour
{
    public Light targetLight;
    public float blinkInterval = 0.5f;

    private void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            targetLight.enabled = !targetLight.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}