using UnityEngine;
using LorQB.Core;
using LorQB.Validation;

namespace LorQB.Input
{
    /// <summary>
    /// Handles player touch input (press, drag, release) and forwards validated release
    /// events to <see cref="ValidationController"/>.
    ///
    /// Input is only processed while the game is in ACTIVE_PLAY.
    /// Touch identifies which cube was pressed; rotation axis selection is deferred to
    /// the rotation system and is not handled here.
    ///
    /// Attach to the GameManager GameObject.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        // ── Fields ────────────────────────────────────────────────────────────────
        private ValidationController _validationController;

        [Header("Raycast")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask cubeLayerMask;

        [Header("Gesture")]
        [SerializeField] private float dragThreshold = 10f;   // pixels

        // ── Runtime state ─────────────────────────────────────────────────────────
        private bool            _touchActive;
        private bool            _dragLogged;
        private Vector2         _pressStartPos;
        private CubeIdentifier  _pressedCube;

        // ── Public API ────────────────────────────────────────────────────────────
        /// <summary>
        /// Wire the <see cref="ValidationController"/> dependency.
        /// Must be called once before the first round begins (e.g. from GameManager).
        /// </summary>
        public void Initialise(ValidationController validationController)
        {
            _validationController = validationController;
        }

        // ── Unity lifecycle ───────────────────────────────────────────────────────
        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void Update()
        {
#if UNITY_EDITOR
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        // ── Android touch ─────────────────────────────────────────────────────────
        private void HandleTouchInput()
        {
            if (UnityEngine.Input.touchCount == 0) return;

            Touch touch = UnityEngine.Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnPress(touch.position);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnDrag(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnRelease(touch.position);
                    break;
            }
        }

#if UNITY_EDITOR
        // ── Editor mouse simulation ───────────────────────────────────────────────
        private void HandleMouseInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
                OnPress(UnityEngine.Input.mousePosition);

            if (UnityEngine.Input.GetMouseButton(0))
                OnDrag(UnityEngine.Input.mousePosition);

            if (UnityEngine.Input.GetMouseButtonUp(0))
                OnRelease(UnityEngine.Input.mousePosition);
        }
#endif

        // ── Input core ────────────────────────────────────────────────────────────
        private void OnPress(Vector2 screenPos)
        {
            if (!IsActivePlay()) return;

            _pressedCube   = RaycastCube(screenPos);
            _pressStartPos = screenPos;
            _touchActive   = true;
            _dragLogged    = false;

            if (_pressedCube != null)
                Debug.Log($"[InputController] Touch start on cube: {_pressedCube.Color}");
            else
                Debug.Log("[InputController] Touch start — no cube hit.");
        }

        private void OnDrag(Vector2 screenPos)
        {
            if (!_touchActive) return;

            if (!_dragLogged && Vector2.Distance(screenPos, _pressStartPos) >= dragThreshold)
            {
                _dragLogged = true;
                Debug.Log("[InputController] Drag threshold passed.");
            }
        }

        private void OnRelease(Vector2 screenPos)
        {
            if (!_touchActive) return;

            _touchActive = false;

            if (_pressedCube == null) return;

            CubeColor from = _pressedCube.Color;

            // Determine 'to' cube by raycasting at the release position.
            // Falls back to the pressed cube when no second cube is found (placeholder).
            CubeIdentifier releasedCube = RaycastCube(screenPos);
            CubeColor to = releasedCube != null ? releasedCube.Color : from;

            Debug.Log($"[InputController] Release — from={from}, to={to}.");

            if (_validationController != null)
                _validationController.ValidateRelease(from, to);
            else
                Debug.LogWarning("[InputController] ValidateRelease skipped: ValidationController not set.");

            _pressedCube = null;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private CubeIdentifier RaycastCube(Vector2 screenPos)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return null;

            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cubeLayerMask))
                return hit.collider.GetComponentInParent<CubeIdentifier>();

            return null;
        }

        private static bool IsActivePlay()
        {
            GameStateManager gsm = GameStateManager.Instance;
            return gsm != null && gsm.GetState() == GameStateManager.GameState.ACTIVE_PLAY;
        }
    }
}
