using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingTextAnimator : MonoBehaviour
{
    public TextMeshProUGUI loadingText;

    private void Start()
    {
        StartCoroutine(AnimateLoading());
    }

    IEnumerator AnimateLoading()
    {
        while (true)
        {
            loadingText.text = "Loading.";
            yield return new WaitForSeconds(0.35f);

            loadingText.text = "Loading..";
            yield return new WaitForSeconds(0.35f);

            loadingText.text = "Loading...";
            yield return new WaitForSeconds(0.35f);
        }
    }
}