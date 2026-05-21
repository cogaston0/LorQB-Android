using UnityEngine;
using System.Collections;

// C13_RedToGreen.cs (Unity 6, LorQB-Android)
// Port of Blender C13_red_to_green.py
// Transfer: Red → Green via Hinge_Red_Green (Z-axis, ROT_SIGN = -1)

public class C13_RedToGreen : MonoBehaviour
{
    // Constants
    float rotateToTransferDuration = 1f;   // Phase 1: 0° → -180°
    float rotateBackDuration       = 2f;   // Phase 3: -180° → 0°

    Vector3 seatRed   = new Vector3( 0.51f, 0.5f, -0.51f);
    Vector3 seatGreen = new Vector3(-0.51f, 0.5f, -0.51f);

    void Start()
    {
        StartCoroutine(RunC13Transfer());
    }

    IEnumerator RunC13Transfer()
    {
        // Wait 1 second before starting
        yield return new WaitForSeconds(1f);

        Debug.Log("=== C13 Start: Red → Green ===");

        // Find all objects
        GameObject hingeRedGreen = GameObject.Find("Hinge_Red_Green");
        GameObject ball          = GameObject.Find("Ball");
        GameObject cubeBlue      = GameObject.Find("Cube_Blue");
        GameObject cubeRed       = GameObject.Find("Cube_Red");
        GameObject cubeGreen     = GameObject.Find("Cube_Green");

        if (hingeRedGreen == null || ball == null || cubeBlue == null || 
            cubeRed == null || cubeGreen == null)
        {
            Debug.LogError("C13: Missing objects — check scene for Hinge_Red_Green, Ball, Cube_Blue, Cube_Red, Cube_Green");
            yield break;
        }

        // Setup: Blue rides Red (passive carry)
        cubeBlue.transform.SetParent(cubeRed.transform, true);
        Debug.Log("C13: Blue parented to Red — passive carry");

        // Setup: Red is parented to hinge (active cube)
        cubeRed.transform.SetParent(hingeRedGreen.transform, true);
        Debug.Log("C13: Red parented to Hinge_Red_Green");

        // Setup: Ball starts in Red
        ball.transform.SetParent(cubeRed.transform, true);
        ball.transform.position = seatRed;
        Debug.Log("C13: Ball in Red at seat — Latch_Red active");

        // Initialize hinge rotation to 0°
        hingeRedGreen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // ============================================================
        // PHASE 1: Rotate Hinge_Red_Green from 0° to -180° on Z-axis
        // ============================================================
        float elapsed = 0f;
        while (elapsed < rotateToTransferDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateToTransferDuration;
            float angleZ = Mathf.Lerp(0f, 180f, t);
            hingeRedGreen.transform.localRotation = Quaternion.Euler(0f, 0f, angleZ);
            yield return null;
        }
        hingeRedGreen.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        Debug.Log("C13: Hinge at -180° — Red opening aligned over Green");

        // ============================================================
        // PHASE 2: Ball transfer Red → Green (instant snap)
        // ============================================================
        ball.transform.SetParent(null, true);
        ball.transform.position = seatGreen;
        ball.transform.SetParent(cubeGreen.transform, true);
        Debug.Log("C13: Ball transferred Red → Green — Latch_Red off, Latch_Green on");

        // ============================================================
        // PHASE 3: Rotate Hinge_Red_Green from -180° back to 0° on Z-axis
        // ============================================================
        elapsed = 0f;
        while (elapsed < rotateBackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateBackDuration;
            float angleZ = Mathf.Lerp(180f, 0f, t);
            hingeRedGreen.transform.localRotation = Quaternion.Euler(0f, 0f, angleZ);
            yield return null;
        }
        hingeRedGreen.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        Debug.Log("C13: Hinge returned to 0° — Red → Green complete");

        Debug.Log("=== C13 Complete: Red → Green ===");
        Debug.Log($"C13: ROT_SIGN = -1 | Axis: Z | Hinge: Hinge_Red_Green");
        Debug.Log($"C13: Latch_Green active at influence 1.0 — ready for C14 reuse");
    }
}
