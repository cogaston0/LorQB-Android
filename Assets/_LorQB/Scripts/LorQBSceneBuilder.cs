// LorQBSceneBuilder.cs  (Blender 5.1.1 reference build)
// Builds the entire LorQB 3D scene programmatically on Play.
// Attach to an empty GameObject named "LorQBSceneBuilder" in an otherwise empty scene.
// Place in: Assets/LorQB/Scripts/

using UnityEngine;

namespace LorQB
{
    /// <summary>
    /// Creates all cubes, hinges, ball, lights, and camera rigs on Awake.
    /// Nothing needs to be placed by hand in the Editor.
    /// </summary>
    public class LorQBSceneBuilder : MonoBehaviour
    {
        [Header("References (auto-filled on Awake)")]
        public LorQBGameManager GameManager;

        void Awake()
        {
            BuildScene();
        }

        void BuildScene()
        {
            // ── Game Manager + Shuffler ───────────────────────────────────────
            var gmGo = new GameObject("LorQBGameManager");
            GameManager = gmGo.AddComponent<LorQBGameManager>();
            var shuffler = gmGo.AddComponent<LorQBShuffler>();
            GameManager.Shuffler = shuffler;

            // ── Lighting ─────────────────────────────────────────────────────
            BuildLighting();

            // ── Camera ───────────────────────────────────────────────────────
            BuildCamera();

            // ── Cubes ────────────────────────────────────────────────────────
            var cubeNodes = new LorQBCubeNode[4];
            for (int i = 0; i < 4; i++)
                cubeNodes[i] = BuildCube((CubeId)i);

            // ── Hinges (visual only) ──────────────────────────────────────────
            var hingeGos = new GameObject[3];
            for (int i = 0; i < 3; i++)
                hingeGos[i] = BuildHinge(i);

            // ── Ball ─────────────────────────────────────────────────────────
            var ball = BuildBall();

            // ── Wire up GameManager ───────────────────────────────────────────
            GameManager.Cubes     = cubeNodes;
            GameManager.Hinges    = hingeGos;
            GameManager.Ball      = ball;

            // ── Input ─────────────────────────────────────────────────────────
            var inputGo = new GameObject("LorQBInputHandler");
            var input = inputGo.AddComponent<LorQBInputHandler>();
            input.Manager = GameManager;

            // ── UI ────────────────────────────────────────────────────────────
            BuildUI(GameManager);
        }

        // ── Cube factory ──────────────────────────────────────────────────────
        LorQBCubeNode BuildCube(CubeId id)
        {
            int idx = (int)id;
            var go = new GameObject($"Cube_{LorQBConfig.CubeNames[idx]}");
            go.transform.position = LorQBConfig.CubePositions[idx];

            // Hollow cube — 5 face panels (top always open; Blue -X open; Yellow +X open)
            var mat = MakeGlassMaterial(LorQBConfig.CubeColors[idx]);
            MeshRenderer firstPanel = BuildHollowCube(go, id, mat);

            // Invisible trigger collider spans full cube volume for raycasting
            var col = go.AddComponent<BoxCollider>();
            col.size = Vector3.one * LorQBConfig.CUBE_SIZE;

            // Node component — reference first panel renderer for highlight effects
            var node = go.AddComponent<LorQBCubeNode>();
            node.Id       = id;
            node.Renderer = firstPanel;

            return node;
        }

        // ── Hollow cube: face panels with openings ────────────────────────────
        // Matches C10_scene_build.py hole layout:
        //   All cubes : top face open  (+Y) — ball entry for C-series / Preplenipine transfers
        //   Blue  (0) : left face open (-X) — side hole facing Yellow (Preplenipine)
        //   Yellow(3) : right face open(+X) — side hole facing Blue   (Preplenipine)
        MeshRenderer BuildHollowCube(GameObject parent, CubeId id, Material mat)
        {
            float s = LorQBConfig.CUBE_SIZE;
            float t = s * 0.038f;   // wall thickness
            float h = s * 0.5f;     // half extent

            bool isBlue   = (id == CubeId.Blue);
            bool isYellow = (id == CubeId.Yellow);

            MeshRenderer first = null;

            // Bottom face — always present
            first = AddFacePanel(parent, new Vector3(0f, -h, 0f), new Vector3(s, t, s), mat);

            // Front (+Z) and Back (-Z) — always present
            AddFacePanel(parent, new Vector3(0f, 0f,  h), new Vector3(s, s, t), mat);
            AddFacePanel(parent, new Vector3(0f, 0f, -h), new Vector3(s, s, t), mat);

            // Left (-X): omit for Blue (Preplenipine side hole toward Yellow)
            if (!isBlue)
                AddFacePanel(parent, new Vector3(-h, 0f, 0f), new Vector3(t, s, s), mat);

            // Right (+X): omit for Yellow (Preplenipine side hole toward Blue)
            if (!isYellow)
                AddFacePanel(parent, new Vector3(h, 0f, 0f), new Vector3(t, s, s), mat);

            // Top (+Y): always omitted — ball entry hole (C-series)

            return first;
        }

        MeshRenderer AddFacePanel(GameObject parent, Vector3 localPos, Vector3 size, Material mat)
        {
            var panel = new GameObject("Face");
            panel.transform.SetParent(parent.transform, false);
            panel.transform.localPosition = localPos;
            panel.transform.localScale    = size;

            var mf = panel.AddComponent<MeshFilter>();
            mf.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

            var mr = panel.AddComponent<MeshRenderer>();
            mr.material              = mat;
            mr.shadowCastingMode     = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows        = false;

            return mr;
        }

