using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using LorQB.Movement;

namespace LorQB.Core
{
    /// <summary>
    /// Central coordinator for Level 1.
    /// Manages game state, orchestrates input locking, and responds to level-complete events.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Inspector references ──────────────────────────────────────────────────
        [Header("Scene References")]
        [SerializeField] private BallTransferController ballTransferController;
        [SerializeField] private CubeRotationController[] cubeRotationControllers;

        // ── State ────────────────────────────────────────────────────────────────
        private bool _isBallTransferring;
        private bool _isRotating;
        private CubeIdentifier _selectedCube;

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>Fired when the player selects a cube via tap.</summary>
        public event Action<CubeIdentifier> OnCubeSelected;

        /// <summary>Fired when the selected cube is deselected.</summary>
        public event Action OnCubeDeselected;

        /// <summary>True while no rotation or transfer is in progress — safe to accept new input.</summary>
        public bool IsInputAllowed => !_isBallTransferring && !_isRotating;

        public CubeIdentifier SelectedCube => _selectedCube;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (ballTransferController != null)
            {
                ballTransferController.OnTransferStarted  += HandleTransferStarted;
                ballTransferController.OnBallTransferred  += HandleBallTransferred;
                ballTransferController.OnLevelComplete    += HandleLevelComplete;
            }

            foreach (var ctrl in cubeRotationControllers)
            {
                if (ctrl == null) continue;
                ctrl.OnRotationStarted  += HandleRotationStarted;
                ctrl.OnRotationComplete += HandleRotationComplete;
            }
        }

        private void OnDisable()
        {
            if (ballTransferController != null)
            {
                ballTransferController.OnTransferStarted  -= HandleTransferStarted;
                ballTransferController.OnBallTransferred  -= HandleBallTransferred;
                ballTransferController.OnLevelComplete    -= HandleLevelComplete;
            }

            foreach (var ctrl in cubeRotationControllers)
            {
                if (ctrl == null) continue;
                ctrl.OnRotationStarted  -= HandleRotationStarted;
                ctrl.OnRotationComplete -= HandleRotationComplete;
            }
        }

        // ── Input lock helpers ───────────────────────────────────────────────────
        public void BlockInput()   => _isRotating = true;
        public void UnblockInput() => _isRotating = false;

        // ── Cube selection ───────────────────────────────────────────────────────
        /// <summary>
        /// Called by TouchInputManager when the player taps a cube.
        /// Tapping the already-selected cube deselects it.
        /// </summary>
        public void SelectCube(CubeIdentifier cube)
        {
            if (_selectedCube == cube)
            {
                DeselectCube();
                return;
            }

            _selectedCube = cube;
            OnCubeSelected?.Invoke(cube);
        }

        public void DeselectCube()
        {
            _selectedCube = null;
            OnCubeDeselected?.Invoke();
        }

        // ── Event handlers ───────────────────────────────────────────────────────
        private void HandleRotationStarted()  => _isRotating = true;
        private void HandleRotationComplete() => _isRotating = false;

        private void HandleTransferStarted()        => _isBallTransferring = true;
        private void HandleBallTransferred(int idx) => _isBallTransferring = false;

        private void HandleLevelComplete()
        {
            _isBallTransferring = false;
            _isRotating         = false;
            // Notify registered listeners (e.g. LevelCompleteController) via their
            // own subscription to BallTransferController.OnLevelComplete.
        }

        // ── Scene management ─────────────────────────────────────────────────────
        public void ReloadLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
