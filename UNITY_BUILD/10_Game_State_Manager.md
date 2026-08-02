# 10 — Game State Manager (Level 1)

## Overview

The Game State Manager controls the flow of a Level 1 round from start to finish. It is the single authority on which phase the game is currently in, what each subsystem is allowed to do in that phase, and how a transition from one phase to another is triggered. All other systems (Touch Input, Ball, Validation, Sequence) defer to the active game state before taking any action.

---

## 1. Purpose

| Responsibility | Detail |
|----------------|--------|
| Flow control | Dictates the active phase of the game at all times |
| Phase tracking | Maintains a single `GameState` value that all systems can read |
| System coordination | Enables or disables input, validation, ball movement, and sequence updates based on the current state |
| Transition authority | Only the Game State Manager may change the active state; subsystems report events to it and wait for a new state in return |

---

## 2. Main States

| State | Description |
|-------|-------------|
| `IDLE` | Initial state before the round has begun; ball exists but no sequence has been issued and no input is processed |
| `BALL_SELECTION` | Sequence has been generated and displayed; the player must tap the ball whose color matches `sequence[0]` |
| `BALL_PLACEMENT` | A ball of the correct color has been selected; the player must drag it into the first required cube |
| `ACTIVE_PLAY` | Ball is seated in a cube; the player is free to rotate cubes — validation is armed but not yet running |
| `VALIDATION` | Player has released their touch; the 10-check validation pipeline is executing synchronously |
| `TRANSFER` | Validation result was `VALID_READY`; ball is being snapped atomically from the source seat to the destination seat |
| `POST_TRANSFER` | Snap is complete; `sequenceIndex` is being incremented and the system checks whether the round is over |
| `ROUND_COMPLETE` | `sequenceIndex` has reached `3`; the ball is on `sequence[3]` and the round is finished |
| `TIME_UP` | The round timer has expired; this state can be entered from any other state at any moment |

---

## 3. State Transitions

### Normal flow

```
IDLE
  └─► BALL_SELECTION        (round starts; sequence generated and displayed)

BALL_SELECTION
  └─► BALL_PLACEMENT        (player taps ball matching sequence[0])

BALL_PLACEMENT
  └─► ACTIVE_PLAY           (player drags ball into first required cube; placement confirmed)

ACTIVE_PLAY
  └─► VALIDATION            (player releases touch after rotating a cube)

VALIDATION
  ├─► TRANSFER              (all 10 validation checks pass → VALID_READY)
  └─► ACTIVE_PLAY           (any validation check fails → INVALID; player may retry)

TRANSFER
  └─► POST_TRANSFER         (atomic seat-to-seat snap is complete)

POST_TRANSFER
  ├─► ACTIVE_PLAY           (sequenceIndex < 3; round continues)
  └─► ROUND_COMPLETE        (sequenceIndex == 3; ball on sequence[3])
```

### Timer override (any state)

```
ANY STATE
  └─► TIME_UP               (round timer reaches zero; fires regardless of current state)
```

The `TIME_UP` transition takes priority over all other pending transitions. No other state change is processed once the timer has expired.

---

## 4. Responsibilities Per State

### Input enabled / disabled

| State | Touch input |
|-------|-------------|
| `IDLE` | Disabled — no gesture is processed |
| `BALL_SELECTION` | Tap on ball only; cube drag is disabled |
| `BALL_PLACEMENT` | Drag on selected ball only; cube drag is disabled |
| `ACTIVE_PLAY` | Cube drag/rotate enabled; ball tap disabled (ball is already placed) |
| `VALIDATION` | All input disabled — pipeline is running |
| `TRANSFER` | All input disabled — snap is executing |
| `POST_TRANSFER` | All input disabled — index update is executing |
| `ROUND_COMPLETE` | All input disabled |
| `TIME_UP` | All input disabled |

### Validation allowed / not allowed

| State | Validation pipeline |
|-------|---------------------|
| `ACTIVE_PLAY` | Armed — will fire immediately on release event |
| `VALIDATION` | Running — 10 checks executing |
| All other states | Not allowed; release events are ignored or do not occur |

### Ball movement allowed / not allowed

