# Agent 04 — Touch Control

## Role
Design and implement the Android touch input system for LorQB / Cubolita Level 1. All interactions must work reliably on Android touchscreens. No mouse or keyboard input is required for the shipped build (Editor mouse support is acceptable for testing only).

---

## Core Constraints

- Target input system: **Unity Input System package** (new input system, not legacy)
- Primary input: **Android touchscreen**
- No accelerometer or gyroscope input
- Touch zones must be clearly defined and not overlap UI elements
- Input must be **blocked during ball transfer** and **during rotation animation**

---

## Gesture Definitions

### Gesture 1 — Rotate Cube (Swipe on Cube)

| Property | Value |
|----------|-------|
| Input | Single-finger swipe on a cube face |
| Direction | Swipe **up** = rotate cube 90° forward; swipe **down** = rotate cube 90° backward |
| Minimum swipe distance | 30 pixels |
| Maximum swipe time | 400 ms |
| Feedback | Cube begins rotating immediately on swipe recognition |

### Gesture 2 — Tap to Select Cube

| Property | Value |
|----------|-------|
| Input | Single tap on a cube |
| Effect | Highlights the selected cube (outline glow) |
| Followed by | Directional tap on arrow buttons OR a swipe to rotate |
| Cancel | Tap elsewhere or tap same cube again deselects |

### Gesture 3 — Tap Arrow Buttons (UI)

| Property | Value |
|----------|-------|
| Input | Tap on on-screen rotate arrows (UI buttons) |
| Effect | Rotates currently selected cube by 90° in arrow direction |
| Location | Bottom HUD row — two arrow buttons per selected cube |

### Gesture 4 — Long Press (Hold to Lock)

| Property | Value |
|----------|-------|
| Input | Long press (≥ 600 ms) on a cube |
| Effect | Locks the cube in current rotation (prevents accidental rotation) |
| Visual indicator | Lock icon appears on cube face |
| Unlock | Another long press |

---

## Touch Input Architecture

```
TouchInputManager (MonoBehaviour, singleton)
    │
    ├── DetectSwipe()         — recognizes swipe gesture, emits SwipeEvent
    ├── DetectTap()           — recognizes tap, emits TapEvent
    ├── DetectLongPress()     — recognizes long press, emits LongPressEvent
    └── IsInputBlocked()      — returns true during ball transfer or rotation
            │
            └── Subscribed to:
                    BallTransferController.OnBallTransferred
                    CubeRotationController.OnRotationComplete
```

### TouchInputManager.cs — Script Specification

**Namespace:** `LorQB.Input`

**Fields:**
```
[SerializeField] float minSwipeDistance = 30f     // pixels
[SerializeField] float maxSwipeDuration = 0.4f    // seconds
[SerializeField] float longPressDuration = 0.6f   // seconds
[SerializeField] LayerMask cubeLayer              // raycast layer for cubes
```

**Events:**
```
event Action<CubeIdentifier, SwipeDirection> OnSwipeOnCube
    // SwipeDirection: Up, Down, Left, Right

event Action<CubeIdentifier> OnTapCube
    // Tap on a cube (short, no movement)

event Action<CubeIdentifier> OnLongPressCube
    // Long press on a cube
```

**Input Blocking:**
```
bool inputBlocked = false

void BlockInput()     // called by GameManager before rotation/transfer
void UnblockInput()   // called by GameManager after rotation/transfer
```

---

## Raycast Touch-to-World

Since cubes are 3D objects in world space, touches on the cube surfaces must be detected via **raycasting**:

```
1. OnScreenTouch → get screen position (touch.position)
2. Camera.main.ScreenPointToRay(touchPosition) → Ray
3. Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cubeLayer)
   NOTE: Colliders are allowed for TOUCH DETECTION only — not for gameplay movement
4. hit.collider.GetComponent<CubeIdentifier>() → identify which cube was touched
```

> ⚠️ **Exception to "no physics" rule:** Colliders may be added to cube GameObjects **solely for raycasting touch detection**. They must not be connected to any Rigidbody and must not affect ball movement.

---

## Touch Zone Layout

```
┌────────────────────────────────┐
│                                │  ← Status bar (no touch)
│  ┌──────────────────────────┐  │
│  │   Shuffle Order Display  │  │  ← UI touch zone (tap to toggle text mode)
│  └──────────────────────────┘  │
│                                │
│   [Blue] [Red] [Green] [Yellow]│  ← 3D Cube touch zones (swipe/tap)
│                                │
│  ┌──────────────────────────┐  │
│  │  Timer    Player Sel.    │  │  ← HUD row (tap player selector)
│  │  [◀ Rot] [▶ Rot]         │  │  ← Arrow buttons (tap to rotate)
│  └──────────────────────────┘  │
└────────────────────────────────┘
```

---

## Input State Machine

```
IDLE
  │
  ├── Touch begins on cube → TOUCH_START (record time + position)
  │
TOUCH_START
  │
  ├── Lift within 400ms, distance < 30px → emit TapCube → IDLE
  ├── Hold > 600ms, no movement → emit LongPressCube → IDLE
  ├── Move > 30px within 400ms → detect direction → emit SwipeOnCube → IDLE
  └── Touch ends without gesture → IDLE

BLOCKED (during rotation or ball transfer)
  │
  └── All input ignored until UnblockInput() is called
```

---

## Selected Cube Visual Feedback

When a cube is selected via tap:
- Apply a **color-tinted outline** shader effect (or enable an outline child object)
- Show **rotation arrow UI buttons** in the HUD

When deselected:
- Remove outline
- Hide rotation arrow UI buttons

---

## Editor Testing Support

For testing in the Unity Editor (Windows/Mac):
- **Mouse left button drag** simulates swipe
- **Mouse left button click** simulates tap
- **Mouse right button click** simulates long press
- Implemented via `#if UNITY_EDITOR` conditional block in `TouchInputManager`

---

## Deliverables (Phase 4)

- [ ] `TouchInputManager.cs` written and placed on GameManager
- [ ] Swipe gesture correctly identifies cube and direction
- [ ] Tap gesture correctly selects/deselects a cube
- [ ] Long press correctly locks/unlocks a cube
- [ ] Input is blocked during rotation animation
- [ ] Input is blocked during ball transfer
- [ ] Arrow button taps trigger rotation via `CubeRotationController`
- [ ] Tested on Android device (physical or emulator)
- [ ] Editor mouse simulation working for development workflow
