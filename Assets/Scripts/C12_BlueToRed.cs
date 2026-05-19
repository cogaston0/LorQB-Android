using UnityEngine;
using System.Collections;

// C12_BlueToRed.cs (Unity 6, LorQB-Android)
// Exact port of Blender C12_blue_to_red.py
// Hinge_Blue_Red: X-axis rotation, ROT_SIGN = +1
// Transfer at 180 degrees — Latch_Blue off, Latch_Red on

public class C12_BlueToRed : MonoBehaviour
{
    // ============================================================
    // SECTION 1: Constants
    // ============================================================
    float phaseOneDuration  = 1f;
    float phaseTwoDuration  = 1f;
    float phaseBackDuration = 2f;

    Vector3 seatBlueWorld = new Vector3(0.51f,  0.25f,  0.51f);
    Vector3 seatRedWorld  = new Vector3(0.51f,  0.25f, -0.51f);

    // ============================================================
    // SECTION 2: Reset / Awake / Start
    // ============================================================
    void Start()
    {
        StartCoroutine(WaitThenTransfer());
    }

    // ============================================================
    // SECTION 3: Main transfer Coroutine
    // ============================================================
    IEnumerator TransferBallBlueToRed()
    {
        GameObject hinge = GameObject.Find("Hinge_Blue_Red");
        GameObject ball  = GameObject.Find("Ball");
        GameObject blue  = GameObject.Find("Cube_Blue");
        GameObject red   = GameObject.Find("Cube_Red");

        if (hinge == null || ball == null || blue == null || red == null)
        {
            Debug.LogError("C12: Missing objects in scene.");
            yield break;
        }

        // Set initial rotation to 0° absolute
        hinge.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        blue.transform.SetParent(hinge.transform, true);
        ball.transform.SetParent(blue.transform, true);
        ball.transform.position = seatBlueWorld;
        Debug.Log("C12: Ball parented to Cube_Blue at seat.");

        // Phase 1: Rotate from 0° to -90° absolute
        float elapsed = 0f;
        while (elapsed < phaseOneDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phaseOneDuration;
            float angle = Mathf.Lerp(0f, -90f, t);
            hinge.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
            yield return null;
        }
        hinge.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        // Phase 2: Rotate from -90° to -180° absolute
        elapsed = 0f;
        while (elapsed < phaseTwoDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phaseTwoDuration;
            float angle = Mathf.Lerp(-90f, -180f, t);
            hinge.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
            yield return null;
        }
        hinge.transform.localRotation = Quaternion.Euler(-180f, 0f, 0f);
        Debug.Log("C12: Hinge at 180 degrees — opening aligned over Red.");

        // Ball transfer
        ball.transform.SetParent(null, true);
        Vector3 startPos = ball.transform.position;
        elapsed = 0f;
        float fallDuration = 0.4f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float gravT = t * t;
            ball.transform.position = Vector3.Lerp(startPos, seatRedWorld, gravT);
            yield return null;
        }
        ball.transform.position = seatRedWorld;
        ball.transform.SetParent(red.transform, true);
        Debug.Log("C12: Ball transferred to Cube_Red.");

        // Return: Rotate from -180° back to 0° absolute (goes BACKWARD)
        elapsed = 0f;
        while (elapsed < phaseBackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phaseBackDuration;
            float angle = Mathf.Lerp(-180f, 0f, t);
            hinge.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
            yield return null;
        }
        hinge.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        Debug.Log("C12: Hinge returned to 0 — sequence complete.");
    }

    // ============================================================
    // SECTION 4: Ball latch and release helpers
    // ============================================================

    // ============================================================
    // SECTION 5: Hinge rotation helpers
    // ============================================================

    // ============================================================
    // SECTION 6: UI / button entry point
    // ============================================================
    IEnumerator WaitThenTransfer()
    {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(TransferBallBlueToRed());
    }
}
