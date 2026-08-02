using UnityEngine;

// LorQBArchitectureValidator.cs
// Simple test script to validate flat hierarchy after C10 scene creation
// Attach to LorQB_Runtime and run in Play mode
public class LorQBArchitectureValidator : MonoBehaviour
{
    [Header("Run validation automatically")]
    public bool validateOnStart = true;
    public float validationDelay = 1.0f; // Wait for C10 to build scene

    void Start()
    {
        if (validateOnStart)
        {
            Invoke(nameof(RunValidation), validationDelay);
        }
    }

    void Update()
    {
        // Press V key to manually run validation
        if (Input.GetKeyDown(KeyCode.V))
        {
            RunValidation();
        }
    }

    void RunValidation()
    {
        Debug.Log("========================================");
        Debug.Log("LorQB ARCHITECTURE VALIDATION");
        Debug.Log("========================================");

        bool result = LorQBRotationHelper.ValidateFlatHierarchy();

        if (result)
        {
            Debug.Log("✅✅✅ ARCHITECTURE VALID ✅✅✅");
            Debug.Log("Flat hierarchy confirmed — ready for C12-C15 movements");
        }
        else
        {
            Debug.LogError("❌❌❌ ARCHITECTURE INVALID ❌❌❌");
            Debug.LogError("Fix hierarchy issues before implementing C12-C15");
        }

        Debug.Log("========================================");

        // Additional diagnostics
        PrintHierarchyDiagnostics();
    }

    void PrintHierarchyDiagnostics()
    {
        GameObject lorqbRoot = GameObject.Find("LorQB_Root");
        if (lorqbRoot == null)
        {
            Debug.LogError("LorQB_Root not found!");
            return;
        }

        Debug.Log($"\nLorQB_Root children count: {lorqbRoot.transform.childCount}");
        Debug.Log("Children:");
        foreach (Transform child in lorqbRoot.transform)
        {
            Vector3 pos = child.position;
            Vector3 rot = child.rotation.eulerAngles;
            Debug.Log($"  - {child.name} | pos: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2}) | rot: ({rot.x:F0}, {rot.y:F0}, {rot.z:F0})");
        }
    }
}
