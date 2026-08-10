#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class UnusedAssetScanner : EditorWindow
{
    private Vector2 scroll;
    private List<string> unusedAssets = new List<string>();
    private bool isScanning;

    [MenuItem("Tools/Unused Asset Scanner")]
    public static void OpenWindow()
    {
        GetWindow<UnusedAssetScanner>("Unused Asset Scanner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unused Asset Scanner", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Scans Build Settings scenes and Resources and finds assets that are not referenced by them. " +
            "Results are POTENTIALLY unused assets. Do not blindly delete runtime-loaded assets.",
            MessageType.Info
        );

        GUILayout.Space(10);

        if (GUILayout.Button("SCAN PROJECT", GUILayout.Height(40)))
        {
            ScanProject();
        }

        GUILayout.Space(10);

        if (unusedAssets.Count > 0)
        {
            GUILayout.Label(
                "Potentially Unused Assets: " + unusedAssets.Count,
                EditorStyles.boldLabel
            );

            if (GUILayout.Button("Export Report"))
            {
                ExportReport();
            }

            GUILayout.Space(5);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach (string asset in unusedAssets)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button(asset, EditorStyles.label))
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(asset);

                    if (obj != null)
                    {
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label(
                "No scan performed yet.",
                EditorStyles.centeredGreyMiniLabel
            );
        }
    }

    private void ScanProject()
    {
        isScanning = true;
        unusedAssets.Clear();

        try
        {
            HashSet<string> usedAssets = new HashSet<string>();

            // -----------------------------------------
            // 1. Scan scenes included in Build Settings
            // -----------------------------------------

            EditorBuildSettingsScene[] buildScenes =
                EditorBuildSettings.scenes;

            foreach (EditorBuildSettingsScene buildScene in buildScenes)
            {
                if (!buildScene.enabled)
                    continue;

                string scenePath = buildScene.path;

                if (!string.IsNullOrEmpty(scenePath))
                {
                    AddDependencies(scenePath, usedAssets);
                }
            }

            // -----------------------------------------
            // 2. Scan Resources folders
            // -----------------------------------------

            string[] allAssets = AssetDatabase.GetAllAssetPaths();

            foreach (string path in allAssets)
            {
                if (!path.StartsWith("Assets/"))
                    continue;

                if (path.Contains("/Resources/"))
                {
                    AddDependencies(path, usedAssets);
                }
            }

            // -----------------------------------------
            // 3. Get all project assets
            // -----------------------------------------

            foreach (string path in allAssets)
            {
                if (!path.StartsWith("Assets/"))
                    continue;

                if (AssetDatabase.IsValidFolder(path))
                    continue;

                // Ignore Unity generated metadata
                if (path.EndsWith(".meta"))
                    continue;

                // Ignore this scanner itself
                if (path.Contains("/Editor/"))
                    continue;

                // Ignore obvious editor-only assets
                if (path.StartsWith("Assets/Plugins/"))
                    continue;

                // Ignore Packages
                if (path.StartsWith("Packages/"))
                    continue;

                if (!usedAssets.Contains(path))
                {
                    unusedAssets.Add(path);
                }
            }

            unusedAssets.Sort();

            Debug.Log(
                "[Unused Asset Scanner] Scan complete. Potentially unused assets: "
                + unusedAssets.Count
            );
        }
        finally
        {
            isScanning = false;
        }

        Repaint();
    }

    private void AddDependencies(
        string assetPath,
        HashSet<string> usedAssets)
    {
        if (string.IsNullOrEmpty(assetPath))
            return;

        string[] dependencies =
            AssetDatabase.GetDependencies(assetPath, true);

        foreach (string dependency in dependencies)
        {
            if (dependency.StartsWith("Assets/"))
            {
                usedAssets.Add(dependency);
            }
        }
    }

    private void ExportReport()
    {
        string reportPath =
            "Assets/UnusedAssetReport.txt";

        using (StreamWriter writer =
               new StreamWriter(reportPath))
        {
            writer.WriteLine(
                "POTENTIALLY UNUSED ASSETS"
            );

            writer.WriteLine(
                "Generated: " + System.DateTime.Now
            );

            writer.WriteLine(
                "Total: " + unusedAssets.Count
            );

            writer.WriteLine();
            writer.WriteLine("--------------------------------");

            foreach (string asset in unusedAssets)
            {
                writer.WriteLine(asset);
            }
        }

        AssetDatabase.Refresh();

        Debug.Log(
            "Unused asset report created at: " +
            reportPath
        );

        EditorUtility.RevealInFinder(
            Path.GetFullPath(reportPath)
        );
    }
}

#endif