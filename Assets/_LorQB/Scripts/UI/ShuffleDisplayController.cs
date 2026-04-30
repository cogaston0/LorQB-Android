using UnityEngine;
using UnityEngine.UI;
using LorQB.Core;
using LorQB.Movement;

namespace LorQB.UI
{
    /// <summary>
    /// Displays the shuffle order as four coloured dot indicators with arrow separators.
    /// The active step is highlighted; completed steps are dimmed.
    ///
    /// Attach to the Shuffle_Order_Bar GameObject under UI.
    /// Assign the four ShuffleDot Images in inspector order (index 0 = first dot, etc.).
    /// </summary>
    public class ShuffleDisplayController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("Dot Images (index 0 = first in sequence)")]
        [SerializeField] private Image[] shuffleDotImages;   // 4 UI Images

        [Header("Optional Text Labels (shown in Text Mode)")]
        [SerializeField] private TMPro.TMP_Text[] shuffleDotLabels; // 4 TMP labels

        [Header("Step Colours")]
        [SerializeField] private Color activeColor    = Color.white;
        [SerializeField] private Color completedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color pendingColor   = new Color(0.7f, 0.7f, 0.7f, 0.6f);

        [Header("Active scale pulse")]
        [SerializeField] private float activeScale = 1.25f;

        [Header("Cube base colours")]
        [SerializeField] private Color colorBlue   = new Color(0.267f, 0.533f, 1f);
        [SerializeField] private Color colorRed    = new Color(1f, 0.267f, 0.267f);
        [SerializeField] private Color colorGreen  = new Color(0.267f, 0.733f, 0.267f);
        [SerializeField] private Color colorYellow = new Color(1f, 0.867f, 0f);

        [Header("Dependencies")]
        [SerializeField] private BallTransferController ballTransferController;

        // ── State ────────────────────────────────────────────────────────────────
        private CubeColor[] _order;
        private int         _activeStep;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void OnEnable()
        {
            if (ballTransferController != null)
                ballTransferController.OnBallTransferred += HandleBallTransferred;
        }

        private void OnDisable()
        {
            if (ballTransferController != null)
                ballTransferController.OnBallTransferred -= HandleBallTransferred;
        }

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>Initialise the display with a freshly generated shuffle order.</summary>
        public void SetShuffleOrder(CubeColor[] order)
        {
            _order      = order;
            _activeStep = 0;
            RefreshDisplay();
        }

        /// <summary>Mark the current step complete and advance to the next.</summary>
        public void AdvanceStep()
        {
            if (_order == null) return;
            _activeStep = Mathf.Clamp(_activeStep + 1, 0, _order.Length - 1);
            RefreshDisplay();
        }

        /// <summary>Reset all dot indicators to the initial state.</summary>
        public void ResetDisplay()
        {
            _activeStep = 0;
            RefreshDisplay();
        }

        /// <summary>Show or hide the text labels (text-mode support).</summary>
        public void SetLabelsVisible(bool visible)
        {
            if (shuffleDotLabels == null) return;
            foreach (var label in shuffleDotLabels)
                if (label != null) label.gameObject.SetActive(visible);
        }

        // ── Private helpers ───────────────────────────────────────────────────────
        private void RefreshDisplay()
        {
            if (_order == null || shuffleDotImages == null) return;

            for (int i = 0; i < shuffleDotImages.Length; i++)
            {
                if (shuffleDotImages[i] == null) continue;

                Color baseColor = CubeColorToUI(_order.Length > i ? _order[i] : CubeColor.Blue);

                if (i < _activeStep)
                {
                    // Completed step — dimmed
                    shuffleDotImages[i].color              = completedColor;
                    shuffleDotImages[i].transform.localScale = Vector3.one;
                }
                else if (i == _activeStep)
                {
                    // Active step — full colour, enlarged
                    shuffleDotImages[i].color              = baseColor * activeColor;
                    shuffleDotImages[i].transform.localScale = Vector3.one * activeScale;
                }
                else
                {
                    // Pending step — faded colour
                    shuffleDotImages[i].color              = baseColor * pendingColor;
                    shuffleDotImages[i].transform.localScale = Vector3.one;
                }
            }
        }

        private Color CubeColorToUI(CubeColor c) => c switch
        {
            CubeColor.Blue   => colorBlue,
            CubeColor.Red    => colorRed,
            CubeColor.Green  => colorGreen,
            CubeColor.Yellow => colorYellow,
            _                => Color.white
        };

        private void HandleBallTransferred(int newStepIndex)
        {
            _activeStep = newStepIndex;
            RefreshDisplay();
        }
    }
}
