using UnityEngine;
using System.Collections;

// C12_BlueToRed.cs — Blue → Red transfer with animated gravity fall (Unity 6, LorQB-Android)
// Equivalent of Blender C12_blue_to_red.py
public class C12_BlueToRed : MonoBehaviour
{
    float hingeRotationDuration = 2f;
    float fallDuration = 1.5f;
    float gravity = 9.8f; // Simulated gravity acceleration

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

        while (elapsed < hingeRotationDuration)
        {
            elapsed += Time.deltaTime;
            hinge.transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / hingeRotationDuration);
            yield return null;
        }
        hinge.transform.rotation = endRot;
        Debug.Log("C12: Hinge rotation complete.");

        // Step 3: Animate ball falling from Blue to Red with gravity simulation
        Debug.Log("C12: Animation started");

        // Unparent ball for independent movement
        Vector3 startPos = ball.transform.position;
        ball.transform.SetParent(null, true);

        // Calculate target position (Red cube seat position)
        Vector3 targetPos = red.transform.position;
        targetPos.y = 0.25f; // Seat height

        // Simulate gravity-based fall
        elapsed = 0f;
        Vector3 velocity = Vector3.zero;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            // Apply gravity acceleration
            velocity += Vector3.down * gravity * Time.deltaTime;

            // Calculate position with parabolic trajectory
            // Use ease-in for gravity effect
            float gravityFactor = t * t; // Quadratic acceleration
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);

            // Add gravity curve to make it fall realistically
            float gravityOffset = Mathf.Lerp(0f, 0.3f, gravityFactor);
            newPos.y -= gravityOffset;

            // Clamp to not go below target
            if (newPos.y < targetPos.y)
                newPos.y = targetPos.y;

            ball.transform.position = newPos;
            yield return null;
        }

        // Ensure final position is exact
        ball.transform.position = targetPos;

        // Step 4: Parent ball to Cube_Red
        ball.transform.SetParent(red.transform, true);
        Debug.Log("C12: Ball transferred to Cube_Red");
    }
}
