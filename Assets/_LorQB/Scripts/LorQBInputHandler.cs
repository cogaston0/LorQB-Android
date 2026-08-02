// LorQBInputHandler.cs  (Blender 5.1.1 reference build)
// Android touch + Editor mouse input — New Input System.
// Handles two gestures:
//   1. Drag ball → drop on first cube (WaitingForBallDrop phase)
//   2. Tap a cube (Idle phase) → transfers ball to next in sequence
// Requires: Input System Package (active in Project Settings → Player)
// Place in: Assets/_LorQB/Scripts/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace LorQB
{
    public class LorQBInputHandler : MonoBehaviour
    {
        [HideInInspector] public LorQBGameManager Manager;

        private Camera _cam;
        private bool   _draggingBall = false;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        void Start()
        {
            _cam = Camera.main;
        }

        void Update()
        {
            if (Manager == null || _cam == null) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            HandleTouch();
#else
            HandleMouse();
#endif
        }

        // ── Android touch ──────────────────────────────────────────────────────

        void HandleTouch()
        {
            var touches = Touch.activeTouches;
            if (touches.Count == 0) return;

            var touch = touches[0];
            var pos   = touch.screenPosition;

            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    OnPointerDown(pos);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
                case UnityEngine.InputSystem.TouchPhase.Stationary:
                    OnPointerMove(pos);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    OnPointerUp(pos);
                    break;
            }
        }

        // ── Editor mouse fallback ──────────────────────────────────────────────

        void HandleMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            var pos = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)  OnPointerDown(pos);
            if (mouse.leftButton.isPressed)             OnPointerMove(pos);
            if (mouse.leftButton.wasReleasedThisFrame)  OnPointerUp(pos);
        }

        // ── Unified pointer events ─────────────────────────────────────────────

        void OnPointerDown(Vector2 screenPos)
        {
            // WaitingForBallDrop: check if player touched the ball
            if (Manager.Phase == GamePhase.WaitingForBallDrop)
            {
                if (HitsBall(screenPos))
                {
                    _draggingBall = true;
                    Manager.Ball.StartDrag();
                }
                return;
            }

            // Idle: tap a cube
            if (Manager.Phase == GamePhase.Idle)
            {
                var cube = RaycastCube(screenPos);
                if (cube != null) Manager.OnCubeTapped(cube.Id);
            }
        }

        void OnPointerMove(Vector2 screenPos)
        {
            if (_draggingBall && Manager.Phase == GamePhase.WaitingForBallDrop)
                Manager.Ball.UpdateDrag(screenPos);
        }

        void OnPointerUp(Vector2 screenPos)
        {
            if (!_draggingBall) return;
            _draggingBall = false;
            Manager.Ball.EndDrag();

            // Check if ball was dropped over a cube
            if (Manager.Phase == GamePhase.WaitingForBallDrop)
            {
                var cube = RaycastCube(screenPos);
                if (cube != null)
                    Manager.OnBallDroppedOnCube(cube.Id);
                else
                    Manager.Ball.ResetToOutside(); // dropped in empty space — reset
            }
        }

        // ── Raycast helpers ────────────────────────────────────────────────────

        bool HitsBall(Vector2 screenPos)
        {
            Ray ray = _cam.ScreenPointToRay(screenPos);
            // Approximate: check if ray is close to ball world position
            float dist = Vector3.Cross(ray.direction,
                Manager.Ball.transform.position - ray.origin).magnitude;
            return dist < LorQBConfig.BALL_RADIUS * 2.5f;
        }

        LorQBCubeNode RaycastCube(Vector2 screenPos)
        {
            Ray ray = _cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                return hit.collider.GetComponentInParent<LorQBCubeNode>();
            return null;
        }
    }
}
