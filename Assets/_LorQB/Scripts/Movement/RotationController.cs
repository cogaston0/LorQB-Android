using UnityEngine;
using LorQB.Core;

namespace LorQB.Movement
{
    /// <summary>
    /// Handles cube rotation based on player drag input.
    /// No physics, no automatic rotation, no snapping — the player controls full motion.
    /// Rotation is only permitted while GameState == ACTIVE_PLAY.
    /// </summary>
    public class RotationController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 120f;

        // ── State ────────────────────────────────────────────────────────────────
        private Transform rotationRoot;
        private float     currentAngle;
        private bool      isRotating;

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Begin tracking a drag rotation on the given pivot Transform.
        /// Only allowed when GameState == ACTIVE_PLAY.
        /// </summary>
        /// <param name="root">The pivot Transform to rotate around.</param>
        public void StartRotation(Transform root)
        {
            GameStateManager gsm = GameStateManager.Instance;
            if (gsm == null || gsm.GetState() != GameStateManager.GameState.ACTIVE_PLAY)
            {
                Debug.LogWarning("[RotationController] StartRotation blocked: GameState is not ACTIVE_PLAY.");
                return;
            }

            rotationRoot = root;
            isRotating   = true;
            currentAngle = 0f;

            Debug.Log("[RotationController] Rotation started.");
        }

        /// <summary>
        /// Apply a player-driven rotation delta this frame.
        /// Only executes while isRotating is true.
        /// Clamps cumulative angle to [0, 180] degrees.
        /// </summary>
        /// <param name="delta">Raw drag delta in degrees (scaled by rotationSpeed externally or here).</param>
        public void UpdateRotation(float delta)
        {
            if (!isRotating) return;

            float scaled   = delta * rotationSpeed * Time.deltaTime;
            float newAngle = Mathf.Clamp(currentAngle + scaled, 0f, 180f);

            // Only apply the portion that keeps us within [0, 180].
            float clampedDelta = newAngle - currentAngle;

            if (Mathf.Approximately(clampedDelta, 0f)) return;

            // Placeholder axis — replace with the correct per-pivot axis when wiring.
            Vector3 axis = Vector3.forward;

            if (rotationRoot != null)
            {
                transform.RotateAround(rotationRoot.position, axis, clampedDelta);
            }
            else
            {
                transform.Rotate(axis, clampedDelta, Space.World);
            }

            currentAngle = newAngle;

            Debug.Log($"[RotationController] Angle updated: {currentAngle:F2}°");
        }

        /// <summary>
        /// Stop the current rotation session.
        /// </summary>
        public void EndRotation()
        {
            isRotating = false;
            Debug.Log($"[RotationController] Rotation ended at {currentAngle:F2}°.");
        }
    }
}
