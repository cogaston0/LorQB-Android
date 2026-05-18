using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// LorQBSceneBuilder (Unity 6, LorQB-Android)
// Hollow transparent glassy cubes — rounded corners + circular holes
// Matches Blender C10 layout
public class LorQBSceneBuilder : MonoBehaviour
{
    const float HOLE_RADIUS = 0.3f;
    const float CORNER_RADIUS = 0.10f;
    const int FACE_GRID = 14;     // subdivisions per face (more = smoother circle hole)
    const float HALF = 0.5f;

    void Start()
    {
        BuildScene();
        Camera.main.transform.position = new Vector3(0f, 2f, -3f);
        Camera.main.transform.LookAt(new Vector3(0f, 0.5f, 0f));
    }

    void BuildScene()
    {
        // Define cube colors
        Color blueColor = new Color(0.20f, 0.55f, 1.00f, 0.35f);
        Color redColor = new Color(1.00f, 0.10f, 0.10f, 0.35f);
        Color greenColor = new Color(0.10f, 0.90f, 0.20f, 0.35f);
        Color yellowColor = new Color(1.00f, 1.00f, 0.00f, 0.35f);

        // Blue   top-right   — 2 holes: top + left side
        CreateHollowCube("Cube_Blue",
            new Vector3(0.51f, 0.5f, 0.51f),
            blueColor, "left");

        // Red    bottom-right — 1 hole: top only
        CreateHollowCube("Cube_Red",
            new Vector3(0.51f, 0.5f, -0.51f),
            redColor, null);

        // Green  bottom-left  — 1 hole: top only
        CreateHollowCube("Cube_Green",
            new Vector3(-0.51f, 0.5f, -0.51f),
            greenColor, null);

        // Yellow top-left    — 2 holes: top + right side
        CreateHollowCube("Cube_Yellow",
            new Vector3(-0.51f, 0.5f, 0.51f),
            yellowColor, "right");

        // Create hinges with adjacent cube colors
        CreateHinge("Hinge_Blue_Red", new Vector3(0.51f, 1f, 0f), blueColor, redColor);
        CreateHinge("Hinge_Red_Green", new Vector3(0f, 1f, -0.51f), redColor, greenColor);
        CreateHinge("Hinge_Green_Yellow", new Vector3(-0.51f, 1f, 0f), greenColor, yellowColor);

        CreateBall(new Vector3(0.51f, 0.25f, 0.51f));

        Debug.Log("LorQB Scene Built.");
    }

    // -------------------------------------------------------------------------
    // Hollow cube: 6 rounded faces, holes where required
    // -------------------------------------------------------------------------
    void CreateHollowCube(string cubeName, Vector3 pos, Color color, string sideHole)
    {
        GameObject cube = new GameObject(cubeName);
        cube.transform.position = pos;
        Material mat = CreateGlassyMaterial(color);

        // Face: normal, uDir, vDir, hasHole
        BuildFace(cube, "Top", Vector3.up, Vector3.right, Vector3.forward, true, mat);
        BuildFace(cube, "Bottom", Vector3.down, Vector3.right, Vector3.back, false, mat);
        BuildFace(cube, "Front", Vector3.forward, Vector3.right, Vector3.up, false, mat);
        BuildFace(cube, "Back", Vector3.back, Vector3.left, Vector3.up, false, mat);
        BuildFace(cube, "Left", Vector3.left, Vector3.back, Vector3.up, sideHole == "left", mat);
        BuildFace(cube, "Right", Vector3.right, Vector3.forward, Vector3.up, sideHole == "right", mat);
    }

