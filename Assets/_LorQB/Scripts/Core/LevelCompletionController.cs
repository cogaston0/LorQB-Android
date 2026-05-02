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

        /// <summary>
        /// Checks whether the current round is complete and, if so, transitions to
        /// <see cref="GameStateManager.GameState.ROUND_COMPLETE"/>.
        /// Call this from POST_TRANSFER after each successful ball transfer.
        /// </summary>
        public void CheckCompletion()
        {
            if (_sequenceManager == null || _gameStateManager == null)
            {
                Debug.LogWarning("[LevelCompletionController] CheckCompletion called before Initialise.");
                return;
            }

            if (_sequenceManager.GetCurrentIndex() == 3)
            {
                Debug.Log("[LevelCompletionController] Round complete — transitioning to ROUND_COMPLETE.");
                _gameStateManager.SetState(GameStateManager.GameState.ROUND_COMPLETE);
            }
        }
    }
}
