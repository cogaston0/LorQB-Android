# 06 — Touch Input Model

## Overview

This document defines how the player controls cubes and the ball in LorQB Android via touch gestures.  
No code, no UI design, no Unity scene — interaction model only.

### Core rule

> The player may touch a cube from **any visible side**.  
> The touched side does **not** determine free rotation.  
> The system always resolves the cube's allowed hinge axis from the Move Map and constrains rotation to that axis only.  
> The ball never transfers while the player is actively touching or rotating a cube.  
> Transfer is checked **only after the player releases**.

---

## 1. Input Types

| Gesture | Target | Action |
|---------|--------|--------|
| **Tap** | Ball | Select ball color |
| **Drag** | Ball | Place ball into first required cube |
| **Touch / drag** | Any visible face of a cube | Rotate the selected cube around its valid hinge axis |
| **Release** | — | Stop rotation; run release validation check |

- The touched face or side of a cube is used only to identify **which cube** was selected — it does not alter the rotation axis.
- A drag shorter than the minimum swipe threshold is discarded (see §6).
- A touch that does not land on a cube or ball is ignored.

---

## 2. Cube Interaction

### Any-Side Touch

- The player can initiate a cube grab from any visible face (top, front, side).
- The system uses the touch hit to resolve which cube object was contacted.
- Once the cube is identified, the system looks up that cube's allowed hinge and pivot from the Move Map.
- Rotation is locked to that hinge axis only — no free 3D rotation is possible regardless of which face was touched.

### Hinge Assignment (from Move Map / Level1SceneBuilder)

| Cube pair | Hinge shortcode | Unity pivot name | Unity rotation axis |
|-----------|----------------|------------------|---------------------|
| Blue / Red | HBR | `Pivot_Blue_Red` | Z-axis |
| Red / Green | HRG | `Pivot_Red_Green` | X-axis |
| Green / Yellow | HGY | `Pivot_Green_Yellow` | Z-axis |

### Rules

- Only the **moving** cube in a pair rotates; the anchor cube does not move.
- One cube is active at a time (single-touch, see §6).
- Swipe direction (left/right or up/down relative to the hinge axis) determines rotation direction.
- The system reads the current angle in real time while the player is dragging.

---

## 3. Ball Behavior During Active Touch

- While the player is touching or dragging a cube, the ball remains **locked to the seat** of the cube that currently holds it.
- The ball does **not** transfer during an active touch/hold, regardless of angle or alignment.
- The ball cannot fall, roll, or move by any means while the player is holding the cube.
- No transfer logic runs during the drag phase.

---

## 4. Release Validation

Transfer is evaluated **only when the player lifts their finger** (drag release event).

### Conditions checked in order

| # | Condition | Description |
|---|-----------|-------------|
| 1 | **Active cube** | Identifies which cube currently holds the ball. |
| 2 | **Next required color** | Checks which color the sequence index expects next. |
| 3 | **Correct hinge / move rule** | Confirms the cube's hinge matches the valid move from the Move Map for the current sequence step. |
| 4 | **Valid angle** | Current rotation angle satisfies the transfer angle requirement. |
| 5 | **Hole alignment** | Exit hole of the source cube aligns with entry hole of the destination cube. |
| 6 | **Destination seat available** | Target seat is empty and ready to receive the ball. |

All six conditions must pass simultaneously.

### If valid

- Ball Transform snaps atomically from the source seat position to the destination seat position (no interpolation in Level 1).
- Sequence index advances to the next step.

### If invalid

- Ball remains in the current cube's seat.
- No sequence change.
- Player can continue rotating and release again.

---

## 5. Constraints

| Rule | Detail |
|------|--------|
| No physics | No Rigidbody, no gravity, no collision forces. |
| No Rigidbody | Cubes and ball have no physics components. |
| No rolling ball | Ball position is always a direct Transform assignment. |
| No automatic move execution | The system never moves the ball autonomously — only a valid player release triggers transfer. |
| No transfer during hold | Ball cannot transfer while the player's finger is down on a cube. |
| Only player-driven rotation | Cubes do not rotate on their own; rotation is always player-initiated via touch drag. |
| Seat-to-seat snap only | Ball movement is seat-to-seat only; no trajectory, no lerp in Level 1. |

---

## 6. Mobile Considerations

### Single-Touch Primary

- All gameplay interactions in Level 1 are driven by a single touch point.
- Only the first active touch is processed; additional fingers are ignored.

### Swipe Threshold

- A minimum swipe distance threshold must be exceeded before a touch-drag is registered as a rotation intent.
- Touches below the threshold are treated as taps, not drags.
- This prevents accidental cube rotation when the player intends to tap.

### Touch Side — Selection Only

- Which face of the cube the player touches affects only the **cube selection** result.
- It does not determine the rotation axis or allow free rotation in an arbitrary direction.
- The hinge axis is always resolved from the Move Map, not from the touch normal.

### Multi-Touch

- Multi-touch input is received by the device but **ignored for Level 1**.
- No pinch, no two-finger rotate, no secondary touch action.

---

## Summary

| Action | Player gesture | System response |
|--------|---------------|-----------------|
| Select ball color | Tap ball | Register active color |
| Place ball | Drag ball → release on first required cube | Lock ball to cube seat |
| Rotate cube | Touch any face, drag past threshold | Identify cube; look up valid hinge; rotate continuously around hinge axis |
| Hold cube | Finger held down while dragging | Ball stays locked to current seat; no transfer |
| Release cube | Lift finger | Stop rotation; run 6-condition release validation |
| Valid release | All 6 conditions pass | Snap ball seat-to-seat; advance sequence index |
| Invalid release | Any condition fails | Ball stays in current cube; no sequence change |
