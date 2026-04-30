using System;
using UnityEngine;
using LorQB.Core;

namespace LorQB.Input
{
    /// <summary>Direction a finger swipe was detected.</summary>
    public enum SwipeDirection { Up, Down, Left, Right }

    /// <summary>
    /// Detects tap, swipe, and long-press gestures on the touchscreen and maps them
    /// to cube interactions.  Uses Unity's legacy Input API (compatible with all 2022 LTS
    /// builds without requiring the new Input System package).
    ///
    /// Colliders on cubes are permitted for raycasting only — they must not be attached
    /// to a Rigidbody and must not influence ball movement.
    ///
    /// Attach to the GameManager GameObject.
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────
        [Header("Gesture Thresholds")]
        [SerializeField] private float minSwipeDistance  = 30f;   // pixels
        [SerializeField] private float maxSwipeDuration  = 0.4f;  // seconds
        [SerializeField] private float longPressDuration = 0.6f;  // seconds

        [Header("Raycast")]
        [SerializeField] private LayerMask cubeLayer;

        // ── Events ───────────────────────────────────────────────────────────────
        /// <summary>Fired when a swipe gesture is detected on a cube surface.</summary>
        public event Action<CubeIdentifier, SwipeDirection> OnSwipeOnCube;

        /// <summary>Fired when a short tap is detected on a cube.</summary>
        public event Action<CubeIdentifier> OnTapCube;

        /// <summary>Fired when a long press (≥ 600 ms) is detected on a cube.</summary>
        public event Action<CubeIdentifier> OnLongPressCube;

        // ── State ────────────────────────────────────────────────────────────────
        private bool          _inputBlocked;
        private Vector2       _touchStartPos;
        private float         _touchStartTime;
        private bool          _touchActive;
        private bool          _longPressEmitted;
        private CubeIdentifier _touchedCube;

        // ── Public API ────────────────────────────────────────────────────────────
        public void BlockInput()   => _inputBlocked = true;
        public void UnblockInput() => _inputBlocked = false;
        public bool IsInputBlocked => _inputBlocked;

        // ── Unity lifecycle ──────────────────────────────────────────────────────
        private void Update()
        {
#if UNITY_EDITOR
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        // ── Android touch ────────────────────────────────────────────────────────
        private void HandleTouchInput()
        {
            if (UnityEngine.Input.touchCount == 0) return;

            Touch touch = UnityEngine.Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    BeginTouch(touch.position);
                    break;

                case TouchPhase.Stationary:
                case TouchPhase.Moved:
                    CheckLongPress(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndTouch(touch.position);
                    break;
            }
        }

#if UNITY_EDITOR
        // ── Editor mouse simulation ──────────────────────────────────────────────
        private void HandleMouseInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
                BeginTouch(UnityEngine.Input.mousePosition);

            if (UnityEngine.Input.GetMouseButton(0))
                CheckLongPress(UnityEngine.Input.mousePosition);

            if (UnityEngine.Input.GetMouseButtonUp(0))
                EndTouch(UnityEngine.Input.mousePosition);

            // Right-click simulates long press for Editor testing
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                CubeIdentifier cube = RaycastCube(UnityEngine.Input.mousePosition);
                if (cube != null) OnLongPressCube?.Invoke(cube);
            }
        }
#endif

        // ── Gesture core ─────────────────────────────────────────────────────────
        private void BeginTouch(Vector2 screenPos)
        {
            if (_inputBlocked) return;

            _touchStartPos    = screenPos;
            _touchStartTime   = Time.unscaledTime;
            _touchActive      = true;
            _longPressEmitted = false;
            _touchedCube      = RaycastCube(screenPos);
        }

        private void CheckLongPress(Vector2 screenPos)
        {
            if (!_touchActive || _inputBlocked || _longPressEmitted) return;
            if (_touchedCube == null) return;

            float elapsed = Time.unscaledTime - _touchStartTime;
            float moved   = Vector2.Distance(screenPos, _touchStartPos);

            if (moved < minSwipeDistance && elapsed >= longPressDuration)
            {
                _longPressEmitted = true;
                OnLongPressCube?.Invoke(_touchedCube);
            }
        }

        private void EndTouch(Vector2 screenPos)
        {
            if (!_touchActive) return;
            _touchActive = false;

            if (_inputBlocked || _touchedCube == null) return;

            float elapsed  = Time.unscaledTime - _touchStartTime;
            float distance = Vector2.Distance(screenPos, _touchStartPos);

            if (_longPressEmitted) return; // already handled

            if (elapsed <= maxSwipeDuration && distance >= minSwipeDistance)
            {
                SwipeDirection dir = GetSwipeDirection(_touchStartPos, screenPos);
                OnSwipeOnCube?.Invoke(_touchedCube, dir);
            }
            else if (elapsed <= maxSwipeDuration && distance < minSwipeDistance)
            {
                OnTapCube?.Invoke(_touchedCube);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        /// <summary>Raycast from screen position into the scene and return a CubeIdentifier if hit.</summary>
        private CubeIdentifier RaycastCube(Vector2 screenPos)
        {
            if (Camera.main == null) return null;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cubeLayer))
                return hit.collider.GetComponentInParent<CubeIdentifier>();

            return null;
        }

        private static SwipeDirection GetSwipeDirection(Vector2 start, Vector2 end)
        {
            Vector2 delta = end - start;
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
            else
                return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        }
    }
}
