#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UnusedAssetScanner : EditorWindow
{
    private Vector2 scroll;

    private List<string> safeUnused = new List<string>();
    private List<string> prefabOnly = new List<string>();
    private List<string> sceneOnly = new List<string>();

    private bool scanning;

    [MenuItem("Tools/Unused Asset Scanner")]
    public static void OpenWindow()
    {
        GetWindow<UnusedAssetScanner>("Unused Asset Scanner");
    }

    private void OnGUI()
    {
        GUILayout.Label(
            "Unused Asset Scanner",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Scans ALL scenes, Resources and Prefabs. " +
            "Assets with no detected references are marked as POTENTIALLY UNUSED. " +
            "Runtime-loaded assets may still require manual verification.",
            MessageType.Info
        );

        GUILayout.Space(8);

        if (GUILayout.Button("SCAN PROJECT", GUILayout.Height(40)))
        {
            ScanProject();
        }

        GUILayout.Space(10);

        if (scanning)
        {
            EditorGUILayout.HelpBox(
                "Scanning project... Please wait.",
                MessageType.Info
            );

            return;
        }

        if (safeUnused.Count == 0 &&
            prefabOnly.Count == 0 &&
            sceneOnly.Count == 0)
        {
            GUILayout.Label(
                "No scan performed yet.",
                EditorStyles.centeredGreyMiniLabel
            );

            return;
        }

        GUILayout.Label(
            "RESULTS",
            EditorStyles.boldLabel
        );

        GUILayout.Label(
            "Potentially Safe Unused: " + safeUnused.Count,
            EditorStyles.boldLabel
        );

        GUILayout.Label(
            "Prefab-Only References: " + prefabOnly.Count
        );

        GUILayout.Label(
            "Scene-Only References: " + sceneOnly.Count
        );

        GUILayout.Space(8);

        if (GUILayout.Button("Export Full Report"))
        {
            ExportReport();
        }

        GUILayout.Space(10);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        DrawSection(
            "POTENTIALLY UNUSED",
            safeUnused
        );

        DrawSection(
            "PREFAB-ONLY REFERENCES",
            prefabOnly
        );

        DrawSection(
            "SCENE-ONLY REFERENCES",
            sceneOnly
        );

        EditorGUILayout.EndScrollView();
    }

    private void DrawSection(
        string title,
        List<string> list)
    {
        GUILayout.Space(10);

        GUILayout.Label(
            title + " (" + list.Count + ")",
            EditorStyles.boldLabel
        );

        foreach (string path in list)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                path,
                EditorStyles.label))
            {
                Object obj =
                    AssetDatabase.LoadAssetAtPath<Object>(path);

                if (obj != null)
                {
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void ScanProject()
    {
        scanning = true;

        safeUnused.Clear();
        prefabOnly.Clear();
        sceneOnly.Clear();

        try
        {
            HashSet<string> sceneReferences =
                new HashSet<string>();

            HashSet<string> prefabReferences =
                new HashSet<string>();

            HashSet<string> resourceReferences =
                new HashSet<string>();

            string[] allAssets =
                AssetDatabase.GetAllAssetPaths();

            // =====================================================
            // 1. SCAN ALL SCENES
            // =====================================================

            string[] scenes =
                allAssets
                    .Where(p =>
                        p.StartsWith("Assets/") &&
                        p.EndsWith(".unity"))
                    .ToArray();

            foreach (string scene in scenes)
            {
                AddDependencies(
                    scene,
                    sceneReferences
                );
            }

            // =====================================================
            // 2. SCAN ALL RESOURCES AS ROOTS
            // =====================================================

            foreach (string asset in allAssets)
            {
                if (!asset.StartsWith("Assets/"))
                    continue;

                if (asset.Contains("/Resources/"))
                {
                    AddDependencies(
                        asset,
                        resourceReferences
                    );
                }
            }

            // =====================================================
            // 3. SCAN ALL PREFABS
            // =====================================================

            string[] prefabs =
                allAssets
                    .Where(p =>
                        p.StartsWith("Assets/") &&
                        p.EndsWith(".prefab"))
                    .ToArray();

            foreach (string prefab in prefabs)
            {
                AddDependencies(
                    prefab,
                    prefabReferences
                );
            }

            // =====================================================
            // 4. CLASSIFY PROJECT ASSETS
            // =====================================================

            foreach (string asset in allAssets)
            {
                if (!asset.StartsWith("Assets/"))
                    continue;

                if (AssetDatabase.IsValidFolder(asset))
                    continue;

                if (asset.EndsWith(".meta"))
                    continue;

                // Never touch editor tools
                if (asset.Contains("/Editor/"))
                    continue;

                // Never classify Packages
                if (asset.StartsWith("Packages/"))
                    continue;

                // Never classify package/plugin infrastructure
                if (asset.StartsWith("Assets/Plugins/"))
                    continue;

                // Never classify this scanner
                if (asset.Contains("UnusedAssetScanner.cs"))
                    continue;

                bool usedByScene =
                    sceneReferences.Contains(asset);

                bool usedByPrefab =
                    prefabReferences.Contains(asset);

                bool usedByResources =
                    resourceReferences.Contains(asset);

                // -------------------------------------------------
                // Completely unreferenced
                // -------------------------------------------------

                if (!usedByScene &&
                    !usedByPrefab &&
                    !usedByResources)
                {
                    safeUnused.Add(asset);
                    continue;
                }

                // -------------------------------------------------
                // Prefab-only
                // -------------------------------------------------

                if (!usedByScene &&
                    usedByPrefab)
                {
                    prefabOnly.Add(asset);
                    continue;
                }

                // -------------------------------------------------
                // Scene-only
                // -------------------------------------------------

                if (usedByScene &&
                    !usedByPrefab)
                {
                    sceneOnly.Add(asset);
                }
            }

            safeUnused.Sort();
            prefabOnly.Sort();
            sceneOnly.Sort();

            Debug.Log(
                "[Unused Asset Scanner] Complete\n" +
                "Potentially unused: " +
                safeUnused.Count +
                "\nPrefab-only: " +
                prefabOnly.Count +
                "\nScene-only: " +
                sceneOnly.Count
            );
        }
        finally
        {
            scanning = false;
        }

        Repaint();
    }

    private void AddDependencies(
        string rootAsset,
        HashSet<string> target)
    {
        if (string.IsNullOrEmpty(rootAsset))
            return;

        string[] dependencies =
            AssetDatabase.GetDependencies(
                rootAsset,
                true
            );

        foreach (string dependency in dependencies)
        {
            if (dependency.StartsWith("Assets/"))
            {
                target.Add(dependency);
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
                "UNUSED ASSET SCANNER REPORT"
            );

            writer.WriteLine(
                "Generated: " +
                System.DateTime.Now
            );

            writer.WriteLine();

            writer.WriteLine(
                "========================================"
            );

            writer.WriteLine(
                "POTENTIALLY UNUSED: " +
                safeUnused.Count
            );

            writer.WriteLine(
                "========================================"
            );

            foreach (string path in safeUnused)
            {
                writer.WriteLine(path);
            }

            writer.WriteLine();

            writer.WriteLine(
                "========================================"
            );

            writer.WriteLine(
                "PREFAB ONLY: " +
                prefabOnly.Count
            );

            writer.WriteLine(
                "========================================"
            );

            foreach (string path in prefabOnly)
            {
                writer.WriteLine(path);
            }

            writer.WriteLine();

            writer.WriteLine(
                "========================================"
            );

            writer.WriteLine(
                "SCENE ONLY: " +
                sceneOnly.Count
            );

            writer.WriteLine(
                "========================================"
            );

            foreach (string path in sceneOnly)
            {
                writer.WriteLine(path);
            }
        }

        AssetDatabase.Refresh();

        Debug.Log(
            "Report created: " +
            reportPath
        );

        EditorUtility.RevealInFinder(
            Path.GetFullPath(reportPath)
        );
    }
}

#endif