    // -------------------------------------------------------------------------
    // Build one face of the hollow cube with optional circular hole
    // -------------------------------------------------------------------------
    void BuildFace(GameObject parent, string faceName,
                   Vector3 normal, Vector3 uDir, Vector3 vDir,
                   bool hasHole, Material mat)
    {
        int n = FACE_GRID;
        const int CIRCLE_SEGMENTS = 32; // Number of segments for circular hole

        // Generate (n+1)x(n+1) rounded-cube vertices
        Vector3[,] pts = new Vector3[n + 1, n + 1];
        for (int vi = 0; vi <= n; vi++)
            for (int ui = 0; ui <= n; ui++)
            {
                float u = (float)ui / n - 0.5f;
                float v = (float)vi / n - 0.5f;
                Vector3 flat = normal * HALF + uDir * u + vDir * v;
                pts[vi, ui] = RoundedCubeProject(flat, HALF, CORNER_RADIUS);
            }

        var verts = new List<Vector3>();
        var tris = new List<int>();

        if (hasHole)
        {
            // Create face with circular hole using proper cylindrical cutout
            BuildFaceWithCircularHole(pts, n, normal, uDir, vDir, verts, tris);
        }
        else
        {
            // No hole: standard grid triangulation
            int[,] idx = new int[n + 1, n + 1];
            for (int vi = 0; vi <= n; vi++)
                for (int ui = 0; ui <= n; ui++)
                {
                    idx[vi, ui] = verts.Count;
                    verts.Add(pts[vi, ui]);
                }

            for (int vi = 0; vi < n; vi++)
                for (int ui = 0; ui < n; ui++)
                {
                    int v00 = idx[vi, ui];
                    int v10 = idx[vi, ui + 1];
                    int v01 = idx[vi + 1, ui];
                    int v11 = idx[vi + 1, ui + 1];

                    tris.Add(v00); tris.Add(v10); tris.Add(v11);
                    tris.Add(v00); tris.Add(v11); tris.Add(v01);
                }
        }

        if (tris.Count == 0) return;

        Mesh m = new Mesh { name = faceName };
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();

        GameObject faceObj = new GameObject(faceName);
        faceObj.transform.SetParent(parent.transform, false);
        faceObj.AddComponent<MeshFilter>().mesh = m;
        faceObj.AddComponent<MeshRenderer>().material = mat;
    }

    // -------------------------------------------------------------------------
    // Build face mesh with precise circular hole (cylinder boolean subtraction)
    // -------------------------------------------------------------------------
    void BuildFaceWithCircularHole(Vector3[,] pts, int n,
                                    Vector3 normal, Vector3 uDir, Vector3 vDir,
                                    List<Vector3> verts, List<int> tris)
    {
        const int SEG   = 32;
        const int RINGS = 8;
        float maxR = HALF * Mathf.Sqrt(2f) + 0.05f;

        int ringStart = verts.Count;

        for (int r = 0; r <= RINGS; r++)
        {
            float radius = Mathf.Lerp(HOLE_RADIUS, maxR, (float)r / RINGS);
            for (int s = 0; s < SEG; s++)
            {
                float angle = s * 2f * Mathf.PI / SEG;
                float u = Mathf.Clamp(radius * Mathf.Cos(angle), -HALF, HALF);
                float v = Mathf.Clamp(radius * Mathf.Sin(angle), -HALF, HALF);
                Vector3 flat = normal * HALF + uDir * u + vDir * v;
                verts.Add(RoundedCubeProject(flat, HALF, CORNER_RADIUS));
            }
        }

        for (int r = 0; r < RINGS; r++)
            for (int s = 0; s < SEG; s++)
            {
                int sN  = (s + 1) % SEG;
                int i00 = ringStart + r * SEG + s;
                int i10 = ringStart + r * SEG + sN;
                int i01 = ringStart + (r + 1) * SEG + s;
                int i11 = ringStart + (r + 1) * SEG + sN;

                tris.Add(i00); tris.Add(i10); tris.Add(i11);
                tris.Add(i00); tris.Add(i11); tris.Add(i01);
            }
    }

    // -------------------------------------------------------------------------
    // Projects a point onto the surface of a rounded cube (SDF approach)
    // -------------------------------------------------------------------------
    Vector3 RoundedCubeProject(Vector3 p, float halfSize, float r)
    {
        float b = halfSize - r;

        Vector3 clamped = new Vector3(
            Mathf.Clamp(p.x, -b, b),
            Mathf.Clamp(p.y, -b, b),
            Mathf.Clamp(p.z, -b, b)
        );

        Vector3 delta = p - clamped;
        float len = delta.magnitude;

        return len > 0.0001f ? clamped + delta.normalized * r : p;
    }

