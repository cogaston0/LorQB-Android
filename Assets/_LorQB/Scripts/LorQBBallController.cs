// LorQBBallController.cs  (Blender 5.1.1 reference build)
// Ball arc movement, color blending, drag support, and outside-reset.
// Per design spec: "At the beginning of the game or new turn,
// the ball is always outside the cubes."
// Place in: Assets/LorQB/Scripts/

using System.Collections;
using UnityEngine;

namespace LorQB
{
    public class LorQBBallController : MonoBehaviour
    {
        [HideInInspector] public MeshRenderer BallRenderer;
        [HideInInspector] public Material     GlowMat;
        [HideInInspector] public Light        BallLight;

        // Whether the ball is currently being dragged by the player
        public bool IsDragging { get; private set; } = false;

        private Camera _cam;
        private float  _dragDepth;   // camera-space depth captured at drag start

        void Start()
        {
            _cam = Camera.main;
        }

        // ── Reset to outside position (new turn) ───────────────────────────────

        public void ResetToOutside()
        {
            StopAllCoroutines();
            IsDragging = false;
            transform.position = LorQBConfig.BallStartPos;
            SetOwnerColor(new Color(0.8f, 0.8f, 0.8f)); // neutral grey outside
        }

        // ── Drag support ──────────────────────────────────────────────────────

        /// <summary>Begin dragging the ball (called by InputHandler on ball touch).</summary>
        public void StartDrag()
        {
            StopAllCoroutines();
            IsDragging = true;
            // Capture the ball's camera-space depth so drag stays in the correct world plane
            if (_cam != null)
                _dragDepth = _cam.WorldToScreenPoint(transform.position).z;
        }

        /// <summary>Update drag position to follow finger/mouse at the ball's captured depth.</summary>
        public void UpdateDrag(Vector2 screenPos)
        {
            if (!IsDragging || _cam == null) return;
            Vector3 world = _cam.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, _dragDepth));
            transform.position = world;
        }

        /// <summary>End drag — GameManager decides what happens based on landing position.</summary>
        public void EndDrag()
        {
            IsDragging = false;
        }

        // ── Arc to seat ────────────────────────────────────────────────────────

        public IEnumerator ArcToSeat(Vector3 destSeat, Color destColor, float duration)
        {
            IsDragging = false;
            Vector3 startPos   = transform.position;
            Color   startEmit  = BallRenderer.material.GetColor("_EmissionColor");
            Color   startGlow  = GlowMat.color;
            Color   startLight = BallLight.color;

            float dist = Vector3.Distance(startPos, destSeat);
            float arcH = Mathf.Clamp(dist * 0.55f, 0.2f, LorQBConfig.CUBE_SIZE * 0.8f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t     = Mathf.Clamp01(elapsed / duration);
                float tEase = EaseInOut(t);
                float arcY  = Mathf.Sin(t * Mathf.PI) * arcH;

                Vector3 flat = Vector3.Lerp(startPos, destSeat, tEase);
                transform.position = new Vector3(flat.x, flat.y + arcY, flat.z);

                Color emit = Color.Lerp(startEmit, destColor * 1.2f, tEase);
                BallRenderer.material.SetColor("_EmissionColor", emit);
                GlowMat.color   = Color.Lerp(startGlow,  new Color(destColor.r, destColor.g, destColor.b, 0.10f), tEase);
                BallLight.color = Color.Lerp(startLight, destColor, tEase);

                yield return null;
            }

            transform.position = destSeat;
            SetOwnerColor(destColor);
            yield return StartCoroutine(Settle(destSeat));
        }

        IEnumerator Settle(Vector3 seat)
        {
            float t = 0f, dur = LorQBConfig.SETTLE_DURATION;
            while (t < dur)
            {
                t += Time.deltaTime;
                float bounce = Mathf.Abs(Mathf.Sin(t / dur * Mathf.PI * 2.5f)) * 0.06f * (1f - t / dur);
                transform.position = seat + new Vector3(0, bounce, 0);
                yield return null;
            }
            transform.position = seat;
        }

        // ── Color ─────────────────────────────────────────────────────────────

        public void SetOwnerColor(Color color)
        {
            BallRenderer.material.SetColor("_EmissionColor", color * 1.2f);
            GlowMat.color   = new Color(color.r, color.g, color.b, 0.10f);
            BallLight.color = color;
        }

        static float EaseInOut(float t) =>
            t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
    }
}
