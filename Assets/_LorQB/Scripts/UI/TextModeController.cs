using UnityEngine;
using TMPro;

namespace LorQB.UI
{
    /// <summary>
    /// Toggles text label overlays on shuffle dots and cube face labels.
    /// Provides an accessibility option for early readers and learners.
    ///
    /// Attach to any persistent UI GameObject (e.g., the header bar).
    /// </summary>
    public class TextModeController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Tooltip("GameObjects whose visibility is toggled when text mode is on/off.")]
        [SerializeField] private GameObject[] textOnlyElements;

        [Tooltip("Button label that alternates between 'TEXT ON' and 'TEXT OFF'.")]
        [SerializeField] private TMP_Text toggleButtonLabel;

        // ── State ────────────────────────────────────────────────────────────────
        private bool _isTextModeOn;

        // ── Properties ───────────────────────────────────────────────────────────
        public bool IsTextModeOn => _isTextModeOn;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Start()
        {
            // Start with text mode off
            _isTextModeOn = false;
            ApplyTextMode();
        }

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>Called by the TEXT MODE button's onClick event.</summary>
        public void ToggleTextMode()
        {
            _isTextModeOn = !_isTextModeOn;
            ApplyTextMode();
        }

        // ── Private helpers ───────────────────────────────────────────────────────
        private void ApplyTextMode()
        {
            if (textOnlyElements != null)
            {
                foreach (var go in textOnlyElements)
                    if (go != null) go.SetActive(_isTextModeOn);
            }

            if (toggleButtonLabel != null)
                toggleButtonLabel.text = _isTextModeOn ? "TEXT OFF" : "TEXT ON";
        }
    }
}
