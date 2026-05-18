using UnityEngine;
using System.Collections;

// C12_BlueToRed.cs (Unity 6, LorQB-Android)
// Blue → Red transfer: hinge rotates Cube_Blue to align opening, ball falls through
public class C12_BlueToRed : MonoBehaviour
{
    float hingeRotationDuration = 2f;
    float fallDuration = 1.2f;

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

        // Step 1: Parent Cube_Blue to Hinge so it rotates with it
        blue.transform.SetParent(hinge.transform, true);

        // Step 2: Parent ball to Cube_Blue — ball rides inside the cube
        ball.transform.SetParent(blue.transform, true);
        Debug.Log("C12: Ball parented to Cube_Blue.");

        // Step 3: Rotate Hinge_Blue_Red 90° on X axis — aligns Blue opening over Red
        float elapsed = 0f;
        Quaternion startRot = hinge.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(90f, 0f, 0f);

        while (elapsed < hingeRotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hingeRotationDuration;
            hinge.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }
        hinge.transform.rotation = endRot;
        Debug.Log("C12: Hinge rotation complete — opening aligned.");

        // Step 4: Unparent ball — it now falls freely through the aligned opening
        ball.transform.SetParent(null, true);

        // Step 5: Gravity fall from current world position down into Red seat
        Vector3 startPos = ball.transform.position;
        Vector3 targetPos = red.transform.position + new Vector3(0f, 0.25f, 0f);

        Debug.Log("C12: Animation started — ball falling.");

        elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float gravityT = t * t; // Quadratic ease-in = gravity acceleration

            Vector3 pos = Vector3.Lerp(startPos, targetPos, gravityT);
            pos.y = Mathf.Max(pos.y, targetPos.y); // Never go below seat
            ball.transform.position = pos;
            yield return null;
        }

        ball.transform.position = targetPos;

        // Step 6: Parent ball to Cube_Red — locked in seat
        ball.transform.SetParent(red.transform, true);
        Debug.Log("C12: Ball transferred to Cube_Red.");
    }
}