using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        // ── Constants ────────────────────────────────────────────────────────────
        private const int MAIN_MENU_SCENE_INDEX = 0;

        // ── Fields ────────────────────────────────────────────────────────────────
        private SequenceManager        sequenceManager;
        private BallTransferController ballTransfer;
        private ValidationController   validation;
        private InputController        inputController;
        private RotationController     rotationController; // reserved for rotation-gate wiring
        private GameStateManager       _gsm;

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>Fired when the player selects a cube.</summary>
        public event Action<CubeIdentifier> OnCubeSelected;

        /// <summary>Fired when the active cube is deselected.</summary>
        public event Action OnCubeDeselected;

        // ── Properties ───────────────────────────────────────────────────────────
        /// <summary>
        /// Returns true when the current game state allows player input
        /// (BALL_SELECTION or ACTIVE_PLAY).
        /// </summary>
        public bool IsInputAllowed =>
            _gsm != null &&
            (_gsm.GetState() == GameStateManager.GameState.BALL_SELECTION ||
             _gsm.GetState() == GameStateManager.GameState.ACTIVE_PLAY);

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

        // ── Public API ────────────────────────────────────────────────────────────
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

        /// <summary>Raises OnCubeSelected for the given cube.</summary>
        public void SelectCube(CubeIdentifier cube) => OnCubeSelected?.Invoke(cube);

        /// <summary>Raises OnCubeDeselected.</summary>
        public void DeselectCube() => OnCubeDeselected?.Invoke();

        /// <summary>Reloads the active scene to restart the current level.</summary>
        public void ReloadLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>Loads scene index 0 (main menu).</summary>
        public void LoadMainMenu()
        {
            SceneManager.LoadScene(MAIN_MENU_SCENE_INDEX);
        }

        // ── Logging ───────────────────────────────────────────────────────────────
        private void OnStateChanged(GameStateManager.GameState newState)
        {
            Debug.Log($"[GameManager] State changed → {newState}");
        }
    }
}
