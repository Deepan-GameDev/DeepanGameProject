// Copyright(c) 2025 Laxminarayan Artistry(LNA). All rights reserved.

/*
 Use of AutoSwitchMaterialShader.cs:
 - Automatically updates materials inside the same package folder where this script lives.
 - Switches materials according to the active render pipeline.
 - Provides a manual menu command:
   Tools -> AutoSwitchMaterial (LNA) -> Fix Materials
*/

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;

namespace LaxminarayanArtistry.MaterialTools
{
    [InitializeOnLoad]
    public static class AutoSwitchMaterialShader
    {
        // ---------------------------------------------------------
        // EDITOR INITIALIZATION
        // ---------------------------------------------------------

        static AutoSwitchMaterialShader()
        {
            EditorApplication.delayCall += FixMaterials;
        }

        // ---------------------------------------------------------
        // MENU
        // ---------------------------------------------------------

        [MenuItem("Tools/AutoSwitchMaterial (LNA)/Fix Materials")]
        public static void FixMaterials()
        {
            string pipeline = GetRenderPipeline();
            string shaderName = GetShaderForCurrentPipeline();
            string targetFolder = GetMaterialsFolderPath();

            Debug.Log(
                $"Active Pipeline: {pipeline} -- Shader: {shaderName}"
            );

            Debug.Log(
                $"Target Folder: {targetFolder}"
            );

            // -----------------------------------------------------
            // VALIDATE MATERIAL FOLDER
            // -----------------------------------------------------

            if (string.IsNullOrEmpty(targetFolder) ||
                !Directory.Exists(targetFolder))
            {
                Debug.LogWarning(
                    $"SpeedTree material fixer: Materials folder not found: {targetFolder}"
                );

                return;
            }

            // -----------------------------------------------------
            // VALIDATE SHADER
            // -----------------------------------------------------

            if (string.IsNullOrEmpty(shaderName))
            {
                Debug.LogWarning(
                    "SpeedTree material fixer: Unknown render pipeline."
                );

                return;
            }

            Shader targetShader = Shader.Find(shaderName);

            if (targetShader == null)
            {
                Debug.LogError(
                    $"Shader '{shaderName}' not found in project."
                );

                return;
            }

            // -----------------------------------------------------
            // FIND MATERIALS
            // -----------------------------------------------------

            string[] matPaths =
                Directory.GetFiles(
                    targetFolder,
                    "*.mat",
                    SearchOption.AllDirectories
                );

            int changedCount = 0;

            // -----------------------------------------------------
            // PROCESS MATERIALS
            // -----------------------------------------------------

            foreach (string path in matPaths)
            {
                Material mat =
                    AssetDatabase.LoadAssetAtPath<Material>(path);

                if (mat == null)
                {
                    continue;
                }

                bool changed = false;

                string currentShader =
                    mat.shader != null
                        ? mat.shader.name
                        : "None";

                // -------------------------------------------------
                // CHANGE SHADER
                // -------------------------------------------------

                if (!currentShader.Contains("SpeedTree8"))
                {
                    mat.shader = targetShader;
                    changed = true;

                    Debug.Log(
                        $"Converted '{mat.name}' from '{currentShader}' -> '{shaderName}'"
                    );
                }
                else if (currentShader != shaderName)
                {
                    mat.shader = targetShader;
                    changed = true;

                    Debug.Log(
                        $"Updated '{mat.name}' to correct pipeline shader: '{shaderName}'"
                    );
                }

                // -------------------------------------------------
                // DOUBLE-SIDED SETTINGS
                // -------------------------------------------------

                switch (pipeline)
                {
                    case "HDRP":

                        if (mat.HasProperty("_DoubleSidedEnable"))
                        {
                            mat.SetFloat(
                                "_DoubleSidedEnable",
                                1f
                            );

                            changed = true;
                        }

                        mat.doubleSidedGI = true;

                        break;

                    case "URP":
                    case "Built-in":

                        if (mat.HasProperty("_TwoSidedEnum"))
                        {
                            mat.SetInt(
                                "_TwoSidedEnum",
                                1
                            );

                            changed = true;
                        }

                        if (mat.HasProperty("_CullMode"))
                        {
                            mat.SetInt(
                                "_CullMode",
                                0
                            );

                            changed = true;
                        }

                        break;
                }

                // -------------------------------------------------
                // SAVE MATERIAL
                // -------------------------------------------------

                if (changed)
                {
                    EditorUtility.SetDirty(mat);
                    changedCount++;
                }
            }

            // -----------------------------------------------------
            // SAVE ASSETS
            // -----------------------------------------------------

            if (changedCount > 0)
            {
                AssetDatabase.SaveAssets();

                Debug.Log(
                    $"Updated {changedCount} material(s)."
                );
            }
            else
            {
                Debug.Log(
                    "No materials needed updates."
                );
            }
        }

        // ---------------------------------------------------------
        // RENDER PIPELINE
        // ---------------------------------------------------------

        private static string GetRenderPipeline()
        {
            var pipeline =
                UnityEngine.Rendering.GraphicsSettings
                    .defaultRenderPipeline;

            if (pipeline == null)
            {
                return "Built-in";
            }

            string type =
                pipeline.GetType().ToString();

            if (type.Contains("HDRenderPipelineAsset"))
            {
                return "HDRP";
            }

            if (type.Contains("UniversalRenderPipelineAsset"))
            {
                return "URP";
            }

            return "Unknown";
        }

        // ---------------------------------------------------------
        // SHADER
        // ---------------------------------------------------------

        private static string GetShaderForCurrentPipeline()
        {
            switch (GetRenderPipeline())
            {
                case "HDRP":
                    return "HDRP/Nature/SpeedTree8";

                case "URP":
                    return "Universal Render Pipeline/Nature/SpeedTree8";

                case "Built-in":
                    return "Nature/SpeedTree8";
            }

            return null;
        }

        // ---------------------------------------------------------
        // MATERIAL FOLDER
        // ---------------------------------------------------------

        private static string GetMaterialsFolderPath()
        {
            string scriptPath =
                GetScriptPathFromStackTrace();

            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogError(
                    "Could not determine script asset path via StackTrace."
                );

                return null;
            }

            string editorFolder =
                Path.GetDirectoryName(scriptPath)
                    .Replace("\\", "/");

            string packageFolder =
                Path.GetDirectoryName(editorFolder)
                    .Replace("\\", "/");

            string materialsPath =
                Path.Combine(
                    packageFolder,
                    "Materials"
                ).Replace("\\", "/");

            return materialsPath;
        }

        // ---------------------------------------------------------
        // FIND SCRIPT PATH
        // ---------------------------------------------------------

        private static string GetScriptPathFromStackTrace()
        {
            var stackTrace =
                new System.Diagnostics.StackTrace(true);

            var frames =
                stackTrace.GetFrames();

            if (frames == null)
            {
                return null;
            }

            foreach (var frame in frames)
            {
                string fileName =
                    frame.GetFileName();

                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                if (fileName.EndsWith(
                    "AutoSwitchMaterialShader.cs"))
                {
                    string fullPath =
                        fileName.Replace("\\", "/");

                    int assetsIndex =
                        fullPath.IndexOf("Assets/");

                    if (assetsIndex >= 0)
                    {
                        return fullPath.Substring(
                            assetsIndex
                        );
                    }
                }
            }

            return null;
        }
    }
}

#endif