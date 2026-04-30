# 08 — Ball System (Level 1)

## Overview

The ball is the single interactive object the player moves through the four cubes. It has no physics simulation of any kind. Its world position is always equal to the world position of the Seat it currently occupies. All movement is atomic, scripted, and instantaneous.

---

## 1. Core Rules

| Rule | Detail |
|------|--------|
| No physics | The ball has no `Rigidbody` component and is never subject to gravity, forces, velocity, or collision response |
| No rolling | The ball never translates or rotates continuously — it either sits still or jumps |
| Single owner | The ball belongs to exactly one cube at a time; before the first round placement it belongs to no cube |
| Position = Seat | `ball.position` is always set to `currentSeat.position`; no interpolation occurs in Level 1 |

---

## 2. Seat System

Each cube has exactly one Seat — a named empty `GameObject` positioned at the top-centre of the cube (Y = +0.5 above the cube centre in local space).

**Critical:** Each `Seat_<Color>` is a **child of its cube** (or of the cube's active rotation group). This means the Seat moves in world space whenever its cube is rotated. The ball tracks the Seat in world space at all times, so it moves with the cube during active rotation.

### Seat default world positions (rotation = 0°)

| Seat | Parent | X | Y | Z |
|------|--------|-----|------|-----|
| Seat_Blue | Cube_Blue | +1.0 | +0.5 | −1.0 |
| Seat_Red | Cube_Red | +1.0 | +0.5 | +1.0 |
| Seat_Green | Cube_Green | −1.0 | +0.5 | +1.0 |
| Seat_Yellow | Cube_Yellow | −1.0 | +0.5 | −1.0 |

### Scene hierarchy

Each Seat is a direct child of its cube so it inherits the cube's rotation:

```
Level1
└── Cubes
    ├── Cube_Blue
    │   └── Seat_Blue      (local: 0, +0.5, 0)
    ├── Cube_Red
    │   └── Seat_Red       (local: 0, +0.5, 0)
    ├── Cube_Green
    │   └── Seat_Green     (local: 0, +0.5, 0)
    └── Cube_Yellow
        └── Seat_Yellow    (local: 0, +0.5, 0)
```

When a cube is part of a `RotationGroup`, its `Seat_<Color>` child rotates with the group, keeping the ball visually inside the cube throughout the rotation.

### Seat wiring

Each `CubeIdentifier` component holds a serialized reference to its matching `Seat_<Color>` Transform. `BallTransferController` looks up the seat's **current world position** to track or snap the ball.

---

## 3. Ball GameObject

| Property | Value |
|----------|-------|
| Name | `Ball` |
| Parent | `Level1` (root) |
| Scale | (0.4, 0.4, 0.4) |
| Starting position | Seat_Blue (+1.0, +0.5, −1.0) |
| Components | `MeshFilter`, `MeshRenderer` |
| **No** `Rigidbody` | Position is managed entirely by script |

---

## 4. Ownership Model

| State | Owner |
|-------|-------|
| Before first round placement | None (ball exists at Seat_Blue visually but shuffle has not started) |
| During play | The cube whose `Seat_<Color>` position equals `ball.position` |
| During transfer | The ball is being moved; ownership transitions from source to destination in the same frame |

Ownership is tracked by `BallTransferController._currentStepIndex`, which is an index into `shuffleOrder[]`. `currentCube = shuffleOrder[_currentStepIndex]`.

---

## 5. Transfer — Seat-to-Seat Snap

Transfer is the only way the ball moves. It is **atomic**: the ball's position changes to the destination seat in a single operation with no intermediate frames.

### Transfer execution (simplified)

```
1. CanTransfer() check:
     - _isTransferring == false
     - _currentStepIndex < shuffleOrder.Length - 1
     - HoleAlignmentDetector.AreHolesAligned() == true for the current pair

2. _isTransferring = true
3. _currentStepIndex = nextStep
4. ball.position = seatTransforms[(int)shuffleOrder[nextStep]].position
5. _isTransferring = false
6. OnBallTransferred fires
7. If _currentStepIndex == shuffleOrder.Length - 1 → OnLevelComplete fires
```

### Valid transfers (Level 1 — closed ring of four hinges)

| Active hinge | From seat | To seat |
|--------------|-----------|---------|
| C12 (Blue ↔ Red) | Seat_Blue | Seat_Red |
| C12 (Blue ↔ Red) | Seat_Red | Seat_Blue |
| C13 (Red ↔ Green) | Seat_Red | Seat_Green |
| C13 (Red ↔ Green) | Seat_Green | Seat_Red |
| C14 (Green ↔ Yellow) | Seat_Green | Seat_Yellow |
| C14 (Green ↔ Yellow) | Seat_Yellow | Seat_Green |
| C15 (Yellow ↔ Blue) | Seat_Yellow | Seat_Blue |
| C15 (Yellow ↔ Blue) | Seat_Blue | Seat_Yellow |

Transfer direction is determined by which cube currently holds the ball and which cube is next in the shuffle order. Only adjacent transfers (same hinge) are valid.

---

## 6. Ball During Cube Rotation

While the player holds/drags a cube, the ball **follows the current Seat in world space**. Because the Seat is a child of the cube (or its rotation group), it moves as the cube rotates. The ball tracks `currentSeat.position` every frame, keeping it visually inside the current cube throughout the drag.

| Phase | Ball behaviour |
|-------|---------------|
| Player begins drag / hold | Ball remains at `currentSeat.position`; Seat starts moving as the cube rotates |
| Active rotation (finger held) | Ball position is updated to `currentSeat.position` each frame — ball follows the cube |
| No transfer during hold | Transfer is **blocked** while the player is actively holding/rotating; the ball cannot jump mid-drag |
| Player releases finger | System evaluates hole alignment |
| Release — valid alignment | Ball snaps to `destinationSeat.position` (transfer executes) |
| Release — invalid alignment | Ball remains at `currentSeat.position` (no transfer; ball stays in current cube) |

### Summary

1. Ball follows the current Seat during cube rotation.
2. The current Seat is attached to the cube (or rotation group) that owns the ball, so it moves with it.
3. Ball remains visually inside the current cube during active touch/drag.
4. On release, if alignment is valid, ball snaps to the destination Seat.
5. On release, if alignment is invalid, ball remains in the current cube at the current Seat.

---

## 7. No-Physics Invariants

The following must always be true at runtime:

- `Ball` has no `Rigidbody`
- `ball.position` always equals `currentSeat.position` (tracked every frame)
- The current Seat moves with its cube during rotation; the ball therefore moves with the cube
- `ball.velocity` does not exist (no Rigidbody)
- `ball.rotation` is not programmatically changed (orientation is irrelevant)
- No physics forces or impulses are ever applied to the ball
- Transfer is blocked while the player is actively holding/rotating; ball stays in the current cube until release

---

## 8. Script Responsibility Summary

| Script | Role |
|--------|------|
| `BallTransferController` | Owns the ball Transform reference, executes seat-based snaps, tracks `_currentStepIndex` |
| `CubeIdentifier` | Exposes `SeatTransform` and `SeatPosition` for its cube |
| `HoleAlignmentDetector` | Provides `AreHolesAligned()` — the gate condition for transfer |
| `Level1SceneBuilder` | Creates the `Ball` GO (no Rigidbody), creates all `Seat_<Color>` GOs, wires seats into `CubeIdentifier` |

---

## Scope Constraints

- This document covers **planning only**.
- No C# code, no UI design, no Unity scene work is included here.
- Implementation details (MonoBehaviour hooks, event bindings, animator states) are deferred to later build documents.
