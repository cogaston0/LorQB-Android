using UnityEngine;
using System.Collections;

// C12_BlueToRed.cs — Blue → Red transfer (Unity 6, LorQB-Android)
// Equivalent of Blender C12_blue_to_red.py
public class C12_BlueToRed : MonoBehaviour
{
    float duration = 2f;

    void Start()
    {
        StartCoroutine(WaitThenTransfer());
    }

    IEnumerator WaitThenTransfer()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(TransferBallBlueToRed());
    }

    IEnumerator TransferBallBlueToRed()
    {
        GameObject hinge = GameObject.Find("Hinge_Blue_Red");
        GameObject ball = GameObject.Find("Ball");
        GameObject blue = GameObject.Find("Cube_Blue");
        GameObject red = GameObject.Find("Cube_Red");

        if (hinge == null || ball == null || blue == null || red == null)
        {
            Debug.LogError("C12: Missing objects in scene.");
            yield break;
        }

        // Step 1: Parent ball to Cube_Blue
        ball.transform.SetParent(blue.transform, true);
        Debug.Log("C12: Ball parented to Cube_Blue.");

        // Step 2: Rotate Hinge_Blue_Red 90° on X axis over duration
        float elapsed = 0f;
        Quaternion startRot = hinge.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(90f, 0f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            hinge.transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / duration);
            yield return null;
        }
        hinge.transform.rotation = endRot;

        // Step 3: Transfer ball to Cube_Red
        ball.transform.SetParent(red.transform, true);
        Debug.Log("C12: Ball transferred to Cube_Red.");
    }
}