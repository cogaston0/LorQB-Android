using UnityEngine;
using LorQB.Core;
using LorQB.Movement;

namespace LorQB.Validation
{
    /// <summary>
    /// Validates player release actions and decides whether a ball transfer is allowed.
    /// Attach this component to the GameManager GameObject.
    /// </summary>
    public class ValidationController : MonoBehaviour
    {
        // ── Fields ───────────────────────────────────────────────────────────────
        private SequenceManager _sequenceManager;
        private BallTransferController _ballTransfer;

        // ── Public API ───────────────────────────────────────────────────────────
        /// <summary>
        /// Wire the SequenceManager and BallTransferController dependencies.
        /// Must be called once before the first round begins (e.g. from GameManager).
        /// </summary>
        public void Initialise(SequenceManager sequenceManager, BallTransferController ballTransfer)
        {
            _sequenceManager = sequenceManager;
            _ballTransfer    = ballTransfer;
        }

        /// <summary>
        /// Validates a player release from <paramref name="from"/> to <paramref name="to"/>
        /// and calls <see cref="BallTransferController.TryTransfer"/> if all checks pass.
        /// </summary>
        /// <param name="from">The cube color the player released from.</param>
        /// <param name="to">The cube color the player released toward.</param>
        public void ValidateRelease(CubeColor from, CubeColor to)
        {
            GameStateManager gsm = GameStateManager.Instance;

            // Step 1 — Must be ACTIVE_PLAY.
            if (gsm == null || gsm.GetState() != GameStateManager.GameState.ACTIVE_PLAY)
            {
                Debug.Log($"[ValidationController] ValidateRelease ignored: state is not ACTIVE_PLAY.");
                return;
            }

            // Step 2 — Move to VALIDATION.
            gsm.SetState(GameStateManager.GameState.VALIDATION);

            // Step 3 — Validate sequence.
            bool sequenceValid =
                _sequenceManager != null &&
                _sequenceManager.HasNextColor() &&
                from == _sequenceManager.GetCurrentColor() &&
                to   == _sequenceManager.GetNextColor();

            if (!sequenceValid)
            {
                Debug.Log($"[ValidationController] Invalid release: from={from}, to={to}. Returning to ACTIVE_PLAY.");
                gsm.SetState(GameStateManager.GameState.ACTIVE_PLAY);
                return;
            }

            // Step 4 — Call transfer.
            bool result = _ballTransfer.TryTransfer(from, to);

            // Step 5 — If result false, return to ACTIVE_PLAY.
            if (!result)
            {
                Debug.Log($"[ValidationController] Transfer rejected by BallTransferController: from={from}, to={to}. Returning to ACTIVE_PLAY.");
                gsm.SetState(GameStateManager.GameState.ACTIVE_PLAY);
                return;
            }

            // Step 6 — Transfer succeeded; BallTransferController handles state.
            Debug.Log($"[ValidationController] Transfer accepted: from={from}, to={to}.");
        }
    }
}
