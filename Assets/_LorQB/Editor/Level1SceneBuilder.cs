#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using LorQB.Core;
using LorQB.Movement;

namespace LorQB.Editor
{
    /// <summary>
    /// Unity Editor tool that builds the Level 1 scene hierarchy with exact world positions.
    ///
    /// Usage: Unity menu → LorQB → Build Level 1 Scene
    ///
    /// Scene hierarchy created:
    ///
    ///   Level1
    ///   ├── Cubes
    ///   │   ├── Cube_Blue    (+1.0,  0.0, -1.0)
    ///   │   ├── Cube_Red     (+1.0,  0.0, +1.0)
    ///   │   ├── Cube_Green   (-1.0,  0.0, +1.0)
    ///   │   └── Cube_Yellow  (-1.0,  0.0, -1.0)
    ///   ├── Seats
    ///   │   ├── Seat_Blue    (+1.0, +0.5, -1.0)
    ///   │   ├── Seat_Red     (+1.0, +0.5, +1.0)
    ///   │   ├── Seat_Green   (-1.0, +0.5, +1.0)
    ///   │   └── Seat_Yellow  (-1.0, +0.5, -1.0)
    ///   ├── Ball             (+1.0, +0.5, -1.0)  ← starts at Seat_Blue
    ///   ├── Pivot_Blue_Red   (+1.0, +0.5,  0.0)  ← Z-axis hinge
    ///   │   ├── Hinge_Blue_Red
    ///   │   └── RotationGroup_Blue_Red
    ///   ├── Pivot_Red_Green  ( 0.0, +0.5, +1.0)  ← X-axis hinge
    ///   │   ├── Hinge_Red_Green
    ///   │   └── RotationGroup_Red_Green
    ///   ├── Pivot_Green_Yellow (-1.0, +0.5, 0.0)  ← Z-axis hinge
    ///   │   ├── Hinge_Green_Yellow
    ///   │   └── RotationGroup_Green_Yellow
    ///   └── UI
    ///       ├── Shuffle_Order_Bar
    ///       ├── Timer
    ///       └── Player_Selector
    /// </summary>
    public static class Level1SceneBuilder
    {
        // ── World-space positions ────────────────────────────────────────────────
        // Cubes are 1×1×1 units; square footprint is 2×2, centred at origin.
        private static readonly Vector3 PosCubeBlue   = new Vector3(+1f, 0f, -1f);
        private static readonly Vector3 PosCubeRed    = new Vector3(+1f, 0f, +1f);
        private static readonly Vector3 PosCubeGreen  = new Vector3(-1f, 0f, +1f);
        private static readonly Vector3 PosCubeYellow = new Vector3(-1f, 0f, -1f);

        // Seats: top-centre of each cube (Y = +0.5 above cube centre)
        private static readonly Vector3 PosSeatBlue   = new Vector3(+1f, +0.5f, -1f);
        private static readonly Vector3 PosSeatRed    = new Vector3(+1f, +0.5f, +1f);
        private static readonly Vector3 PosSeatGreen  = new Vector3(-1f, +0.5f, +1f);
        private static readonly Vector3 PosSeatYellow = new Vector3(-1f, +0.5f, -1f);

        // Pivots: at the top edge midpoint between adjacent cubes
        private static readonly Vector3 PosPivotBlueRed    = new Vector3(+1f, +0.5f,  0f);
        private static readonly Vector3 PosPivotRedGreen   = new Vector3( 0f, +0.5f, +1f);
        private static readonly Vector3 PosPivotGreenYellow= new Vector3(-1f, +0.5f,  0f);

        // Hinge rotation axes
        private static readonly Vector3 AxisBlueRed     = Vector3.forward; // Z-axis
        private static readonly Vector3 AxisRedGreen    = Vector3.right;   // X-axis
        private static readonly Vector3 AxisGreenYellow = Vector3.forward; // Z-axis

