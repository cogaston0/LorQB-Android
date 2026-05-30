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
    [RequireComponent(typeof(BallTransferController))]
    [RequireComponent(typeof(ValidationController))]
    [RequireComponent(typeof(InputController))]
    [RequireComponent(typeof(RotationController))]
    public class GameManager : MonoBehaviour
    {
        // ── Fields ─────────────────────────────────────────────────────────
        private SequenceManager        sequenceManager;
        private BallTransferController ballTransfer;
        private ValidationController   validation;
        private InputController        inputController;
        private RotationController     rotationController; // reserved for rotation-gate wiring
        private GameStateManager       _gsm;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            sequenceManager    = new SequenceManager();
            ballTransfer       = GetComponent<BallTransferController>();
            validation         = GetComponent<ValidationController>();
            inputController    = GetComponent<InputController>();
            rotationController = GetComponent<RotationController>();
            _gsm               = GameStateManager.Instance;
        }

        private void Start()
        {
            sequenceManager.GenerateSequence();
            Debug.Log("[GameManager] Sequence generated.");

            ballTransfer.SetSequenceManager(sequenceManager);
            validation.Initialise(sequenceManager, ballTransfer);
            inputController.Initialise(validation);

            if (_gsm != null)
            {
                _gsm.OnStateChanged += OnStateChanged;
                _gsm.SetState(GameStateManager.GameState.BALL_SELECTION);
            }
            else
            {
                Debug.LogWarning("[GameManager] GameStateManager.Instance is null in Start.");
            }
        }

        private void OnDestroy()
        {
            if (_gsm != null)
                _gsm.OnStateChanged -= OnStateChanged;
        }

        // ── Public API ─────────────────────────────────────────────────────────
        /// <summary>
        /// Begins a round by transitioning to BALL_PLACEMENT.
        /// Call after the player has selected the starting cube.
        /// </summary>
        public void StartRound()
        {
            if (_gsm != null)
                _gsm.SetState(GameStateManager.GameState.BALL_PLACEMENT);
            else
                Debug.LogWarning("[GameManager] GameStateManager.Instance is null in StartRound.");
        }

        public void ReloadLevel()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        public void LoadMainMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        public event System.Action<string> OnCubeSelected;

        public event System.Action<string> OnCubeDeselected;

        public bool IsInputAllowed => true;

        // ── Logging ─────────────────────────────────────────────────────────
        private void OnStateChanged(GameStateManager.GameState newState)
        {
            Debug.Log($"[GameManager] State changed → {newState}");
        }
    }
}
