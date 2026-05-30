using UnityEngine;
using System.Collections.Generic;

// LorQBRotationHelper.cs
// Provides temporary rotation group management for C12-C15 movements
// Implements LOCAL HINGE SYSTEM without recursive transform inheritance
//
// ARCHITECTURE RULES:
// 1. All cubes remain siblings under LorQB_Root (flat hierarchy)
// 2. Hinges remain independent objects
// 3. Rotations use TEMPORARY runtime groups
// 4. Groups are destroyed after animation completes
// 5. No permanent parent-child nesting between cubes
public static class LorQBRotationHelper
{
    // -------------------------------------------------------------------------
    // Create temporary rotation group for a specific hinge movement
    // -------------------------------------------------------------------------
    public static GameObject CreateRotationGroup(string groupName, Vector3 hingePosition, params GameObject[] objects)
    {
        GameObject rotGroup = new GameObject(groupName);
        rotGroup.transform.position = hingePosition;
        rotGroup.transform.rotation = Quaternion.identity;

        // Temporarily parent objects to rotation group (preserving world positions)
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.transform.SetParent(rotGroup.transform, true);
            }
        }

        Debug.Log($"[LorQB Rotation] Created temp group '{groupName}' at {hingePosition}");
        return rotGroup;
    }

    // -------------------------------------------------------------------------
    // Destroy rotation group and reparent objects back to LorQB_Root
    // -------------------------------------------------------------------------
    public static void DestroyRotationGroup(GameObject rotGroup)
    {
        if (rotGroup == null) return;

        GameObject lorqbRoot = GameObject.Find("LorQB_Root");
        if (lorqbRoot == null)
        {
            Debug.LogError("[LorQB Rotation] LorQB_Root not found! Cannot reparent objects.");
            return;
        }

        // Reparent all children back to LorQB_Root
        List<Transform> children = new List<Transform>();
        foreach (Transform child in rotGroup.transform)
        {
            children.Add(child);
        }

        foreach (Transform child in children)
        {
            child.SetParent(lorqbRoot.transform, true);
        }

        Object.Destroy(rotGroup);
        Debug.Log($"[LorQB Rotation] Destroyed temp group '{rotGroup.name}' and reparented objects to LorQB_Root");
    }

    // -------------------------------------------------------------------------
    // C12: Blue → Red rotation group (Blue, Hinge_Blue_Red, Red)
    // -------------------------------------------------------------------------
    public static GameObject CreateC12RotationGroup()
    {
        GameObject cubeBlue = GameObject.Find("Cube_Blue");
        GameObject hingeBlueRed = GameObject.Find("Hinge_Blue_Red");
        GameObject cubeRed = GameObject.Find("Cube_Red");

        if (cubeBlue == null || hingeBlueRed == null || cubeRed == null)
        {
            Debug.LogError("[LorQB Rotation] C12: Missing objects for rotation group!");
            return null;
        }

        Vector3 hingePos = hingeBlueRed.transform.position;
        return CreateRotationGroup("C12_RotGroup", hingePos, cubeBlue, hingeBlueRed, cubeRed);
    }

    // -------------------------------------------------------------------------
    // C13: Red → Green rotation group (Red, Hinge_Red_Green, Green)
    // -------------------------------------------------------------------------
    public static GameObject CreateC13RotationGroup()
    {
        GameObject cubeRed = GameObject.Find("Cube_Red");
        GameObject hingeRedGreen = GameObject.Find("Hinge_Red_Green");
        GameObject cubeGreen = GameObject.Find("Cube_Green");

        if (cubeRed == null || hingeRedGreen == null || cubeGreen == null)
        {
            Debug.LogError("[LorQB Rotation] C13: Missing objects for rotation group!");
            return null;
        }

        Vector3 hingePos = hingeRedGreen.transform.position;
        return CreateRotationGroup("C13_RotGroup", hingePos, cubeRed, hingeRedGreen, cubeGreen);
    }

    // -------------------------------------------------------------------------
    // C14: Green → Yellow rotation group (Green, Hinge_Green_Yellow, Yellow)
    // -------------------------------------------------------------------------
    public static GameObject CreateC14RotationGroup()
    {
        GameObject cubeGreen = GameObject.Find("Cube_Green");
        GameObject hingeGreenYellow = GameObject.Find("Hinge_Green_Yellow");
        GameObject cubeYellow = GameObject.Find("Cube_Yellow");

        if (cubeGreen == null || hingeGreenYellow == null || cubeYellow == null)
        {
            Debug.LogError("[LorQB Rotation] C14: Missing objects for rotation group!");
            return null;
        }

        Vector3 hingePos = hingeGreenYellow.transform.position;
        return CreateRotationGroup("C14_RotGroup", hingePos, cubeGreen, hingeGreenYellow, cubeYellow);
    }

    // -------------------------------------------------------------------------
    // C15: Yellow → Blue rotation group (Yellow, [implied hinge], Blue)
    // Note: C15 closes the loop, may need special handling
    // -------------------------------------------------------------------------
    public static GameObject CreateC15RotationGroup()
    {
        GameObject cubeYellow = GameObject.Find("Cube_Yellow");
        GameObject cubeBlue = GameObject.Find("Cube_Blue");

        if (cubeYellow == null || cubeBlue == null)
        {
            Debug.LogError("[LorQB Rotation] C15: Missing objects for rotation group!");
            return null;
        }

        // C15 rotates across the top row (Yellow-Blue connection)
        // Use midpoint between Yellow and Blue as rotation point
        Vector3 hingePos = (cubeYellow.transform.position + cubeBlue.transform.position) / 2f;
        hingePos.y = 1f; // Same height as other hinges

        return CreateRotationGroup("C15_RotGroup", hingePos, cubeYellow, cubeBlue);
    }

    // -------------------------------------------------------------------------
    // Validate flat hierarchy structure
    // -------------------------------------------------------------------------
    public static bool ValidateFlatHierarchy()
    {
        GameObject lorqbRoot = GameObject.Find("LorQB_Root");
        if (lorqbRoot == null)
        {
            Debug.LogError("[LorQB Rotation] LorQB_Root not found!");
            return false;
        }

        string[] expectedObjects = {
            "Cube_Blue", "Cube_Red", "Cube_Green", "Cube_Yellow",
            "Hinge_Blue_Red", "Hinge_Red_Green", "Hinge_Green_Yellow",
            "Ball"
        };

        bool allValid = true;
        foreach (string objName in expectedObjects)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj == null)
            {
                Debug.LogError($"[LorQB Rotation] Missing object: {objName}");
                allValid = false;
                continue;
            }

            // Check that object is direct child of LorQB_Root
            if (obj.transform.parent == null || obj.transform.parent.gameObject != lorqbRoot)
            {
                Debug.LogError($"[LorQB Rotation] {objName} is NOT a direct child of LorQB_Root! (parent: {obj.transform.parent?.name ?? "null"})");
                allValid = false;
            }
        }

        if (allValid)
        {
            Debug.Log("[LorQB Rotation] ✅ Flat hierarchy validated — all objects are siblings under LorQB_Root");
        }

        return allValid;
    }
}
