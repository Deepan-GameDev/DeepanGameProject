using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightningEffect : MonoBehaviour
{
    public Image flashImage;
    public AudioSource audioSource;
    public AudioClip thunderClip;

    [Range(0f, 1f)]
    public float flashAlpha = 0.45f;

    public float minDelay = 8f;
    public float maxDelay = 15f;

    private void Start()
    {
        Color c = flashImage.color;
        c.a = 0f;
        flashImage.color = c;

        StartCoroutine(LightningLoop());
    }

    IEnumerator LightningLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            yield return StartCoroutine(Flash());

            if (audioSource != null && thunderClip != null)
            {
                audioSource.PlayOneShot(thunderClip);
            }
        }
    }

    IEnumerator Flash()
    {
        Color c = flashImage.color;

        c.a = flashAlpha;
        flashImage.color = c;

        yield return new WaitForSeconds(0.08f);

        c.a = 0f;
        flashImage.color = c;
    }
}