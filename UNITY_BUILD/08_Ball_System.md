# 08 — Ball System (Level 1)

## Overview

The ball is a single, non-physics object whose position is fully controlled by script. It exists at exactly one Seat at all times (except before the first placement, when it has no owner). There is no physics simulation, no gravity, no velocity, and no rolling.

---

## 1. Core Rules

| Rule | Detail |
|------|--------|
| **No physics** | The Ball GameObject has no Rigidbody component. |
| **No rolling** | The ball never moves continuously through space; it teleports atomically between Seats. |
| **Single owner** | The ball always belongs to exactly one cube (via that cube's Seat), or to no cube before the player makes the first placement. |
| **Position defined by Seat** | `ball.transform.position` is always set to equal the active `Seat_<Color>.position`. It is never calculated independently. |

---

## 2. Seat System

Each cube has exactly one predefined Seat. A Seat is an empty GameObject positioned at the top-centre of its cube (Y = +0.5 above the cube centre). Seats are snap-target Transforms — the ball's position is always assigned directly from a Seat.

### Seat Definitions

| Seat name       | Owner cube    | World position (X, Y, Z) | Notes                        |
|-----------------|---------------|--------------------------|------------------------------|
| `Seat_Blue`     | `Cube_Blue`   | (+1.0, +0.5, −1.0)       | Ball starts here at round start |
| `Seat_Red`      | `Cube_Red`    | (+1.0, +0.5, +1.0)       |                              |
| `Seat_Green`    | `Cube_Green`  | (−1.0, +0.5, +1.0)       |                              |
| `Seat_Yellow`   | `Cube_Yellow` | (−1.0, +0.5, −1.0)       |                              |

All four Seats are placed under the `Seats` group in the scene hierarchy and are wired into their respective `CubeIdentifier` component via the `seatTransform` serialized field.

### Seat Ownership Rule

- Each Seat is permanently associated with one cube. Seat ownership never changes.
- When the ball transfers, the system sets `ball.transform.position = destinationSeat.position`. The ball now "belongs to" the destination cube.
- There is no intermediate state: ownership switches atomically at the transfer frame.

---

## 3. Ball Object Properties

| Property              | Value                            |
|-----------------------|----------------------------------|
| GameObject name       | `Ball`                           |
| Parent                | `Level1` (root)                  |
| Initial position      | `Seat_Blue` (+1.0, +0.5, −1.0)  |
| Scale                 | (0.4, 0.4, 0.4) — sphere mesh    |
| Rigidbody             | **None**                         |
| Collider              | **None** (ball is not raycasted) |
| Physics simulation    | **Disabled entirely**            |

---

## 4. Ball State Model

The ball is always in one of three states:

| State         | Condition                                          | Ball position |
|---------------|----------------------------------------------------|---------------|
| **Unplaced**  | Round start, before first player placement         | At `Seat_Blue` (default spawn) — no active cube |
| **Seated**    | Ball is resting on an active cube's Seat           | `= activeSeat.position` |
| **In Transit**| Transfer has been triggered; snap is executing     | Moves atomically to `destinationSeat.position` in the same frame |

There is no "in flight" or "rolling" state. The transition from **Seated** to **Seated** (via a new cube) is instantaneous — no lerp, no arc, no physics.

---

## 5. Transfer Behavior

Ball transfer is the only event that changes which Seat the ball belongs to. It is always:

- **Atomic** — executes in a single frame; cannot be interrupted mid-transfer.
- **Scripted** — `BallTransferController` sets `ball.transform.position = targetSeat.position`.
- **Validated** — transfer only fires when `HoleAlignmentDetector` confirms alignment on the active hinge.
- **Seat-to-seat** — the source and destination are always named Seats; no world positions are hard-coded in the transfer logic.

### Transfer Triggers by Move Series

| Series | Transfer frame | Trigger condition |
|--------|---------------|-------------------|
| C-series (C12, C13, C14, C15) | Frame 120 → 121 | Hinge reaches 180° peak |
| T-series (T01, T02, T03, T04) | Frame 161       | Both hinge stages complete |

See `04_Blender_To_Android_Move_Map.md` for per-move seat source and destination details.

---

## 6. Sequence Integration

The ball's current Seat must match the expected color at each step of the active sequence:

- Before a transfer is validated, `BallTransferController` checks that the source Seat color equals `sequence[sequenceIndex]` and the destination Seat color equals `sequence[sequenceIndex + 1]`.
- If the check fails, the ball stays at its current Seat and no ownership change occurs.
- If the check passes, ownership transfers and `sequenceIndex` increments.

See `07_Sequence_Shuffle_Model.md` for full sequence and validation rules.

---

## 7. Scene Hierarchy Reference

```
Level1
├── Seats
│   ├── Seat_Blue    (+1.0, +0.5, −1.0)
│   ├── Seat_Red     (+1.0, +0.5, +1.0)
│   ├── Seat_Green   (−1.0, +0.5, +1.0)
│   └── Seat_Yellow  (−1.0, +0.5, −1.0)
└── Ball             (+1.0, +0.5, −1.0)  ← starts at Seat_Blue
```

Seats are siblings of the Ball under `Level1`, not children of their respective cubes. This ensures Seat world positions remain fixed even when cubes rotate around their pivot hinges.

---

## 8. Script Responsibilities

| Script                    | Role related to the ball |
|---------------------------|--------------------------|
| `BallTransferController`  | Owns the transfer logic; sets `ball.transform.position = seat.position` |
| `CubeIdentifier`          | Holds a reference to the cube's `seatTransform`; used by `BallTransferController` to resolve seat positions |
| `HoleAlignmentDetector`   | Fires the alignment event that unlocks transfer; does not move the ball itself |
| `GameManager`             | Tracks which cube currently owns the ball; coordinates sequence validation with transfer |

---

## Scope Constraints

- This document covers **planning only**.
- No C# code, no UI design, no Unity scene work is included here.
- Implementation details (MonoBehaviour hooks, event bindings, exact field names) are deferred to later build documents.
