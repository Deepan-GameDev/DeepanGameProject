using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InternetConnectionManager : MonoBehaviour
{
    public static InternetConnectionManager Instance { get; private set; }

    [Header("Internet Required UI")]
    [SerializeField] private GameObject noInternetPanel;
    [SerializeField] private Button retryButton;

    [Header("Internet Check")]
    [SerializeField] private float checkInterval = 2f;

    private bool internetAvailable;
    private bool checkingInternet;

    private Coroutine internetCheckCoroutine;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start blocked until connection is confirmed.
        internetAvailable = false;

        if (noInternetPanel != null)
            noInternetPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RetryInternetCheck);
        }

        RetryInternetCheck();
    }

    private void Update()
    {
        // If connection is lost, immediately block the game.
        if (Application.internetReachability ==
            NetworkReachability.NotReachable)
        {
            SetInternetState(false);
        }
    }

    private void RetryInternetCheck()
    {
        if (checkingInternet)
            return;

        if (internetCheckCoroutine != null)
            StopCoroutine(internetCheckCoroutine);

        internetCheckCoroutine =
            StartCoroutine(CheckInternetConnection());
    }

    private IEnumerator CheckInternetConnection()
    {
        checkingInternet = true;

        // No network at all.
        if (Application.internetReachability ==
            NetworkReachability.NotReachable)
        {
            SetInternetState(false);

            checkingInternet = false;
            yield break;
        }

        // Try to reach a reliable endpoint.
        using (UnityEngine.Networking.UnityWebRequest request =
               UnityEngine.Networking.UnityWebRequest.Get(
                   "https://www.google.com/generate_204"))
        {
            request.timeout = 3;

            yield return request.SendWebRequest();

            bool connected =
                request.result ==
                UnityEngine.Networking.UnityWebRequest.Result.Success;

            SetInternetState(connected);
        }

        checkingInternet = false;
    }

    private void SetInternetState(bool connected)
    {
        internetAvailable = connected;

        if (connected)
        {
            // Internet available → allow game.
            if (noInternetPanel != null)
                noInternetPanel.SetActive(false);

            Time.timeScale = 1f;

            Debug.Log("[Internet] INTERNET AVAILABLE.");
        }
        else
        {
            // Internet unavailable → block game.
            if (noInternetPanel != null)
                noInternetPanel.SetActive(true);

            Time.timeScale = 0f;

            Debug.LogWarning(
                "[Internet] INTERNET REQUIRED. GAME BLOCKED."
            );
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            RetryInternetCheck();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            RetryInternetCheck();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}