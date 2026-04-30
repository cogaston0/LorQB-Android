using System;
using UnityEngine;
using LorQB.Core;

namespace LorQB.Movement
{
    /// <summary>
    /// Manages seat-based ball transfer between cubes.
    ///
    /// Transfer model (no physics):
    ///   - The ball has no Rigidbody.
    ///   - Each cube has a Seat_[Color] Transform at the top-center of the cube.
    ///   - When a transfer is triggered, the ball's world position is instantly snapped
    ///     to the target Seat's world position.
    ///   - Transfer is only valid when the next cube in the shuffle order is adjacent
    ///     and its holes are aligned with the current cube.
    ///
    /// Attach this component to the GameManager GameObject.
    /// </summary>
    public class BallTransferController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("Ball")]
        [SerializeField] private Transform ballTransform;

        [Header("Cube Seats")]
        [Tooltip("Seat transforms in CubeColor enum order: Blue(0), Red(1), Green(2), Yellow(3).")]
        [SerializeField] private Transform[] seatTransforms = new Transform[4];

        [Header("Shuffle")]
        [Tooltip("Array of CubeColor values representing the required visit order (Blue always first).")]
        [SerializeField] private CubeColor[] shuffleOrder;

        [Header("Hole Detectors")]
        [Tooltip("Hole detectors in the same order as each adjacent pair defined by shuffleOrder.")]
        [SerializeField] private HoleAlignmentDetector[] holeDetectors;

        // ── State ────────────────────────────────────────────────────────────────
        private int  _currentStepIndex; // index into shuffleOrder of the cube currently holding the ball
        private bool _isTransferring;

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>Fired immediately before a ball transfer begins.</summary>
        public event Action OnTransferStarted;

        /// <summary>Fired after the ball has snapped to its new seat. Argument = new step index.</summary>
        public event Action<int> OnBallTransferred;

        /// <summary>Fired when the ball reaches the final cube in the shuffle order.</summary>
        public event Action OnLevelComplete;

        // ── Properties ───────────────────────────────────────────────────────────
        public bool IsTransferring   => _isTransferring;
        public int  CurrentStepIndex => _currentStepIndex;

        /// <summary>The CubeColor currently holding the ball.</summary>
        public CubeColor CurrentCube => shuffleOrder != null && shuffleOrder.Length > 0
            ? shuffleOrder[_currentStepIndex]
            : CubeColor.Blue;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Start()
        {
            InitialisePosition();
        }

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>
        /// Set the shuffle order and reset ball to the starting position.
        /// Called by ShuffleOrderGenerator after generating a new order.
        /// </summary>
        public void SetShuffleOrder(CubeColor[] order)
        {
            shuffleOrder       = order;
            _currentStepIndex  = 0;
            _isTransferring    = false;
            InitialisePosition();
        }

        /// <summary>
        /// Attempt to transfer the ball to the next cube in the shuffle order.
        /// Silently no-ops if: already transferring, ball is at last step, or holes are not aligned.
        /// Called externally by HoleAlignmentDetector.OnHolesAligned or by other game logic.
        /// </summary>
        public void TriggerTransfer()
        {
            if (!CanTransfer()) return;

            int nextStep = _currentStepIndex + 1;
            ExecuteTransfer(nextStep);
        }

        /// <summary>
        /// Returns true when a transfer to the next cube is currently possible.
        /// Conditions: not already transferring, not at the final step, holes are aligned.
        /// </summary>
        public bool CanTransfer()
        {
            if (_isTransferring)                  return false;
            if (shuffleOrder == null)             return false;
            if (_currentStepIndex >= shuffleOrder.Length - 1) return false;

            // Check hole alignment for the pair (currentStep → currentStep+1)
            HoleAlignmentDetector detector = GetDetectorForCurrentPair();
            if (detector != null && !detector.AreHolesAligned()) return false;

            return true;
        }

        // ── Private helpers ───────────────────────────────────────────────────────
        private void InitialisePosition()
        {
            if (ballTransform == null || shuffleOrder == null || shuffleOrder.Length == 0) return;

            _currentStepIndex = 0;
            SnapBallToCurrentSeat();
        }

        private void ExecuteTransfer(int nextStep)
        {
            _isTransferring = true;
            OnTransferStarted?.Invoke();

            _currentStepIndex = nextStep;

            // Seat-based snap: instantly move ball to the target seat position — no physics.
            SnapBallToCurrentSeat();

            _isTransferring = false;
            OnBallTransferred?.Invoke(_currentStepIndex);

            if (_currentStepIndex >= shuffleOrder.Length - 1)
            {
                OnLevelComplete?.Invoke();
            }
        }

        private void SnapBallToCurrentSeat()
        {
            if (ballTransform == null) return;

            Transform seat = GetSeatForStep(_currentStepIndex);
            if (seat != null)
            {
                ballTransform.position = seat.position;
            }
        }

        /// <summary>Returns the seat Transform for a given step index into the shuffle order.</summary>
        private Transform GetSeatForStep(int stepIndex)
        {
            if (shuffleOrder == null || stepIndex < 0 || stepIndex >= shuffleOrder.Length)
                return null;

            CubeColor color = shuffleOrder[stepIndex];
            int colorIndex  = (int)color;

            if (seatTransforms == null || colorIndex >= seatTransforms.Length)
                return null;

            return seatTransforms[colorIndex];
        }

        /// <summary>
        /// Returns the HoleAlignmentDetector responsible for the transition
        /// from the current cube to the next cube in the shuffle order.
        /// Detectors are expected to be ordered to match shuffle pair indices (0=step0→1, etc.).
        /// </summary>
        private HoleAlignmentDetector GetDetectorForCurrentPair()
        {
            if (holeDetectors == null || holeDetectors.Length == 0) return null;

            // Use the detector at the current step index if available
            if (_currentStepIndex < holeDetectors.Length)
                return holeDetectors[_currentStepIndex];

            return null;
        }
    }
}
