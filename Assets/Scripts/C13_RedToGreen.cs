using System.Collections;
using UnityEngine;

public class C13_RedToGreen : MonoBehaviour
{
    // Blender C13 reference timing: frames 241-480 @ 24fps
    private const float BlenderFps = 24f;
    private const int FStart = 241;
    private const int FMid = 300;
    private const int FHold = 360;
    private const int FSwap = 361;
    private const int FRet = 420;
    private const int FEnd = 480;

    private const float RotSign = -1f; // Blender ROT_SIGN = -1.0 on Y axis

    [SerializeField] private bool runOnStart;

    private bool _running;

    private GameObject _cubeBlue;
    private GameObject _cubeRed;
    private GameObject _cubeGreen;
    private GameObject _cubeYellow;
    private GameObject _hingeRedGreen;
    private GameObject _ball;
    private GameObject _seatRed;
    private GameObject _seatGreen;

    private Vector3 _seatRedLocalInRed;
    private Vector3 _seatGreenLocalInGreen;

    private void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(RunC13());
        }
    }

    [ContextMenu("Run C13: Red -> Green")]
    public void RunFromContextMenu()
    {
        if (!_running)
        {
            StartCoroutine(RunC13());
        }
    }

    private IEnumerator RunC13()
    {
        _running = true;
        GameObject rotGroup = null;

        Debug.Log("[C13] === C13 Start: Red -> Green ===");

        if (!LorQBRotationHelper.ValidateFlatHierarchy())
        {
            Debug.LogError("[C13] Flat hierarchy validation failed before movement.");
            _running = false;
            yield break;
        }

        if (!FindSceneObjects())
        {
            _running = false;
            yield break;
        }

        _seatRedLocalInRed = _cubeRed.transform.InverseTransformPoint(_seatRed.transform.position);
        _seatGreenLocalInGreen = _cubeGreen.transform.InverseTransformPoint(_seatGreen.transform.position);

        rotGroup = LorQBRotationHelper.CreateC13RotationGroup();
        if (rotGroup == null)
        {
            Debug.LogError("[C13] Failed to create C13 rotation group.");
            _running = false;
            yield break;
        }

        // Blender passive carry adaption: include Blue in temporary group for this phase.
        _cubeBlue.transform.SetParent(rotGroup.transform, true);
        Debug.Log("[C13] Blue parented to temp C13 rotation group for passive carry.");

        _ball.transform.SetParent(rotGroup.transform, true);
        _ball.transform.position = _cubeRed.transform.TransformPoint(_seatRedLocalInRed);
        Debug.Log("[C13] Ball attached to temp group at Seat_Red.");

        rotGroup.transform.rotation = Quaternion.identity;

        float tStartToMid = FramesToSeconds(FMid - FStart);   // 0 -> 90
        float tMidToHold = FramesToSeconds(FHold - FMid);     // 90 -> 180
        float tSwapGap = FramesToSeconds(FSwap - FHold);      // hold boundary
        float tSwapToRet = FramesToSeconds(FRet - FSwap);     // 180 -> 90
        float tRetToEnd = FramesToSeconds(FEnd - FRet);       // 90 -> 0

        yield return RotateGroupToAngle(rotGroup, 0f, 90f, tStartToMid, "[C13] Phase 1: 0° -> 90°");
        yield return RotateGroupToAngle(rotGroup, 90f, 180f, tMidToHold, "[C13] Phase 2: 90° -> 180°");

        Debug.Log("[C13] Hold at 180° (frame 360 boundary).");
        if (tSwapGap > 0f)
        {
            yield return new WaitForSeconds(tSwapGap);
        }

        _ball.transform.position = _cubeGreen.transform.TransformPoint(_seatGreenLocalInGreen);
        Debug.Log("[C13] Ball transfer: Seat_Red -> Seat_Green (frame 360 -> 361 boundary).");

        yield return RotateGroupToAngle(rotGroup, 180f, 90f, tSwapToRet, "[C13] Phase 3: 180° -> 90°");
        yield return RotateGroupToAngle(rotGroup, 90f, 0f, tRetToEnd, "[C13] Phase 4: 90° -> 0°");

        LorQBRotationHelper.DestroyRotationGroup(rotGroup);
        rotGroup = null;

        if (!LorQBRotationHelper.ValidateFlatHierarchy())
        {
            Debug.LogError("[C13] Flat hierarchy validation failed after movement.");
            _running = false;
            yield break;
        }

        Debug.Log("[C13] === C13 Complete: Red -> Green ===");
        Debug.Log($"[C13] Frames {FStart}-{FEnd} | Transfer at {FHold}->{FSwap}");
        Debug.Log("[C13] Axis: Y | ROT_SIGN: -1.0 | Hinge: Hinge_Red_Green");

        _running = false;
    }

    private bool FindSceneObjects()
    {
        _cubeBlue = GameObject.Find("Cube_Blue");
        _cubeRed = GameObject.Find("Cube_Red");
        _cubeGreen = GameObject.Find("Cube_Green");
        _cubeYellow = GameObject.Find("Cube_Yellow");
        _hingeRedGreen = GameObject.Find("Hinge_Red_Green");
        _ball = GameObject.Find("Ball");
        _seatRed = GameObject.Find("Seat_Red");
        _seatGreen = GameObject.Find("Seat_Green");

        bool ok = true;
        ok &= Require(_cubeBlue, "Cube_Blue");
        ok &= Require(_cubeRed, "Cube_Red");
        ok &= Require(_cubeGreen, "Cube_Green");
        ok &= Require(_cubeYellow, "Cube_Yellow");
        ok &= Require(_hingeRedGreen, "Hinge_Red_Green");
        ok &= Require(_ball, "Ball");
        ok &= Require(_seatRed, "Seat_Red");
        ok &= Require(_seatGreen, "Seat_Green");

        if (ok)
        {
            Debug.Log("[C13] Required scene objects located.");
        }

        return ok;
    }

    private static bool Require(Object obj, string name)
    {
        if (obj != null)
        {
            return true;
        }

        Debug.LogError($"[C13] Missing required object: {name}");
        return false;
    }

    private static float FramesToSeconds(int frames)
    {
        return Mathf.Max(0f, frames / BlenderFps);
    }

    private static IEnumerator RotateGroupToAngle(GameObject rotGroup, float fromDeg, float toDeg, float duration, string phaseLog)
    {
        Debug.Log(phaseLog);

        if (duration <= 0f)
        {
            ApplySignedYRotation(rotGroup.transform, toDeg);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = Mathf.Lerp(fromDeg, toDeg, t);
            ApplySignedYRotation(rotGroup.transform, angle);
            yield return null;
        }

        ApplySignedYRotation(rotGroup.transform, toDeg);
    }

    private static void ApplySignedYRotation(Transform target, float unsignedDegrees)
    {
        target.rotation = Quaternion.Euler(0f, RotSign * unsignedDegrees, 0f);
    }
}