        // ── Hinge factory ─────────────────────────────────────────────────────
        GameObject BuildHinge(int index)
        {
            var go = new GameObject($"Hinge_{index}");
            go.transform.position = LorQBConfig.HingePivots[index];

            var mf = go.AddComponent<MeshFilter>();
            var cylinder = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
            mf.mesh = cylinder;

            var mr = go.AddComponent<MeshRenderer>();
            mr.material = MakeMetal();

            go.transform.localScale = new Vector3(
                LorQBConfig.HINGE_RADIUS * 2f,
                LorQBConfig.HINGE_LENGTH * 0.5f,
                LorQBConfig.HINGE_RADIUS * 2f
            );
            // Orient hinge along Z (into screen for a front-facing 2D layout)
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            return go;
        }

        // ── Ball factory ──────────────────────────────────────────────────────
        LorQBBallController BuildBall()
        {
            var go = new GameObject("Ball");
            // Ball starts OUTSIDE cubes per design spec (Rule 1 / shuffle system)
            go.transform.position = LorQBConfig.BallStartPos;

            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

            var mr = go.AddComponent<MeshRenderer>();
            mr.material = MakeBallMaterial(LorQBConfig.EmissiveColors[0]);

            float s = LorQBConfig.BALL_RADIUS * 2f;
            go.transform.localScale = new Vector3(s, s, s);

            // Soft glow sphere
            var glowGo = new GameObject("BallGlow");
            glowGo.transform.SetParent(go.transform, false);
            var gmf = glowGo.AddComponent<MeshFilter>();
            gmf.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            var gmr = glowGo.AddComponent<MeshRenderer>();
            var glowMat = new Material(Shader.Find("Standard"));
            SetTransparent(glowMat);
            glowMat.color = new Color(LorQBConfig.EmissiveColors[0].r,
                                      LorQBConfig.EmissiveColors[0].g,
                                      LorQBConfig.EmissiveColors[0].b, 0.10f);
            gmr.material = glowMat;
            glowGo.transform.localScale = Vector3.one * 1.6f;

            // Point light on ball
            var lightGo = new GameObject("BallLight");
            lightGo.transform.SetParent(go.transform, false);
            var pl = lightGo.AddComponent<Light>();
            pl.type      = LightType.Point;
            pl.color     = LorQBConfig.EmissiveColors[0];
            pl.intensity = 1.2f;
            pl.range     = LorQBConfig.CUBE_SIZE * 2.5f;

            var ctrl = go.AddComponent<LorQBBallController>();
            ctrl.BallRenderer = mr;
            ctrl.GlowMat      = glowMat;
            ctrl.BallLight    = pl;

            return ctrl;
        }

        // ── Lighting ──────────────────────────────────────────────────────────
        void BuildLighting()
        {
            RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.15f, 0.18f, 0.25f);

            var dlGo = new GameObject("DirectionalLight");
            var dl   = dlGo.AddComponent<Light>();
            dl.type      = LightType.Directional;
            dl.color     = new Color(1f, 0.95f, 0.85f);
            dl.intensity = 0.9f;
            dlGo.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

            var fillGo = new GameObject("FillLight");
            var fill   = fillGo.AddComponent<Light>();
            fill.type      = LightType.Directional;
            fill.color     = new Color(0.3f, 0.4f, 0.7f);
            fill.intensity = 0.4f;
            fillGo.transform.rotation = Quaternion.Euler(-20f, 150f, 0f);
        }

        // ── Camera ────────────────────────────────────────────────────────────
        void BuildCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.12f);
            cam.fieldOfView     = 48f;

            // Angled 3D view — matches Blender reference (above, left-of-centre)
            cam.transform.position = new Vector3(-2.0f, 4.5f, -9.0f);
            cam.transform.LookAt(new Vector3(0f, 0f, 0f));
        }

        // ── Canvas / HUD ──────────────────────────────────────────────────────
        void BuildUI(LorQBGameManager gm)
        {
            // Minimal UI: LorQBGameManager.OnGUI() handles the HUD overlay.
            // For a production build, replace with a proper Canvas setup.
        }

        // ── Material helpers ──────────────────────────────────────────────────
        Material MakeGlassMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Standard"));
            SetTransparent(mat);
            // 28% opacity — glass look matching Blender reference
            mat.color = new Color(color.r, color.g, color.b, 0.28f);
            mat.SetFloat("_Glossiness", 0.96f);
            mat.SetFloat("_Metallic",   0.0f);
            // Subtle inner emission so faces read even at low alpha
            mat.SetColor("_EmissionColor", color * 0.08f);
            mat.EnableKeyword("_EMISSION");
            return mat;
        }

        Material MakeBallMaterial(Color emissive)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            mat.SetFloat("_Glossiness", 0.88f);
            mat.SetFloat("_Metallic",   0.3f);
            mat.SetColor("_EmissionColor", emissive * 1.2f);
            mat.EnableKeyword("_EMISSION");
            return mat;
        }

        Material MakeMetal()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.6f, 0.6f, 0.65f);
            mat.SetFloat("_Glossiness", 0.78f);
            mat.SetFloat("_Metallic",   0.9f);
            return mat;
        }

        static void SetTransparent(Material mat)
        {
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetFloat("_Mode", 3);   // Transparent
            mat.SetInt("_SrcBlend",  (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend",  (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite",    0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        // BuildEdges and MakeBoxMesh removed — hollow cube now built from face panels
    }
}
