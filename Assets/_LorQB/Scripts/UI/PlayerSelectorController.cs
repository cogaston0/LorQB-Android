using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LorQB.UI
{
    /// <summary>
    /// Handles the pre-game player selection panel.
    ///
    /// Flow:
    ///   1. Panel shown on scene load (game paused).
    ///   2. Player picks from dropdown or types a custom name.
    ///   3. Player taps [START] → OnPlayerConfirmed fires → panel hides.
    ///
    /// Attach to the Player_Selector GameObject under UI.
    /// </summary>
    public class PlayerSelectorController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("UI Elements")]
        [SerializeField] private TMP_Dropdown  playerDropdown;
        [SerializeField] private TMP_InputField customNameField;
        [SerializeField] private Button         startButton;
        [SerializeField] private GameObject     selectorPanel;

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>Fired when START is tapped. Argument = selected player name.</summary>
        public event Action<string> OnPlayerConfirmed;

        // ── State ────────────────────────────────────────────────────────────────
        private string _confirmedName = "Guest";

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Start()
        {
            ShowPanel();

            if (startButton != null)
                startButton.onClick.AddListener(HandleStartTapped);

            if (playerDropdown != null)
            {
                playerDropdown.ClearOptions();
                playerDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Player 1", "Player 2", "Player 3", "Player 4", "Guest"
                });
            }
        }

        // ── Public API ────────────────────────────────────────────────────────────
        public void ShowPanel()
        {
            if (selectorPanel != null) selectorPanel.SetActive(true);
            Time.timeScale = 0f; // pause game while selecting
        }

        public void HidePanel()
        {
            if (selectorPanel != null) selectorPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public string ConfirmedPlayerName => _confirmedName;

        // ── Handlers ─────────────────────────────────────────────────────────────
        private void HandleStartTapped()
        {
            // Prefer custom name if entered, else use dropdown selection
            string name = string.Empty;

            if (customNameField != null && !string.IsNullOrWhiteSpace(customNameField.text))
                name = customNameField.text.Trim();
            else if (playerDropdown != null)
                name = playerDropdown.options[playerDropdown.value].text;

            if (string.IsNullOrEmpty(name)) name = "Guest";

            _confirmedName = name;
            HidePanel();
            OnPlayerConfirmed?.Invoke(_confirmedName);
        }
    }
}
