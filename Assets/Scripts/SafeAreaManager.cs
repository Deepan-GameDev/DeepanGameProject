using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Applies Android display cutout safe areas to gameplay controls and menu content.
/// Full-screen visual panels remain edge-to-edge so fades, vignettes and modal backdrops
/// never expose the area behind them.
/// </summary>
[DefaultExecutionOrder(-1000)]
public sealed class SafeAreaManager : MonoBehaviour
{
    private static readonly HashSet<SafeArea> SafeAreaRoots = new();
    private static SafeAreaManager instance;

    private Rect lastSafeArea;
    private Vector2Int lastResolution;
    private ScreenOrientation lastOrientation;
    private int lastSceneHandle = int.MinValue;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        EnsureInstance();
    }

    internal static void Register(SafeArea safeArea)
    {
        if (safeArea == null)
            return;

        SafeAreaRoots.Add(safeArea);
        EnsureInstance().Refresh(true);
    }

    internal static void Unregister(SafeArea safeArea)
    {
        if (safeArea != null)
            SafeAreaRoots.Remove(safeArea);
    }

    private static SafeAreaManager EnsureInstance()
    {
        if (instance != null)
            return instance;

        GameObject managerObject = new("SafeAreaManager");
        DontDestroyOnLoad(managerObject);
        instance = managerObject.AddComponent<SafeAreaManager>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Refresh(true);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void Update()
    {
        if (Screen.safeArea != lastSafeArea ||
            lastResolution.x != Screen.width ||
            lastResolution.y != Screen.height ||
            lastOrientation != Screen.orientation ||
            lastSceneHandle != SceneManager.GetActiveScene().handle)
        {
            Refresh(true);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Refresh(true);
    }

    private void Refresh(bool normalizeHierarchy)
    {
        lastSafeArea = Screen.safeArea;
        lastResolution = new Vector2Int(Screen.width, Screen.height);
        lastOrientation = Screen.orientation;
        lastSceneHandle = SceneManager.GetActiveScene().handle;

        SafeArea[] roots = FindObjectsByType<SafeArea>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        SafeAreaRoots.Clear();
        foreach (SafeArea root in roots)
        {
            SafeAreaRoots.Add(root);
            root.Apply(lastSafeArea);
        }

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            ConfigureScaler(canvas.GetComponent<CanvasScaler>());
        }

        if (normalizeHierarchy)
        {
            foreach (SafeArea root in SafeAreaRoots)
                MoveFullScreenPanelsOutsideSafeArea(root);
        }
    }

    private static void ConfigureScaler(CanvasScaler scaler)
    {
        if (scaler == null || scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize)
            return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void MoveFullScreenPanelsOutsideSafeArea(SafeArea safeAreaRoot)
    {
        Canvas canvas = safeAreaRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        RectTransform canvasTransform = canvas.transform as RectTransform;
        if (canvasTransform == null)
            return;

        // Only immediate children are considered: nested content stays inside its panel,
        // so buttons and text remain part of their existing menu hierarchy.
        for (int i = safeAreaRoot.transform.childCount - 1; i >= 0; i--)
        {
            RectTransform child = safeAreaRoot.transform.GetChild(i) as RectTransform;
            if (child == null || !IsFullScreenPanel(child.name))
                continue;

            child.SetParent(canvasTransform, false);
            StretchToParent(child);
        }

        // Some scenes already place a fader directly below the Canvas, but its original
        // centred 100x100 RectTransform cannot cover non-reference resolutions.
        foreach (Transform transformChild in canvasTransform)
        {
            RectTransform child = transformChild as RectTransform;
            if (child == null)
                continue;

            if (IsFullScreenPanel(child.name))
                StretchToParent(child);
        }
    }

    private static bool IsFullScreenPanel(string objectName)
    {
        return objectName.Equals("FadePanel", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("FaderPanel", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("BloodVignette", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("DeathScreen", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("DeathMessagePanel", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("GameOverPanel", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("PausePanel", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("SettingsPanel", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("NotePanel", StringComparison.OrdinalIgnoreCase) ||
               objectName.Equals("EscapePanel", StringComparison.OrdinalIgnoreCase);
    }

    private static void StretchToParent(RectTransform panel)
    {
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.sizeDelta = Vector2.zero;
    }
}
