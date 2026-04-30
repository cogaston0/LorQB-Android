# 12 — Level Completion (Level 1)

## Overview

This document defines how a round ends and resets in Level 1. It covers the exact conditions that trigger `ROUND_COMPLETE`, what happens to every subsystem on completion, how success is communicated internally, how the round resets for replay, and how the failure path (`TIME_UP`) is handled. No code is included; this is a planning document only.

---

## 1. Completion Condition

A round is complete when **all three** of the following are true simultaneously:

| Condition | Requirement |
|-----------|-------------|
| `sequenceIndex` value | Must equal `3` |
| Ball location | Ball must be physically seated inside the cube whose color matches `sequence[3]` |
| Last transfer validity | The transfer that advanced `sequenceIndex` to `3` must have been a `VALID_READY` result from the Validation System |

The check is performed by the Game State Manager during `POST_TRANSFER`, immediately after `sequenceIndex` is incremented to `3`. No other state may trigger `ROUND_COMPLETE`.

---

## 2. On Completion

When the completion condition is satisfied the Game State Manager transitions to `ROUND_COMPLETE`. The following actions occur **at the moment of that transition**:

| Action | Detail |
|--------|--------|
| State change | Game State Manager sets active state to `ROUND_COMPLETE` |
| Timer stops | The countdown timer is frozen at its current value; the elapsed time is captured as the player's completion time (see Step 11) |
| Input disabled | All touch input is disabled — no tap, drag, or release event is forwarded to any subsystem |
| Ball frozen | Ball remains stationary inside the final cube (`sequence[3]`); it is not moved, snapped, or reset |
| Transfers blocked | No further validation pipeline runs are permitted; release events are swallowed |
| Sequence advancement blocked | `sequenceIndex` remains at `3`; no further increment occurs |

The system holds `ROUND_COMPLETE` until the player explicitly initiates a reset (see Section 4). There is no automatic timeout or replay.

---

## 3. Feedback

Feedback is expressed in game logic only. No UI design is specified here.

| Event | Logic signal |
|-------|-------------|
| Round complete | Internal `roundCompleted` flag is set to `true` |
| Sequence marked | The active sequence is flagged as completed; a new sequence will be generated on the next reset |
| Completion time recorded | The elapsed time value captured when the timer stopped is written to a `completionTime` record for potential scoring use |
| Failed attempt (TIME_UP) | `roundCompleted` remains `false`; no completion time is recorded |

The `roundCompleted` flag and `completionTime` record are the two logical outputs of a successful round. Any future UI layer or analytics system reads these values; neither is produced here.

---

## 4. Reset Behavior

Reset is triggered by an explicit player action after `ROUND_COMPLETE` (or after `TIME_UP`). The following operations are performed in order:

| Step | Action | Detail |
|------|--------|--------|
| 1 | Reset `sequenceIndex` | Set to `0` |
| 2 | Generate new sequence | Randomly select one of the 24 permutations of Blue, Red, Green, Yellow; store in `sequence[0..3]` |
| 3 | Reset cube rotations | All three pivot groups (Blue_Red, Red_Green, Green_Yellow) return to their base rotation (0°) on their respective hinge axes (Z, X, Z) |
| 4 | Reset ball state | Ball state is set to `FREE`; ball is detached from any seat and returned to its neutral pre-placement position |
| 5 | Clear completion flags | `roundCompleted` is set to `false`; `completionTime` record is cleared |
| 6 | Advance to `BALL_SELECTION` | Game State Manager transitions to `BALL_SELECTION`; the new sequence is displayed and the player may begin the next round |

The timer is not restarted at this point. It restarts only when the ball is placed into the first cube and the state advances to `ACTIVE_PLAY` (see Step 11).

---

## 5. Failure Condition

Failure occurs when the round timer reaches zero before `ROUND_COMPLETE` is reached.

| Condition | Behavior |
|-----------|----------|
| `TIME_UP` trigger | The Game State Manager immediately enters `TIME_UP`; this transition takes priority over any pending `VALIDATION`, `TRANSFER`, or `POST_TRANSFER` operation |
| Ball location | Ball remains in whatever cube it was seated in when the timer expired; it is not moved or reset |
| No completion | `ROUND_COMPLETE` is **not** entered; `roundCompleted` remains `false` |
| No completion time | No `completionTime` value is recorded |
| Input | All input is disabled on entry to `TIME_UP` |
| Recovery | Player must initiate a reset to begin a new round; see Section 4 for reset behavior |

There is no partial credit, no score, and no incremental save of progress when a round ends in `TIME_UP`.

---

## 6. Constraints

| Constraint | Detail |
|------------|--------|
| No auto replay | The game does not start a new round automatically after `ROUND_COMPLETE` or `TIME_UP`; the player must explicitly trigger a reset |
| No automatic move execution | The completion or reset path never initiates a cube rotation or ball snap on behalf of the player |
| No physics | No `Rigidbody`, forces, or physics queries are used in any completion or reset operation |
| Single completion point | `ROUND_COMPLETE` can only be entered from `POST_TRANSFER`; no shortcut path exists |
| Single failure point | `TIME_UP` is the only failure-terminal state; no other event terminates the round as a failure |
| Ball not auto-removed | On completion the ball is not despawned or animated away; it sits statically in the final cube until reset |
| Sequence not re-used | The sequence that was completed (or failed) is discarded; a fresh permutation is always generated on reset |

---

## Scope Constraints

- This document covers **planning only**.
- No C# code, no UI design, no Unity scene work is included here.
- Implementation details (MonoBehaviour hooks, event bindings, coroutines, reset triggers) are deferred to later build documents.