        // ── Menu entry ───────────────────────────────────────────────────────────
        [MenuItem("LorQB/Build Level 1 Scene")]
        public static void BuildLevel1Scene()
        {
            // Register undo so the operation can be reverted in one step
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Build Level 1 Scene");
            int undoGroup = Undo.GetCurrentGroup();

            // Root
            GameObject level1 = CreateGO("Level1", null, Vector3.zero);

            // ── Cubes ──
            GameObject cubesGroup = CreateGO("Cubes", level1.transform, Vector3.zero);

            GameObject cubeBlue   = CreateCube("Cube_Blue",   cubesGroup.transform, PosCubeBlue,   CubeColor.Blue);
            GameObject cubeRed    = CreateCube("Cube_Red",    cubesGroup.transform, PosCubeRed,    CubeColor.Red);
            GameObject cubeGreen  = CreateCube("Cube_Green",  cubesGroup.transform, PosCubeGreen,  CubeColor.Green);
            GameObject cubeYellow = CreateCube("Cube_Yellow", cubesGroup.transform, PosCubeYellow, CubeColor.Yellow);

            // ── Seats ──
            GameObject seatsGroup = CreateGO("Seats", level1.transform, Vector3.zero);

            GameObject seatBlue   = CreateSeat("Seat_Blue",   seatsGroup.transform, PosSeatBlue);
            GameObject seatRed    = CreateSeat("Seat_Red",    seatsGroup.transform, PosSeatRed);
            GameObject seatGreen  = CreateSeat("Seat_Green",  seatsGroup.transform, PosSeatGreen);
            GameObject seatYellow = CreateSeat("Seat_Yellow", seatsGroup.transform, PosSeatYellow);

            // Wire seats into CubeIdentifier components
            WireSeat(cubeBlue,   seatBlue);
            WireSeat(cubeRed,    seatRed);
            WireSeat(cubeGreen,  seatGreen);
            WireSeat(cubeYellow, seatYellow);

            // ── Ball ── (starts at Seat_Blue position)
            GameObject ball = CreateBall("Ball", level1.transform, PosSeatBlue);

            // ── Pivot groups ──
            CreatePivotGroup(level1.transform, "Blue_Red",    PosPivotBlueRed,     AxisBlueRed,
                             cubeRed);
            CreatePivotGroup(level1.transform, "Red_Green",   PosPivotRedGreen,    AxisRedGreen,
                             cubeGreen);
            CreatePivotGroup(level1.transform, "Green_Yellow",PosPivotGreenYellow, AxisGreenYellow,
                             cubeYellow);

            // ── UI ──
            GameObject uiGroup = CreateGO("UI", level1.transform, Vector3.zero);
            CreateGO("Shuffle_Order_Bar", uiGroup.transform, Vector3.zero);
            CreateGO("Timer",             uiGroup.transform, Vector3.zero);
            CreateGO("Player_Selector",   uiGroup.transform, Vector3.zero);

            // Select the root so the user can see it
            Selection.activeGameObject = level1;

            Undo.CollapseUndoOperations(undoGroup);
            Debug.Log("[Level1SceneBuilder] Scene hierarchy created successfully.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        /// <summary>Create an empty GameObject with the given name, parent, and world position.</summary>
        private static GameObject CreateGO(string name, Transform parent, Vector3 worldPos)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);

            if (parent != null)
                go.transform.SetParent(parent, worldPositionStays: false);

            go.transform.position = worldPos;
            return go;
        }