    // -------------------------------------------------------------------------
    // Glassy URP transparent material
    // -------------------------------------------------------------------------
    Material CreateGlassyMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetFloat("_Surface", 1f);    // Transparent
        mat.SetFloat("_Blend", 0f);    // Alpha blend
        mat.SetFloat("_Cull", 0f);    // Double-sided
        mat.SetFloat("_AlphaClip", 0f);
        mat.SetFloat("_ZWrite", 0f);
        mat.SetFloat("_Smoothness", 0.95f); // Glassy
        mat.SetFloat("_Metallic", 0.05f);
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        mat.color = color;
        return mat;
    }

    void CreateHinge(string name, Vector3 pos, Color colorA, Color colorB)
    {
        GameObject hinge = new GameObject(name);
        hinge.transform.position = pos;

        // Determine hinge axis based on name
        // Hinge_Blue_Red and Hinge_Green_Yellow: X axis
        // Hinge_Red_Green: Y axis
        bool isXAxis = name.Contains("Blue_Red") || name.Contains("Green_Yellow");
        Vector3 pinAxis = isXAxis ? Vector3.right : Vector3.forward;
        Vector3 leafNormal = isXAxis ? Vector3.forward : Vector3.right;

        // Pin segment dimensions
        float pinRadius = 0.025f;
        float pinLength = 0.0875f;
        float totalLength = 0.5f;
        int pinCount = 4;
        float spacing = totalLength / (pinCount - 1);

        // Create white material for pins
        Material pinMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        pinMat.color = Color.white;

        // Create 4 white cylinder pin segments
        for (int i = 0; i < pinCount; i++)
        {
            GameObject pinSegment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pinSegment.name = $"Pin_{i}";
            pinSegment.transform.SetParent(hinge.transform, false);

            // Position along hinge axis
            float offset = -totalLength / 2f + i * spacing;
            pinSegment.transform.localPosition = pinAxis * offset;

            // Scale: cylinder default is height=2, diameter=1
            // We want radius 0.025, length 0.0875
            float diameter = pinRadius * 2f;
            float halfLength = pinLength / 2f;
            pinSegment.transform.localScale = new Vector3(diameter, halfLength, diameter);

            // Rotate cylinder to align with hinge axis
            if (isXAxis)
                pinSegment.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            else
                pinSegment.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            pinSegment.GetComponent<Renderer>().material = pinMat;
        }

        // Leaf dimensions: 0.5 x 0.015 x 0.12
        Vector3 leafSize = new Vector3(0.5f, 0.015f, 0.12f);

        // Create two flat cube leaves (one each side)
        CreateHingeLeaf(hinge.transform, "Leaf_A", leafNormal * 0.06f, leafSize, colorA, isXAxis);
        CreateHingeLeaf(hinge.transform, "Leaf_B", leafNormal * -0.06f, leafSize, colorB, isXAxis);
    }

    void CreateHingeLeaf(Transform parent, string name, Vector3 offset, Vector3 size, Color color, bool isXAxis)
    {
        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leaf.name = name;
        leaf.transform.SetParent(parent, false);
        leaf.transform.localPosition = offset;

        // Scale leaf: default cube is 1x1x1
        // For X-axis hinges: leaf extends along X (0.5), thin in Z (0.015), width Y (0.12)
        // For Y-axis hinges: leaf extends along Y (0.5), thin in X (0.015), width Z (0.12)
        if (isXAxis)
            leaf.transform.localScale = new Vector3(size.x, size.z, size.y); // X, Y, Z
        else
            leaf.transform.localScale = new Vector3(size.y, size.x, size.z); // X, Y, Z

        // Create material matching adjacent cube color
        Material leafMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        leafMat.color = new Color(color.r, color.g, color.b, 1f); // Opaque version
        leaf.GetComponent<Renderer>().material = leafMat;
    }

    void CreateBall(Vector3 pos)
    {
        GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "Ball";
        ball.transform.position = pos;
        ball.transform.localScale = Vector3.one * 0.5f;
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.95f, 0.95f, 0.95f);
        mat.SetFloat("_Smoothness", 0.9f);
        ball.GetComponent<Renderer>().material = mat;
    }
}
