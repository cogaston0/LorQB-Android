using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LorQB.Movement;

namespace LorQB.UI
{
    /// <summary>
    /// Displays the Level Complete overlay when the ball reaches the final cube.
    ///
    /// Attach to the LevelComplete panel child under UI.
    /// The panel starts hidden and fades in when OnLevelComplete fires.
    /// </summary>
    public class LevelCompleteController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("UI Elements")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text   playerNameText;
        [SerializeField] private TMP_Text   finalTimeText;
        [SerializeField] private Button     playAgainButton;
        [SerializeField] private Button     mainMenuButton;

        [Header("Dependencies")]
        [SerializeField] private BallTransferController ballTransferController;
        [SerializeField] private GameTimerController    timerController;
        [SerializeField] private Core.GameManager       gameManager;

        // ── State ────────────────────────────────────────────────────────────────
        private string _playerName = "Guest";

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (playAgainButton != null) playAgainButton.onClick.AddListener(HandlePlayAgain);
            if (mainMenuButton  != null) mainMenuButton.onClick.AddListener(HandleMainMenu);
        }

        private void OnEnable()
        {
            if (ballTransferController != null)
                ballTransferController.OnLevelComplete += HandleLevelComplete;
        }

        private void OnDisable()
        {
            if (ballTransferController != null)
                ballTransferController.OnLevelComplete -= HandleLevelComplete;
        }

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>Set the player name to display on the level complete screen.</summary>
        public void SetPlayerName(string name)
        {
            _playerName = string.IsNullOrWhiteSpace(name) ? "Guest" : name;
        }

        // ── Handlers ─────────────────────────────────────────────────────────────
        private void HandleLevelComplete()
        {
            if (timerController != null) timerController.StopTimer();

            if (playerNameText != null) playerNameText.text = _playerName;
            if (finalTimeText  != null) finalTimeText.text  = timerController != null
                ? timerController.FormattedTime
                : "00:00";

            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void HandlePlayAgain()
        {
            if (gameManager != null) gameManager.ReloadLevel();
        }

        private void HandleMainMenu()
        {
            if (gameManager != null) gameManager.LoadMainMenu();
        }
    }
}