        /// <summary>
        /// Create a cube GameObject with a CubeIdentifier component and a box collider
        /// (for touch raycasting only — no Rigidbody).
        /// </summary>
        private static GameObject CreateCube(string name, Transform parent, Vector3 worldPos, CubeColor color)
        {
            GameObject go = CreateGO(name, parent, worldPos);

            // CubeIdentifier
            var id = Undo.AddComponent<CubeIdentifier>(go);

            // Serialized field assignment is done via SerializedObject so Undo records it
            var so  = new SerializedObject(id);
            so.FindProperty("cubeColor").enumValueIndex = (int)color;
            so.ApplyModifiedProperties();

            // BoxCollider for raycasting (no Rigidbody — not for physics)
            Undo.AddComponent<BoxCollider>(go);

            // Visual placeholder: use a Unity primitive cube, then remove its collider
            // (the BoxCollider added above is on the root GO for raycasting)
            var meshFilter   = Undo.AddComponent<MeshFilter>(go);
            var meshRenderer = Undo.AddComponent<MeshRenderer>(go);

            // Assign default cube mesh via a temporary primitive (cleaned up immediately)
            var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshFilter.sharedMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            // Leave material as default — assign coloured material in Inspector
            go.transform.localScale = Vector3.one;

            return go;
        }

        /// <summary>Create a seat marker (empty GO used as ball snap-target position).</summary>
        private static GameObject CreateSeat(string name, Transform parent, Vector3 worldPos)
        {
            return CreateGO(name, parent, worldPos);
        }

        /// <summary>Create the Ball sphere GameObject.</summary>
        private static GameObject CreateBall(string name, Transform parent, Vector3 worldPos)
        {
            GameObject go = CreateGO(name, parent, worldPos);

            var meshFilter   = Undo.AddComponent<MeshFilter>(go);
            var meshRenderer = Undo.AddComponent<MeshRenderer>(go);

            var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshFilter.sharedMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);

            go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            // No Rigidbody — position is scripted.
            return go;
        }

        /// <summary>
        /// Create a Pivot_[AB] group with Hinge_[AB] and RotationGroup_[AB] children.
        /// Also attaches CubeRotationController to the rotating cube and
        /// HoleAlignmentDetector to the Hinge child.
        /// </summary>
        private static void CreatePivotGroup(
            Transform  levelRoot,
            string     colorPairSuffix,     // e.g. "Blue_Red"
            Vector3    pivotWorldPos,
            Vector3    rotationAxis,
            GameObject rotatableCube)       // the cube that rotates around this pivot
        {
            string pivotName = $"Pivot_{colorPairSuffix}";
            string hingeName = $"Hinge_{colorPairSuffix}";
            string rotName   = $"RotationGroup_{colorPairSuffix}";

            GameObject pivot     = CreateGO(pivotName, levelRoot,       pivotWorldPos);
            GameObject hinge     = CreateGO(hingeName, pivot.transform, Vector3.zero);
            GameObject rotGroup  = CreateGO(rotName,   pivot.transform, Vector3.zero);

            // ── CubeRotationController on the rotating cube ──
            var ctrl = Undo.AddComponent<CubeRotationController>(rotatableCube);
            var so   = new SerializedObject(ctrl);
            so.FindProperty("pivotTransform").objectReferenceValue = pivot.transform;

            // Encode axis as a Vector3 property
            var axisProp = so.FindProperty("rotationAxis");
            axisProp.vector3Value = rotationAxis;

            so.ApplyModifiedProperties();

            // ── HoleAlignmentDetector on the Hinge child ──
            // HoleAxis children are added when the cube meshes are built;
            // wire them up in the Inspector after assigning meshes.
            Undo.AddComponent<HoleAlignmentDetector>(hinge);

            // ── HoleAxis marker on the Hinge for alignment reference ──
            CreateGO("HoleAxis", hinge.transform, Vector3.zero);

            // ── HoleAxis marker on the RotationGroup for the rotating side ──
            CreateGO("HoleAxis", rotGroup.transform, Vector3.zero);
        }

        /// <summary>Wire a seat Transform into the CubeIdentifier on a cube GO.</summary>
        private static void WireSeat(GameObject cube, GameObject seat)
        {
            var id = cube.GetComponent<CubeIdentifier>();
            if (id == null) return;

            var so = new SerializedObject(id);
            so.FindProperty("seatTransform").objectReferenceValue = seat.transform;
            so.ApplyModifiedProperties();
        }
    }
}
#endif
