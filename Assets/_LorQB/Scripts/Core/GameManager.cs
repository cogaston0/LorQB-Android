using UnityEngine;
using LorQB.Movement;
using LorQB.Validation;
using LorQB.Input;

namespace LorQB.Core
{
    /// <summary>
    /// Wires all Level 1 systems together and controls round flow.
    /// No physics, no UI, no auto-play — pure orchestration.
    /// Attach to the GameManager GameObject alongside BallTransferController,
    /// ValidationController, InputController, and RotationController.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Fields ────────────────────────────────────────────────────────────────
        private SequenceManager       sequenceManager;
        private BallTransferController ballTransfer;
        private ValidationController  validation;
        private InputController       inputController;
        private RotationController    rotationController;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            sequenceManager    = new SequenceManager();
            ballTransfer       = GetComponent<BallTransferController>();
            validation         = GetComponent<ValidationController>();
            inputController    = GetComponent<InputController>();
            rotationController = GetComponent<RotationController>();
        }

        private void Start()
        {
            sequenceManager.GenerateSequence();
            Debug.Log("[GameManager] Sequence generated.");

            validation.Initialise(sequenceManager, ballTransfer);
            inputController.Initialise(validation);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += OnStateChanged;
                GameStateManager.Instance.SetState(GameStateManager.GameState.BALL_SELECTION);
            }
            else
            {
                Debug.LogWarning("[GameManager] GameStateManager.Instance is null in Start.");
            }
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= OnStateChanged;
        }

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>
        /// Begins a round by transitioning to BALL_PLACEMENT.
        /// Call after the player has selected the starting cube.
        /// </summary>
        public void StartRound()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.SetState(GameStateManager.GameState.BALL_PLACEMENT);
            else
                Debug.LogWarning("[GameManager] GameStateManager.Instance is null in StartRound.");
        }

        // ── Logging ───────────────────────────────────────────────────────────────
        private void OnStateChanged(GameStateManager.GameState newState)
        {
            Debug.Log($"[GameManager] State changed → {newState}");
        }
    }
}
