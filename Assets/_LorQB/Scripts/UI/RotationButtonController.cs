using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LorQB.Core;
using LorQB.Movement;

namespace LorQB.UI
{
    /// <summary>
    /// Shows and manages the rotation arrow buttons in the bottom HUD.
    ///
    /// Visibility rules:
    ///   - Panel is hidden when no cube is selected.
    ///   - Panel appears when a cube is selected via GameManager.OnCubeSelected.
    ///   - Buttons are disabled (grayed out) during rotation or ball transfer.
    ///
    /// Attach to the rotation controls row GameObject under UI.
    /// </summary>
    public class RotationButtonController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("UI Elements")]
        [SerializeField] private Button     rotateLeftButton;
        [SerializeField] private Button     rotateRightButton;
        [SerializeField] private TMP_Text   selectedCubeLabel;
        [SerializeField] private GameObject buttonPanel;

        [Header("Dependencies")]
        [SerializeField] private GameManager            gameManager;
        [SerializeField] private BallTransferController ballTransferController;

        // ── State ────────────────────────────────────────────────────────────────
        private CubeRotationController _activeController;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Start()
        {
            Hide();

            if (rotateLeftButton  != null) rotateLeftButton.onClick.AddListener(OnRotateLeft);
            if (rotateRightButton != null) rotateRightButton.onClick.AddListener(OnRotateRight);
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnCubeSelected   += HandleCubeSelected;
                gameManager.OnCubeDeselected += HandleCubeDeselected;
            }

            if (ballTransferController != null)
            {
                ballTransferController.OnTransferStarted += HandleActionStarted;
                ballTransferController.OnBallTransferred += _ => HandleActionComplete();
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnCubeSelected   -= HandleCubeSelected;
                gameManager.OnCubeDeselected -= HandleCubeDeselected;
            }

            if (ballTransferController != null)
            {
                ballTransferController.OnTransferStarted -= HandleActionStarted;
                ballTransferController.OnBallTransferred -= _ => HandleActionComplete();
            }
        }

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>Show the rotation panel for the specified cube.</summary>
        public void ShowForCube(CubeIdentifier cube)
        {
            if (cube == null) { Hide(); return; }

            _activeController = cube.GetComponent<CubeRotationController>();

            if (buttonPanel     != null) buttonPanel.SetActive(true);
            if (selectedCubeLabel != null) selectedCubeLabel.text = cube.Color.ToString();

            SetButtonsEnabled(_activeController != null && !_activeController.IsRotating);
        }

        /// <summary>Hide the rotation panel and clear the active cube.</summary>
        public void Hide()
        {
            _activeController = null;
            if (buttonPanel != null) buttonPanel.SetActive(false);
        }

        public void SetButtonsEnabled(bool enabled)
        {
            if (rotateLeftButton  != null) rotateLeftButton.interactable  = enabled;
            if (rotateRightButton != null) rotateRightButton.interactable = enabled;
        }

        // ── Button handlers ───────────────────────────────────────────────────────
        private void OnRotateLeft()
        {
            if (_activeController == null || !gameManager.IsInputAllowed) return;
            _activeController.RotateBackward();
            SetButtonsEnabled(false);
        }

        private void OnRotateRight()
        {
            if (_activeController == null || !gameManager.IsInputAllowed) return;
            _activeController.RotateForward();
            SetButtonsEnabled(false);
        }

        // ── Event handlers ────────────────────────────────────────────────────────
        private void HandleCubeSelected(CubeIdentifier cube)   => ShowForCube(cube);
        private void HandleCubeDeselected()                    => Hide();
        private void HandleActionStarted()                     => SetButtonsEnabled(false);
        private void HandleActionComplete()                    => SetButtonsEnabled(true);
    }
}
