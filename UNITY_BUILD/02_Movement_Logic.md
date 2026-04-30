# Level 1 — Movement Logic

---

## Overview

The movement system is **fully scripted** — no physics engine, no Rigidbody, no gravity. All cube rotation is driven by discrete 90-degree steps around fixed pivot points. The ball never moves on its own; it is snapped atomically from one `Seat` transform to the next when hole alignment is confirmed.

---

## 1. Hinge Identifiers & Rotation Sequences

Each hinge connects two adjacent cubes via a pivot at the shared top edge. Only the non-anchor cube in each pair can rotate.

### C12 — Blue ↔ Red (`Pivot_Blue_Red`)

| Property         | Value                                      |
|------------------|--------------------------------------------|
| Pivot position   | (X=+0.51, Y=0.00, Z=+1.00) — right top edge |
| Rotation axis    | Z-axis (`Vector3.forward`)                 |
| Rotatable cube   | Cube_Red (Blue is the anchor, never rotates) |
| Discrete steps   | 0° → 90° → 180° → 270° → 0° (wrapping)   |
| Positive (+) dir | Cube_Red opens away from Cube_Blue (lid-open) |
| Negative (−) dir | Cube_Red closes back toward Cube_Blue      |

**Rotation sequence (C12):**

```
State 0:   Cube_Red at   0° — holes facing +X / −X (default aligned with Blue)
State 1:   Cube_Red at  90° — holes facing +Z / −Z
State 2:   Cube_Red at 180° — holes facing −X / +X (anti-aligned with Blue)
State 3:   Cube_Red at 270° — holes facing −Z / +Z
```

---

### C13 — Red ↔ Green (`Pivot_Red_Green`)

| Property         | Value                                       |
|------------------|---------------------------------------------|
| Pivot position   | (X=0.00, Y=+0.51, Z=−0.51) — back top edge |
| Rotation axis    | X-axis (`Vector3.right`)                    |
| Rotatable cube   | Cube_Green                                  |
| Discrete steps   | 0° → 90° → 180° → 270° → 0° (wrapping)    |
| Positive (+) dir | Cube_Green opens away from Cube_Red         |
| Negative (−) dir | Cube_Green closes back toward Cube_Red      |

**Rotation sequence (C13):**

```
State 0:   Cube_Green at   0° — holes facing +X / −X (default aligned with Red)
State 1:   Cube_Green at  90° — holes facing +Y / −Y
State 2:   Cube_Green at 180° — holes facing −X / +X (anti-aligned with Red)
State 3:   Cube_Green at 270° — holes facing −Y / +Y
```

---

### C14 — Green ↔ Yellow (`Pivot_Green_Yellow`)

| Property         | Value                                       |
|------------------|---------------------------------------------|
| Pivot position   | (X=−0.51, Y=0.00, Z=+1.00) — left top edge |
| Rotation axis    | Z-axis (`Vector3.forward`)                  |
| Rotatable cube   | Cube_Yellow                                 |
| Discrete steps   | 0° → 90° → 180° → 270° → 0° (wrapping)    |
| Positive (+) dir | Cube_Yellow opens away from Cube_Green      |
| Negative (−) dir | Cube_Yellow closes back toward Cube_Green   |

**Rotation sequence (C14):**

```
State 0:   Cube_Yellow at   0° — holes facing +X / −X (default aligned with Green)
State 1:   Cube_Yellow at  90° — holes facing +Z / −Z
State 2:   Cube_Yellow at 180° — holes facing −X / +X (anti-aligned with Green)
State 3:   Cube_Yellow at 270° — holes facing −Z / +Z
```

---

### C15 — Yellow ↔ Blue (`Pivot_Yellow_Blue`)

C15 is the **closing hinge** of the Level 1 color cycle. It connects Cube_Yellow back to Cube_Blue, completing the ring: Blue → Red → Green → Yellow → Blue.

| Property         | Value                                                        |
|------------------|--------------------------------------------------------------|
| Pivot position   | (X=0.00, Y=+0.51, Z=+0.51) — front top edge, X midpoint    |
| Rotation axis    | X-axis (`Vector3.right`)                                     |
| Rotatable cube   | Cube_Yellow (Blue is the anchor, never rotates)              |
| Discrete steps   | 0° → 90° → 180° → 270° → 0° (wrapping)                     |
| Positive (+) dir | Cube_Yellow opens away from Cube_Blue (lid-open, front)     |
| Negative (−) dir | Cube_Yellow closes back toward Cube_Blue                     |

**Rotation sequence (C15):**

```
State 0:   Cube_Yellow at   0° — holes facing +X / −X (default aligned with Blue)
State 1:   Cube_Yellow at  90° — holes facing +Y / −Y
State 2:   Cube_Yellow at 180° — holes facing −X / +X (anti-aligned with Blue)
State 3:   Cube_Yellow at 270° — holes facing −Y / +Y
```

> **Scene hierarchy note:** `Pivot_Yellow_Blue`, `Hinge_Yellow_Blue`, and `RotationGroup_Yellow_Blue` must be present in the Level 1 scene alongside C12–C14. Cube_Yellow's `CubeRotationController` handles both its C14 (Green-side) and C15 (Blue-side) hole alignment checks; its world-space rotation angle is shared across both adjacent hinges.

---

## 2. Ball Transfer Trigger Condition

Transfer is triggered only when **all three of the following conditions are simultaneously true**:

| # | Condition | Detail |
|---|-----------|--------|
| 1 | **Holes aligned** | `HoleAlignmentDetector` reports that the `HoleAxis.forward` vectors of the two cubes sharing the active hinge are within **±5°** of each other in world space |
| 2 | **Correct hinge is active** | The aligned hinge connects the cube currently holding the ball to the **next cube in the shuffle order** |
| 3 | **No transfer in progress** | `BallTransferController.isTransferring == false` |