| State | Ball movement |
|-------|---------------|
| `BALL_PLACEMENT` | Ball follows player drag toward the first cube seat |
| `ACTIVE_PLAY` | Ball tracks `currentSeat.position` in world space each frame (passive follow — moves with cube during rotation) |
| `TRANSFER` | Ball snaps atomically to `destinationSeat.position` (one frame) |
| All other states | Ball position is frozen at `currentSeat.position`; no movement of any kind |

### Sequence index updates

| State | `sequenceIndex` |
|-------|-----------------|
| `IDLE` | Reset to `0` |
| `BALL_SELECTION` | Reads `0` (does not change) |
| `BALL_PLACEMENT` | Reads `0` (does not change) |
| `ACTIVE_PLAY` | Reads current value (does not change) |
| `VALIDATION` | Reads current value (does not change) |
| `TRANSFER` | Does not change yet — snap fires first |
| `POST_TRANSFER` | Increments by `1`; triggers round-complete check |
| `ROUND_COMPLETE` | Reads `3`; no further increment |
| `TIME_UP` | Frozen at whatever value it held when the timer expired |

---

## 5. Integration

### Sequence Model (Step 07)

- `IDLE` → `BALL_SELECTION`: the Sequence Model generates one of the 24 permutations and exposes `sequence[0..3]`.
- `BALL_SELECTION`: the Game State Manager reads `sequence[0]` to determine which ball color is required.
- `POST_TRANSFER`: the Game State Manager increments `sequenceIndex` and reads `sequence[sequenceIndex]` to determine the next required move.
- `ROUND_COMPLETE` fires when `sequenceIndex` reaches `3`.

### Validation System (Step 09)

- The Game State Manager enters `VALIDATION` on every player release event during `ACTIVE_PLAY`.
- The Validation System runs its 10-check pipeline and returns either `VALID_READY` or `INVALID`.
- The Game State Manager transitions to `TRANSFER` on `VALID_READY` or back to `ACTIVE_PLAY` on `INVALID`.
- The Validation System does not change state itself — it only returns a result to the Game State Manager.

### Ball System (Step 08)

- `BALL_PLACEMENT`: the Ball System accepts a placement command and seats the ball in the first cube.
- `ACTIVE_PLAY`: the Ball System tracks `currentSeat.position` every frame.
- `TRANSFER`: the Ball System executes the atomic snap to `destinationSeat.position`.
- The Ball System queries the Game State Manager before acting; it refuses to snap or move if the state does not permit it.

### Touch Input (Step 06)

- Touch Input reads the current game state before dispatching any gesture event.
- Tap events on the ball are forwarded only during `BALL_SELECTION`.
- Drag events on the ball are forwarded only during `BALL_PLACEMENT`.
- Cube drag events are forwarded only during `ACTIVE_PLAY`.
- Release events that trigger validation are forwarded only during `ACTIVE_PLAY`.
- All touch events are swallowed (not forwarded) during `VALIDATION`, `TRANSFER`, `POST_TRANSFER`, `ROUND_COMPLETE`, and `TIME_UP`.

---

## 6. Constraints

| Constraint | Detail |
|------------|--------|
| No automatic moves | The Game State Manager never initiates a cube rotation or ball snap on behalf of the player; every transition into `TRANSFER` is caused by a valid player release |
| No physics | No `Rigidbody`, forces, or physics queries are involved in any state or transition |
| Player-driven transitions | All transitions from `IDLE` through `ROUND_COMPLETE` are triggered by explicit player actions (tap, drag, release); the only exception is `TIME_UP`, which is triggered by the timer |
| Single active state | The Game State Manager holds exactly one `GameState` value at any given frame; no blended or parallel states exist |
| Subsystems are passive | Touch Input, Ball System, Validation System, and Sequence Model do not change game state themselves — they report events and return results; the Game State Manager is the only writer of the active state |
| No re-entry guards needed for timer | Because `TIME_UP` is the sole timer-driven transition and is unconditional, no re-entry check is required; the timer fires once and the state is set |

---

## Scope Constraints

- This document covers **planning only**.
- No C# code, no UI design, no Unity scene work is included here.
- Implementation details (MonoBehaviour hooks, event bindings, coroutines) are deferred to later build documents.
