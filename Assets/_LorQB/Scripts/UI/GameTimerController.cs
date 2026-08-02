using System;
using UnityEngine;
using TMPro;

namespace LorQB.UI
{
    /// <summary>
    /// Counts up elapsed time while the game is active and formats it as MM:SS.
    ///
    /// Lifecycle:
    ///   StartTimer()  — called when player confirms name/selection
    ///   PauseTimer()  — called on OnApplicationPause(true)
    ///   ResumeTimer() — called on OnApplicationPause(false)
    ///   StopTimer()   — called when BallTransferController.OnLevelComplete fires
    ///   ResetTimer()  — called when the scene reloads
    ///
    /// Attach to the Timer GameObject under UI.
    /// </summary>
    public class GameTimerController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [SerializeField] private TMP_Text timerText;

        // ── State ────────────────────────────────────────────────────────────────
        private float _elapsedSeconds;
        private bool  _isRunning;

        // ── Properties ───────────────────────────────────────────────────────────
        public float  ElapsedSeconds  => _elapsedSeconds;
        public string FormattedTime   => FormatTime(_elapsedSeconds);
        public bool   IsRunning       => _isRunning;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Update()
        {
            if (!_isRunning) return;

            _elapsedSeconds += Time.deltaTime;
            if (timerText != null)
                timerText.text = FormattedTime;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) PauseTimer();
            else        ResumeTimer();
        }

        // ── Public API ────────────────────────────────────────────────────────────
        public void StartTimer()
        {
            _isRunning = true;
        }

        public void StopTimer()
        {
            _isRunning = false;
        }

        public void PauseTimer()
        {
            _isRunning = false;
        }

        public void ResumeTimer()
        {
            _isRunning = true;
        }

        public void ResetTimer()
        {
            _isRunning      = false;
            _elapsedSeconds = 0f;

            if (timerText != null)
                timerText.text = FormatTime(0f);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private static string FormatTime(float totalSeconds)
        {
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