When all three conditions are met, a transfer event is fired immediately (no additional player input required).

---

## 3. Seat-to-Seat Transfer Logic

The ball exists at **exactly one** `Seat_<Color>` transform at all times. Transfer is **atomic** — the ball jumps directly from the source seat to the destination seat in a single frame (no interpolation in Level 1).

### Transfer steps (in order):

```
1. HoleAlignmentDetector fires OnHolesAligned(HingeID, CubeA, CubeB)
2. BallTransferController receives event
3. Check: is CubeA or CubeB == currentCube?          → if neither, ignore
4. Check: is the other cube == nextCubeInShuffleOrder? → if not, ignore
5. Check: isTransferring == false                     → if true, ignore
6. Set isTransferring = true
7. Set ball.position = destinationSeat.position
8. Update currentCube = destinationCube
9. Advance shuffleIndex by 1
10. Set isTransferring = false
11. Fire OnBallTransferred(sourceCube, destinationCube)
12. If shuffleIndex == totalCubes → fire OnLevelComplete
```

### Transfer table (Level 1 — all four hinges, closed ring):

| Active hinge | Ball moves from | Ball moves to   | Condition        |
|--------------|-----------------|-----------------|------------------|
| C12          | Seat_Blue       | Seat_Red        | Holes aligned    |
| C12          | Seat_Red        | Seat_Blue       | Holes aligned    |
| C13          | Seat_Red        | Seat_Green      | Holes aligned    |
| C13          | Seat_Green      | Seat_Red        | Holes aligned    |
| C14          | Seat_Green      | Seat_Yellow     | Holes aligned    |
| C14          | Seat_Yellow     | Seat_Green      | Holes aligned    |
| C15          | Seat_Yellow     | Seat_Blue       | Holes aligned    |
| C15          | Seat_Blue       | Seat_Yellow     | Holes aligned    |

> **Level 1 constraint:** Only adjacent transfers are valid. The shuffle order may be non-linear (e.g., Blue → Green), but the ball still physically travels step-by-step through intermediate cubes. The `BallTransferController` permits transfer only to cubes directly adjacent to the current cube via an aligned hinge, regardless of the shuffle order target.

---

## 4. State Machine for Movement

### States

```
IDLE
  The ball rests in a seat. No rotation in progress. Input is accepted.

ROTATING
  A cube is actively animating toward the next 90° snap angle.
  Ball transfers are blocked.
  Further input on the same cube is queued or ignored (one rotation at a time).

TRANSFER_PENDING
  Holes are aligned. Transfer condition has just been detected.
  The system is about to execute the seat snap.
  Rotation is blocked. Input is blocked.

TRANSFERRING
  Ball position is being set to the destination seat.
  (Atomic in Level 1 — this state lasts one frame.)

LEVEL_COMPLETE
  Ball has reached the final cube in the shuffle order.
  All input is blocked.
  LevelCompleteController is notified.
```

### Transitions

```
IDLE ──[player rotates cube]──────────────────────────────► ROTATING
ROTATING ──[snap angle reached]──────────────────────────► IDLE
ROTATING ──[snap angle reached AND holes aligned]────────► TRANSFER_PENDING
TRANSFER_PENDING ──[transfer executed]───────────────────► TRANSFERRING
TRANSFERRING ──[ball.position = dest, isTransferring=false]► IDLE
IDLE ──[shuffleIndex == totalCubes]──────────────────────► LEVEL_COMPLETE
```

### State diagram

```
         ┌──────────────────────────────────────────────────────┐
         │                                                      │
         ▼                                                      │
       IDLE ────[player input]────► ROTATING ─[snap done]──────┘
         ▲                              │
         │                 [snap done + holes aligned]
         │                              │
         │                              ▼
         │                    TRANSFER_PENDING
         │                              │
         │                  [BallTransferController executes]
         │                              │
         │                              ▼
         └──────────────────── TRANSFERRING ──[if final cube]──► LEVEL_COMPLETE
```

---

## 5. Rotation Rules Summary

| Rule | Detail |
|------|--------|
| Discrete steps only | Each rotation is exactly 90°; intermediate angles are only seen during the tween animation |
| One cube at a time | Only one `CubeRotationController` may be in ROTATING state simultaneously |
| Anchor immovable | Cube_Blue has no `CubeRotationController` and cannot rotate |
| Input blocked during transfer | All touch input is ignored while `isTransferring == true` |
| Snap tolerance | A snap is considered complete when the cube's Euler angle is within 1° of the target snap angle |
| Hole alignment check | Run every frame during ROTATING state; also run once on snap completion |
| Wrapping | After 270°, the next +90° rotation wraps to 0° (not 360°) |

---

## 6. Hole Alignment — World-Space Check

Each hinge has a `HoleAxis` child marker on both the hinge side and the rotation-group side. Alignment is valid when:

```
angle = Vector3.Angle(HoleAxisA.forward, HoleAxisB.forward)
isAligned = (angle <= 5.0f) || (angle >= 175.0f)
```

The second condition (`>= 175°`) catches the anti-parallel case (both holes face opposite directions — this is geometrically aligned for pass-through).

---

## Notes & Constraints

- No `Rigidbody` on any game object in the movement system
- No physics forces, gravity, or collision resolution
- No cube detachment — hinges are permanent parent-child relationships in the hierarchy
- All rotation is via pivot transforms; cube world-positions never change
- Ball position is always equal to exactly one `Seat_<Color>.position`
- Transfer is atomic and cannot be interrupted once triggered
