// LorQBCubeNode.cs  (Blender 5.1.1 reference build)
// Per-cube component: identity, visual state, glow.
// Added: NextTarget state (pulsing highlight showing player where to go).
// Place in: Assets/LorQB/Scripts/

using UnityEngine;

namespace LorQB
{
    public enum CubeState { Idle, BallInside, Transferring, NextTarget, Win }

    public class LorQBCubeNode : MonoBehaviour
    {
        public CubeId       Id;
        public MeshRenderer Renderer;

        private Material  _mat;
        private Color     _baseColor;
        private Color     _emissive;
        private CubeState _state = CubeState.Idle;
        private float     _t     = 0f;

        void Start()
        {
            int idx    = (int)Id;
            _baseColor = LorQBConfig.CubeColors[idx];
            _emissive  = LorQBConfig.EmissiveColors[idx];
            _mat       = Renderer.material;
            ApplyState();
        }

        void Update()
        {
            _t += Time.deltaTime;

            switch (_state)
            {
                case CubeState.BallInside:
                {
                    float a = _baseColor.a + Mathf.Sin(_t * 2.2f) * 0.08f;
                    _mat.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, Mathf.Clamp(a, 0.10f, 0.55f));
                    _mat.SetColor("_EmissionColor", _emissive * (0.5f + Mathf.Sin(_t * 1.8f) * 0.2f));
                    break;
                }
                case CubeState.NextTarget:
                {
                    // Bright attention pulse — this is where the ball must go
                    float pulse = 0.5f + 0.5f * Mathf.Sin(_t * 5f);
                    _mat.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.15f + pulse * 0.3f);
                    _mat.SetColor("_EmissionColor", _emissive * (0.4f + pulse * 0.8f));
                    break;
                }
                case CubeState.Win:
                {
                    float pulse = 0.5f + 0.5f * Mathf.Sin(_t * 4f);
                    _mat.color = new Color(1f, 1f, 1f, 0.25f + pulse * 0.25f);
                    _mat.SetColor("_EmissionColor", _emissive * (0.8f + pulse * 0.6f));
                    break;
                }
            }
        }

        public void SetState(CubeState state)
        {
            _state = state;
            _t     = 0f;
            ApplyState();
        }

        void ApplyState()
        {
            if (_mat == null) return;
            switch (_state)
            {
                case CubeState.Idle:
                    _mat.color = _baseColor;
                    _mat.SetColor("_EmissionColor", Color.black);
                    _mat.DisableKeyword("_EMISSION");
                    break;
                case CubeState.BallInside:
                    _mat.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.32f);
                    _mat.SetColor("_EmissionColor", _emissive * 0.5f);
                    _mat.EnableKeyword("_EMISSION");
                    break;
                case CubeState.NextTarget:
                    _mat.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.38f);
                    _mat.SetColor("_EmissionColor", _emissive * 0.9f);
                    _mat.EnableKeyword("_EMISSION");
                    break;
                case CubeState.Transferring:
                    _mat.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.45f);
                    _mat.SetColor("_EmissionColor", _emissive * 0.9f);
                    _mat.EnableKeyword("_EMISSION");
                    break;
                case CubeState.Win:
                    _mat.color = new Color(1f, 1f, 1f, 0.40f);
                    _mat.SetColor("_EmissionColor", _emissive * 1.2f);
                    _mat.EnableKeyword("_EMISSION");
                    break;
            }
        }
    }
}
