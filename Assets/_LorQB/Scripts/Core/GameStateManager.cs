using System;
using UnityEngine;

namespace LorQB.Core
{
    /// <summary>
    /// Central state controller for LorQB Level 1.
    /// Single authority over the active GameState; all subsystems read from here
    /// and report events back — they never write the state themselves.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        // ── Enum ─────────────────────────────────────────────────────────────────
        public enum GameState
        {
            IDLE,
            BALL_SELECTION,
            BALL_PLACEMENT,
            ACTIVE_PLAY,
            VALIDATION,
            TRANSFER,
            POST_TRANSFER,
            ROUND_COMPLETE,
            TIME_UP
        }

        // ── Singleton ────────────────────────────────────────────────────────────
        public static GameStateManager Instance { get; private set; }

        // ── State ────────────────────────────────────────────────────────────────
        private GameState _currentState = GameState.IDLE;

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>Fired immediately after the active state changes.</summary>
        public event Action<GameState> OnStateChanged;

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

        // ── Public API ───────────────────────────────────────────────────────────
        /// <summary>Returns the currently active game state.</summary>
        public GameState GetState() => _currentState;

        /// <summary>
        /// Transitions to <paramref name="newState"/> if the transition is allowed
        /// by the Level 1 state-machine definition.
        /// TIME_UP is always allowed from any state.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (!IsTransitionAllowed(_currentState, newState))
            {
                Debug.LogWarning(
                    $"[GameStateManager] Blocked illegal transition: {_currentState} → {newState}");
                return;
            }

            Debug.Log($"[GameStateManager] State: {_currentState} → {newState}");
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }

        // ── Transition table ─────────────────────────────────────────────────────
        private static bool IsTransitionAllowed(GameState from, GameState to)
        {
            // TIME_UP may be entered from any state (timer override).
            if (to == GameState.TIME_UP)
                return true;

            return (from, to) switch
            {
                (GameState.IDLE,           GameState.BALL_SELECTION)  => true,
                (GameState.BALL_SELECTION, GameState.BALL_PLACEMENT)  => true,
                (GameState.BALL_PLACEMENT, GameState.ACTIVE_PLAY)     => true,
                (GameState.ACTIVE_PLAY,    GameState.VALIDATION)      => true,
                (GameState.VALIDATION,     GameState.TRANSFER)        => true,
                (GameState.VALIDATION,     GameState.ACTIVE_PLAY)     => true,
                (GameState.TRANSFER,       GameState.POST_TRANSFER)   => true,
                (GameState.POST_TRANSFER,  GameState.ACTIVE_PLAY)     => true,
                (GameState.POST_TRANSFER,  GameState.ROUND_COMPLETE)  => true,
                _ => false
            };
        }
    }
}
