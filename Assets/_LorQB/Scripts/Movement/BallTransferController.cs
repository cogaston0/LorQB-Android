using System;
using System.Collections.Generic;
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
    ///   - Transfer is ONLY permitted when GameState == TRANSFER (i.e. after a valid
    ///     player release confirmed by the Validation System) and holes are aligned.
    ///
    /// Attach this component to the GameManager GameObject.
    /// </summary>
    public class BallTransferController : MonoBehaviour
    {
        // ── Serializable seat-map entry ───────────────────────────────────────────
        [Serializable]
        private class SeatEntry
        {
            public CubeColor color;
            public Transform seat;
        }

        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("Ball")]
        [SerializeField] private Transform ballTransform;

        [Header("Cube Seats")]
        [Tooltip("Map each CubeColor to its Seat_<Color> Transform. Converted to a Dictionary at runtime.")]
        [SerializeField] private List<SeatEntry> seatEntries = new List<SeatEntry>();

        [Header("Hole Detectors")]
        [Tooltip("Ordered to match each sequence transition: index 0 = step 0→1, 1 = step 1→2, 2 = step 2→3.")]
        [SerializeField] private HoleAlignmentDetector[] holeDetectors;

        // ── Dependencies ─────────────────────────────────────────────────────────
        private SequenceManager _sequenceManager;

        // ── Runtime state ─────────────────────────────────────────────────────────
        private Dictionary<CubeColor, Transform> _seatMap;

        // Local mirror of SequenceManager.sequenceIndex. SequenceManager does not expose
        // its index publicly, so this field is required for HoleAlignmentDetector indexing
        // and the OnBallTransferred event argument. Keep it in sync via AdvanceSequence().
        private int  _currentStepIndex;
        private bool _isTransferring;

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>Fired immediately before a ball transfer begins.</summary>
        public event Action OnTransferStarted;

        /// <summary>Fired after the ball has snapped to its new seat. Argument = new step index.</summary>
        public event Action<int> OnBallTransferred;

        /// <summary>Fired when the ball reaches the final cube in the sequence.</summary>
        public event Action OnLevelComplete;

        // ── Properties ───────────────────────────────────────────────────────────
        public bool IsTransferring   => _isTransferring;
        public int  CurrentStepIndex => _currentStepIndex;

        /// <summary>The CubeColor currently holding the ball.</summary>
        public CubeColor CurrentCube => _sequenceManager != null
            ? _sequenceManager.GetCurrentColor()
            : CubeColor.Blue;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            BuildSeatMap();
        }

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>
        /// Wire the SequenceManager dependency and reset ball to the starting seat.
        /// Must be called once before the first round begins (e.g. from GameManager).
        /// </summary>
        public void SetSequenceManager(SequenceManager sequenceManager)
        {
            _sequenceManager  = sequenceManager;
            _currentStepIndex = 0;
            _isTransferring   = false;
            InitialisePosition();
        }

        /// <summary>
        /// Attempt to transfer the ball to the next cube in the sequence.
        /// No-ops silently if CanTransfer() is false.
        /// </summary>
        public void TriggerTransfer()
        {
            if (!CanTransfer()) return;

            ExecuteTransfer();
        }

        /// <summary>
        /// Validates and executes a ball transfer from one cube color to another.
        /// Must be called while GameState == VALIDATION. On success, transitions the
        /// state machine: VALIDATION → TRANSFER → POST_TRANSFER (→ ROUND_COMPLETE
        /// when the sequence is exhausted).
        /// </summary>
        /// <param name="from">The color the ball is currently on.</param>
        /// <param name="to">The color to transfer the ball to.</param>
        /// <returns>True if the transfer was performed; false otherwise.</returns>
        public bool TryTransfer(CubeColor from, CubeColor to)
        {
            Debug.Log($"[Transfer Attempt] {from} → {to}");

            GameStateManager gsm = GameStateManager.Instance;

            if (gsm == null)
            {
                Debug.LogWarning("[BallTransferController] TryTransfer: GameStateManager.Instance is null.");
                Debug.LogWarning("[Transfer Invalid]");
                return false;
            }

            if (gsm.GetState() != GameStateManager.GameState.VALIDATION)
            {
                Debug.LogWarning($"[BallTransferController] TryTransfer: GameState is {gsm.GetState()}, expected VALIDATION.");
                Debug.LogWarning("[Transfer Invalid]");
                return false;
            }

            if (_sequenceManager == null)
            {
                Debug.LogWarning("[BallTransferController] TryTransfer: _sequenceManager is null.");
                Debug.LogWarning("[Transfer Invalid]");
                return false;
            }

            if (!_sequenceManager.HasNextColor())
            {
                Debug.LogWarning("[BallTransferController] TryTransfer: no next color in sequence.");
                Debug.LogWarning("[Transfer Invalid]");
                return false;
            }

            if (from != _sequenceManager.GetCurrentColor())
            {
                Debug.LogWarning($"[BallTransferController] TryTransfer: 'from' color {from} does not match current sequence color {_sequenceManager.GetCurrentColor()}.");
                Debug.LogWarning("[Transfer Invalid]");
                return false;
            }

            if (to != _sequenceManager.GetNextColor())
            {
                Debug.LogWarning($"[BallTransferController] TryTransfer: 'to' color {to} does not match next sequence color {_sequenceManager.GetNextColor()}.");
                Debug.LogWarning("[Transfer Invalid]");
                return false;
            }

            if (!_seatMap.ContainsKey(to))
            {
                Debug.LogWarning($"[BallTransferController] TryTransfer: _seatMap does not contain an entry for target color {to}.");
                Debug.LogWarning("[Transfer Invalid]");
                return false;
            }

            // All checks passed — execute the transfer.
            gsm.SetState(GameStateManager.GameState.TRANSFER);

            ballTransform.position = _seatMap[to].position;
            _sequenceManager.AdvanceSequence();
            _currentStepIndex++;
            OnBallTransferred?.Invoke(_currentStepIndex);

            gsm.SetState(GameStateManager.GameState.POST_TRANSFER);

            if (!_sequenceManager.HasNextColor())
            {
                Debug.Log("[ROUND COMPLETE TRIGGERED]");
                OnLevelComplete?.Invoke();
                gsm.SetState(GameStateManager.GameState.ROUND_COMPLETE);
            }

            Debug.Log("[Transfer Success]");
            return true;
        }

        /// <summary>
        /// Returns true when a transfer is currently permitted.
        /// All conditions must hold: GameState == TRANSFER, sequence has a next step,
        /// ball is not already transferring, seats exist for both current and next colors,
        /// and holes are aligned.
        /// </summary>
        public bool CanTransfer()
        {
            if (_isTransferring)        return false;
            if (_sequenceManager == null) return false;
            if (!_sequenceManager.HasNextColor()) return false;

            // Gate: only transfer after a confirmed valid release (state must be TRANSFER).
            GameStateManager gsm = GameStateManager.Instance;
            if (gsm == null) return false;
            if (gsm.GetState() != GameStateManager.GameState.TRANSFER) return false;

            // Require seats for both current and next color.
            CubeColor currentColor = _sequenceManager.GetCurrentColor();
            CubeColor nextColor    = _sequenceManager.GetNextColor();

            if (!_seatMap.ContainsKey(currentColor))
            {
                Debug.LogWarning($"[BallTransferController] CanTransfer: _seatMap missing entry for current color {currentColor}.");
                return false;
            }

            if (!_seatMap.ContainsKey(nextColor))
            {
                Debug.LogWarning($"[BallTransferController] CanTransfer: _seatMap missing entry for target color {nextColor}.");
                return false;
            }

            HoleAlignmentDetector detector = GetDetectorForCurrentPair();
            if (detector == null)
            {
                Debug.LogWarning($"[BallTransferController] CanTransfer: no HoleAlignmentDetector for step {_currentStepIndex}.");
                return false;
            }

            if (!detector.AreHolesAligned()) return false;

            return true;
        }

        // ── Private helpers ───────────────────────────────────────────────────────
        private void BuildSeatMap()
        {
            _seatMap = new Dictionary<CubeColor, Transform>();
            foreach (SeatEntry entry in seatEntries)
            {
                if (entry.seat == null) continue;

                if (_seatMap.ContainsKey(entry.color))
                {
                    Debug.LogWarning($"[BallTransferController] Duplicate seat entry for color {entry.color}; ignoring.");
                    continue;
                }

                _seatMap[entry.color] = entry.seat;
            }
        }

        private void InitialisePosition()
        {
            if (ballTransform == null || _sequenceManager == null || _seatMap == null) return;

            _currentStepIndex = 0;
            SnapBallToCurrentSeat();
        }

        private void ExecuteTransfer()
        {
            _isTransferring = true;
            OnTransferStarted?.Invoke();

            // Advance sequence state and mirror locally for detector indexing.
            _sequenceManager.AdvanceSequence();
            _currentStepIndex++;

            // Seat-based snap: instantly move ball to the target seat — no physics.
            SnapBallToCurrentSeat();

            _isTransferring = false;
            OnBallTransferred?.Invoke(_currentStepIndex);

            if (!_sequenceManager.HasNextColor())
            {
                OnLevelComplete?.Invoke();
            }
        }

        private void SnapBallToCurrentSeat()
        {
            if (ballTransform == null || _seatMap == null || _sequenceManager == null) return;

            CubeColor color = _sequenceManager.GetCurrentColor();
            if (_seatMap.TryGetValue(color, out Transform seat))
            {
                ballTransform.position = seat.position;
            }
            else
            {
                Debug.LogWarning($"[BallTransferController] SnapBallToCurrentSeat: _seatMap missing entry for color {color}.");
            }
        }

        /// <summary>
        /// Returns the HoleAlignmentDetector for the active transition pair.
        /// Detectors are ordered to match sequence pair indices (0 = step 0→1, etc.).
        /// </summary>
        private HoleAlignmentDetector GetDetectorForCurrentPair()
        {
            if (holeDetectors == null || holeDetectors.Length == 0) return null;

            if (_currentStepIndex < holeDetectors.Length)
                return holeDetectors[_currentStepIndex];

            return null;
        }
    }
}
