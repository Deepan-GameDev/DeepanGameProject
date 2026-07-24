using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static string sceneToLoad;

    private IEnumerator Start()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Don't switch immediately
        operation.allowSceneActivation = false;

        // Wait until scene is loaded
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        // Small cinematic delay
        yield return new WaitForSeconds(5f);

        operation.allowSceneActivation = true;
    }
}