using System;
using UnityEngine;

namespace LorQB.Movement
{
    /// <summary>
    /// Detects when the holes of two adjacent cubes are aligned within an angular threshold.
    ///
    /// Each cube must have a child empty GameObject named "HoleAxis" whose forward vector
    /// points along the hole's travel axis. Alignment is measured as the angle between the
    /// two HoleAxis.forward vectors in world space.
    ///
    /// Attach one HoleAlignmentDetector to each Pivot (Pivot_Blue_Red, Pivot_Red_Green,
    /// Pivot_Green_Yellow), then assign the two adjacent cube HoleAxis transforms.
    /// </summary>
    public class HoleAlignmentDetector : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("Hole Axis Markers")]
        [Tooltip("HoleAxis child of the left/anchor cube.")]
        [SerializeField] private Transform holeAxisA;

        [Tooltip("HoleAxis child of the right/rotating cube.")]
        [SerializeField] private Transform holeAxisB;

        [Header("Detection Settings")]
        [SerializeField] private float alignmentThreshold = 5f;  // degrees

        // ── State ────────────────────────────────────────────────────────────────
        private bool _wasAligned;

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>
        /// Fired once per alignment event (rising edge).
        /// Not spammed every frame — only fires on the frame alignment first occurs.
        /// </summary>
        public event Action OnHolesAligned;

        /// <summary>Fired once when holes go out of alignment (falling edge).</summary>
        public event Action OnHolesMisaligned;

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>Returns true when the two hole axes are within the alignment threshold.</summary>
        public bool AreHolesAligned() => GetAngularOffset() <= alignmentThreshold;

        /// <summary>Angular difference (degrees) between the two hole forward vectors in world space.</summary>
        public float GetAngularOffset()
        {
            if (holeAxisA == null || holeAxisB == null) return 180f;
            return Vector3.Angle(holeAxisA.forward, holeAxisB.forward);
        }

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Update()
        {
            bool aligned = AreHolesAligned();

            if (aligned && !_wasAligned)
            {
                _wasAligned = true;
                OnHolesAligned?.Invoke();
            }
            else if (!aligned && _wasAligned)
            {
                _wasAligned = false;
                OnHolesMisaligned?.Invoke();
            }
        }
    }
}
