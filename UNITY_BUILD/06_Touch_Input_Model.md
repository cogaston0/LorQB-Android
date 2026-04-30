# 06 — Touch Input Model

## Overview

This document defines how the player controls cubes and the ball in LorQB Android via touch gestures. The system reads player input in real time and maps it to cube rotation and ball placement. No code, no UI design, no Unity scene — interaction model only.

---

## 1. Input Types

| Gesture | Target | Action |
|---------|--------|--------|
| **Tap** | Ball or cube face | Select the tapped object |
| **Drag** | Cube face | Rotate the cube around its hinge axis (continuous while dragging) |
| **Release** | — | Stop rotation; trigger validation check |

- A tap that does not land on a valid target is ignored.
- A drag shorter than the minimum swipe threshold is discarded (see §7).

---

## 2. Cube Interaction

### Hinge Assignment

Each cube is linked to exactly one pivot. The player can only rotate a cube around its designated hinge axis — free rotation is not permitted.

| Cube pair | Hinge shortcode | Unity pivot name | Unity rotation axis |
|-----------|----------------|------------------|---------------------|
| Blue / Red | HBR | `Pivot_Blue_Red` | Z-axis |
| Red / Green | HRG | `Pivot_Red_Green` | X-axis |
| Green / Yellow | HGY | `Pivot_Green_Yellow` | Z-axis |

### Rules

- The player taps a cube face to select it, then drags to rotate.
- Rotation is constrained to the cube's assigned hinge axis only.
- The cube that is the **anchor** for a given hinge does not rotate; only the **moving** cube in the pair rotates around the pivot.
- No cube can be rotated simultaneously with another cube (single-touch primary, see §7).

---

## 3. Rotation Behavior

- **Continuous rotation**: the cube rotates as long as the player's finger is dragging.
- **No snapping by default**: the cube stops at whatever angle the player releases.
- **Real-time angle reading**: the system reads the current rotation angle on every frame while dragging.
- **Direction**: swipe direction (left/right or up/down depending on the hinge axis) determines whether rotation is clockwise or counter-clockwise.
- **Axis mapping**:
  - HBR and HGY (Z-axis): horizontal swipe → rotation around Z.
  - HRG (X-axis): vertical or depth swipe → rotation around X.

---

## 4. Ball Interaction

### Selection

- **Tap ball** → the ball becomes the selected object and its color is registered as the active color.

### Placement

- **Drag ball** → the ball follows the drag gesture toward a cube.
- On release over the first required cube → the ball is placed into that cube's seat.
- After placement → the ball is **locked to the seat** and cannot be dragged again until a valid transfer moves it to the next seat.

### Seat Lock

- Once placed, the ball's Transform is parented to (or snapped to the position of) the cube's seat.
- No further tap or drag input acts on the ball until the game state advances.

---

## 5. Transfer Trigger

### When It Fires

A transfer check is triggered on **either** of the following events:

1. **Drag release** — the player lifts their finger after rotating a cube.
2. **Continuous alignment detection** — the system detects valid alignment while the player is still dragging (fires immediately without waiting for release).

### What the System Checks

| # | Condition | Description |
|---|-----------|-------------|
| 1 | **Angle** | Current rotation angle matches the required transfer angle. |
| 2 | **Hinge** | The rotating cube is using its designated hinge axis. |
| 3 | **Hole alignment** | Exit hole of the active cube aligns with entry hole of the next cube. |

All three conditions must be true simultaneously for the check to pass.

### Result

- **Valid** → execute seat-to-seat transfer: ball Transform snaps atomically from the source seat position to the destination seat position.
- **Invalid** → no transfer; ball remains in current seat; player continues rotating.

---

## 6. Constraints

| Rule | Detail |
|------|--------|
| No physics | No Rigidbody, no gravity, no collision forces. |
| No Rigidbody | Cubes and ball have no physics components. |
| Ball never rolls | Ball position is always a direct Transform assignment. |
| Ball only snaps between seats | Movement is seat-to-seat only; no interpolation or trajectory in Level 1. |
| Rotation is Transform-only | Cube rotation is applied via `Transform.localEulerAngles` or `Transform.Rotate`; no physics torque. |

---

## 7. Mobile Considerations

### Single-Touch Primary

- All gameplay interactions in Level 1 are driven by a single touch point.
- Only the first active touch is processed; additional fingers are ignored.

### Accidental Rotation Prevention

- A minimum **swipe distance threshold** must be exceeded before a drag is registered as a rotation intent.
- Taps shorter than the threshold are treated as tap events, not drags.
- The threshold prevents unintended cube rotation when the player is attempting to tap.

### Multi-Touch

- Multi-touch is recognized in the input layer but **ignored for Level 1**.
- No pinch, no two-finger rotate, no secondary touch action.

---

## Summary

| Action | Player gesture | System response |
|--------|---------------|-----------------|
| Select ball | Tap ball | Register active color |
| Place ball | Drag ball → release on cube | Lock ball to cube seat |
| Rotate cube | Drag cube face (past threshold) | Rotate around hinge axis (continuous) |
| Stop rotation | Release finger | Stop rotation; run transfer check |
| Transfer (drag release) | Finger lifted after valid alignment | Snap ball seat-to-seat |
| Transfer (live detection) | Valid alignment while still dragging | Snap ball seat-to-seat immediately |
| Invalid release | Finger lifted, conditions not met | No transfer; ball stays in place |
