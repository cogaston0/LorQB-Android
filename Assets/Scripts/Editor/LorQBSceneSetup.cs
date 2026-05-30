using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// LorQBSceneSetup.cs (Unity Editor)
// Step-by-step system test automation
// C10 → C12 → C13 → Validator (sequential testing)

public class LorQBSceneSetup : MonoBehaviour
{
    // ============================================================
    // STEP 1: C10 Only (Creator Test)
    // ============================================================
    [MenuItem("LorQB/Test Step 1 - C10 Only (Creator)")]
    public static void TestStep1_C10Only()
    {
        Debug.Log("[LorQB Setup] STEP 1: Testing C10 Creator only");

        GameObject lorqbRuntime = CreateOrFindRuntime();

        // Remove all components first (clean slate)
        RemoveAllTestComponents(lorqbRuntime);

        // Attach ONLY C10
        lorqbRuntime.AddComponent<C10_SceneBuild>();
        Debug.Log("[LorQB Setup] ✅ C10_SceneBuild attached");
        Debug.Log("[LorQB Setup] Press Play to test C10 scene creation");
        Debug.Log("[LorQB Setup] Expected: Cubes, ball, hinges appear");

        SaveAndSelect(lorqbRuntime);
    }

    // ============================================================
    // STEP 2: C10 + C12 (First Manipulator Test) — CORRECTED ARCHITECTURE
    // ============================================================
    [MenuItem("LorQB/Test Step 2 - C10 + C12 (Blue to Red)")]
    public static void TestStep2_C10_C12()
    {
        Debug.Log("[LorQB Setup] STEP 2: Testing C10 + C12 (Blue → Red) with flat hierarchy");

        GameObject lorqbRuntime = CreateOrFindRuntime();

        // Remove all components first
        RemoveAllTestComponents(lorqbRuntime);

        // Attach C10 + C12 + Validator
        lorqbRuntime.AddComponent<C10_SceneBuild>();
        lorqbRuntime.AddComponent<C12_BlueToRed>();
        lorqbRuntime.AddComponent<LorQBArchitectureValidator>();
        Debug.Log("[LorQB Setup] ✅ C10_SceneBuild + C12_BlueToRed + Validator attached");
        Debug.Log("[LorQB Setup] Press Play to test C12 ball transfer with corrected architecture");
        Debug.Log("[LorQB Setup] Expected: Scene builds, validation passes, ball moves Blue → Red");

        SaveAndSelect(lorqbRuntime);
    }

    // ============================================================
    // STEP 3: C10 + C13 (Second Manipulator Test)
    // ============================================================
    [MenuItem("LorQB/Test Step 3 - C10 + C13 (Red to Green)")]
    public static void TestStep3_C10_C13()
    {
        Debug.Log("[LorQB Setup] STEP 3: Testing C10 + C13 (Red → Green)");

        GameObject lorqbRuntime = CreateOrFindRuntime();

        // Remove all components first
        RemoveAllTestComponents(lorqbRuntime);

        // Attach C10 + C13
        lorqbRuntime.AddComponent<C10_SceneBuild>();
        lorqbRuntime.AddComponent<C13_RedToGreen>();
        Debug.Log("[LorQB Setup] ✅ C10_SceneBuild + C13_RedToGreen attached");
        Debug.Log("[LorQB Setup] Press Play to test C13 ball transfer");
        Debug.Log("[LorQB Setup] Expected: Scene builds, ball moves Red → Green");

        SaveAndSelect(lorqbRuntime);
    }

    // ============================================================
    // Helper Methods
    // ============================================================

    static GameObject CreateOrFindRuntime()
    {
        GameObject lorqbRuntime = GameObject.Find("LorQB_Runtime");

        if (lorqbRuntime == null)
        {
            lorqbRuntime = new GameObject("LorQB_Runtime");
            Debug.Log("[LorQB Setup] Created LorQB_Runtime GameObject");
        }
        else
        {
            Debug.Log("[LorQB Setup] Found existing LorQB_Runtime GameObject");
        }

        return lorqbRuntime;
    }

    static void RemoveAllTestComponents(GameObject obj)
    {
        // Remove C10
        var c10 = obj.GetComponent<C10_SceneBuild>();
        if (c10 != null) DestroyImmediate(c10);

        // Remove C12
        var c12 = obj.GetComponent<C12_BlueToRed>();
        if (c12 != null) DestroyImmediate(c12);

        // Remove C13
        var c13 = obj.GetComponent<C13_RedToGreen>();
        if (c13 != null) DestroyImmediate(c13);

        // Remove C13 PilotRunner
        var pilot = obj.GetComponent<C13_PilotRunner>();
        if (pilot != null) DestroyImmediate(pilot);

        Debug.Log("[LorQB Setup] Removed all existing test components (clean slate)");
    }

    static void SaveAndSelect(GameObject obj)
    {
        // Mark scene as dirty to ensure save
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        // Save the scene
        Scene activeScene = SceneManager.GetActiveScene();
        string scenePath = activeScene.path;

        if (string.IsNullOrEmpty(scenePath))
        {
            // Scene not saved yet, prompt for location
            scenePath = EditorUtility.SaveFilePanel(
                "Save Scene As",
                "Assets",
                "LorQB_Test.unity",
                "unity"
            );

            if (!string.IsNullOrEmpty(scenePath))
            {
                // Convert absolute path to relative Assets path
                if (scenePath.StartsWith(Application.dataPath))
                {
                    scenePath = "Assets" + scenePath.Substring(Application.dataPath.Length);
                }

                EditorSceneManager.SaveScene(activeScene, scenePath);
                Debug.Log($"[LorQB Setup] Scene saved to: {scenePath}");
            }
            else
            {
                Debug.LogWarning("[LorQB Setup] Scene save cancelled by user");
            }
        }
        else
        {
            // Scene already has a path, just save
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log($"[LorQB Setup] Scene saved: {scenePath}");
        }

        Debug.Log("[LorQB Setup] ✅ Setup complete - Press Play to test");

        // Select the LorQB_Runtime object in hierarchy for user visibility
        Selection.activeGameObject = obj;
    }

    // ============================================================
    // ARCHITECTURE VALIDATION: Add validator component
    // ============================================================
    [MenuItem("LorQB/Add Architecture Validator")]
    public static void AddArchitectureValidator()
    {
        GameObject lorqbRuntime = CreateOrFindRuntime();

        // Check if validator already exists
        var validator = lorqbRuntime.GetComponent<LorQBArchitectureValidator>();
        if (validator != null)
        {
            Debug.Log("[LorQB Setup] Architecture Validator already attached");
        }
        else
        {
            lorqbRuntime.AddComponent<LorQBArchitectureValidator>();
            Debug.Log("[LorQB Setup] ✅ Architecture Validator attached");
            Debug.Log("[LorQB Setup] Press Play, then wait 1 second for automatic validation");
            Debug.Log("[LorQB Setup] Or press 'V' key in Play mode to manually validate");
        }

        SaveAndSelect(lorqbRuntime);
    }
}
