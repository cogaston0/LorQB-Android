using UnityEngine;

namespace LorQB.Core
{
    /// <summary>
    /// Detects when a round is complete and transitions to ROUND_COMPLETE state.
    /// Attach this component to the GameManager GameObject.
    /// </summary>
    public class LevelCompletionController : MonoBehaviour
    {
        // ── Fields ───────────────────────────────────────────────────────────────
        private SequenceManager _sequenceManager;
        private GameStateManager _gameStateManager;

        // ── Public API ───────────────────────────────────────────────────────────
        /// <summary>
        /// Wire the SequenceManager and GameStateManager dependencies.
        /// Must be called once before the first round begins (e.g. from GameManager).
        /// </summary>
        public void Initialise(SequenceManager sequenceManager, GameStateManager gameStateManager)
        {
            _sequenceManager  = sequenceManager;
            _gameStateManager = gameStateManager;
        }

        public void CheckCompletion()
        {
            Debug.Log("[LevelCompletionController] Disabled — completion handled by BallTransferController.");
        }
    }
}
