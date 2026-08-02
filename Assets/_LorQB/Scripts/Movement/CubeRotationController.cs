using System;
using UnityEngine;

namespace LorQB.Movement
{
    /// <summary>
    /// Rotates a cube around its hinge pivot using scripted transform-based movement.
    /// No Rigidbody, no physics joints.
    ///
    /// Attach to each rotatable cube (Cube_Red, Cube_Green, Cube_Yellow).
    /// Cube_Blue is the anchor and does not get this component.
    /// </summary>
    public class CubeRotationController : MonoBehaviour
    {
        // ── Constants ────────────────────────────────────────────────────────────
        private const float RotationIncrement = 90f;

        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("Hinge Setup")]
        [Tooltip("The Pivot_[ColorA]_[ColorB] Transform that acts as the rotation centre.")]
        [SerializeField] private Transform pivotTransform;

        [Tooltip("World-space axis around which this cube rotates (Z or X as per scene plan).")]
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;

        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 90f;   // degrees per second
        [SerializeField] private float[] allowedAngles = { 0f, 90f, 180f, 270f };
        [SerializeField] private float snapThreshold  = 0.5f; // degrees — completes rotation

        // ── State ────────────────────────────────────────────────────────────────
        private float _currentAngle;  // running angle around hinge axis
        private float _targetAngle;
        private bool  _isRotating;
        private bool  _isLocked;      // cube is locked by long-press

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>Fired when a rotation begins.</summary>
        public event Action OnRotationStarted;

        /// <summary>Fired when the cube reaches its target angle (within snapThreshold).</summary>
        public event Action OnRotationComplete;

        // ── Properties ───────────────────────────────────────────────────────────
        public bool IsRotating => _isRotating;
        public bool IsLocked   => _isLocked;

        /// <summary>Current angle (degrees) accumulated around the hinge axis.</summary>
        public float CurrentAngle => _currentAngle;

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>
        /// Initiates a smooth rotation to the target angle.
        /// Blocked when the cube is locked or already rotating.
        /// </summary>
        public void RotateTo(float targetAngle)
        {
            if (_isLocked || _isRotating) return;

            _targetAngle = NormalizeAngle(SnapToAllowed(targetAngle));
            _isRotating  = true;
            OnRotationStarted?.Invoke();
        }

        /// <summary>Instantly snap the cube to the nearest allowed angle.</summary>
        public void SnapToNearest()
        {
            float snapped = SnapToAllowed(_currentAngle);
            float delta   = Mathf.DeltaAngle(_currentAngle, snapped);
            ApplyDelta(delta);
            _currentAngle = snapped;
            _targetAngle  = snapped;
            _isRotating   = false;
        }

        /// <summary>Rotate clockwise by 90 degrees (from the player's perspective).</summary>
        public void RotateForward()  => RotateTo(_currentAngle + RotationIncrement);

        /// <summary>Rotate counter-clockwise by 90 degrees.</summary>
        public void RotateBackward() => RotateTo(_currentAngle - RotationIncrement);

        /// <summary>Toggle the rotation lock on this cube (long-press mechanic).</summary>
        public void ToggleLock() => _isLocked = !_isLocked;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Update()
        {
            if (!_isRotating) return;

            float delta    = rotationSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, delta);
            float angleDelta = newAngle - _currentAngle;

            ApplyDelta(angleDelta);
            _currentAngle = newAngle;

            if (Mathf.Abs(Mathf.DeltaAngle(_currentAngle, _targetAngle)) < snapThreshold)
            {
                // Snap precisely to target
                float remaining = Mathf.DeltaAngle(_currentAngle, _targetAngle);
                ApplyDelta(remaining);
                _currentAngle = _targetAngle;
                _isRotating   = false;
                OnRotationComplete?.Invoke();
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────────
        /// <summary>Apply a delta rotation around the hinge pivot to this cube's transform.</summary>
        private void ApplyDelta(float deltaAngle)
        {
            if (pivotTransform == null)
            {
                transform.Rotate(rotationAxis, deltaAngle, Space.World);
            }
            else
            {
                transform.RotateAround(pivotTransform.position, rotationAxis, deltaAngle);
            }
        }

        /// <summary>Find the nearest angle in allowedAngles to the given angle.</summary>
        private float SnapToAllowed(float angle)
        {
            if (allowedAngles == null || allowedAngles.Length == 0) return angle;

            float best = allowedAngles[0];
            float bestDiff = Mathf.Abs(Mathf.DeltaAngle(angle, allowedAngles[0]));

            for (int i = 1; i < allowedAngles.Length; i++)
            {
                float diff = Mathf.Abs(Mathf.DeltaAngle(angle, allowedAngles[i]));
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    best     = allowedAngles[i];
                }
            }

            return best;
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f) angle += 360f;
            return angle;
        }
    }
}
