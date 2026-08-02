# 09 — Validation System (Level 1)

## Overview

The validation system determines whether a player-initiated move is legal and whether the ball may transfer to the next cube. Validation is passive during active hold and fires once on release. The system never auto-executes moves and applies no physics of any kind.

---

## 1. Purpose

| Property | Detail |
|----------|--------|
| Role | Evaluate whether a completed player action satisfies all move rules |
| Trigger | Player releases touch after rotating a cube |
| Scope | Level 1 only (four-cube, three-hinge ring) |
| Execution model | No automatic movement; the player physically performs every rotation |
| Hold phase | Monitor only — no validation occurs while the finger is held |
| Release phase | Full validation runs; result determines whether transfer fires |

The system does not move the ball on behalf of the player. It only inspects state after release and either permits or blocks the transfer.

---

## 2. Required Validation Checks

All ten conditions below must be true simultaneously for a transfer to be permitted. If any single check fails, the entire validation result is `INVALID`.

| # | Check | Requirement |
|---|-------|-------------|
| 1 | **Ball placement** | Ball is present in a valid starting cube (not unplaced / pre-round state) |
| 2 | **Current cube matches sequence** | `currentCube.color == sequence[sequenceIndex]` |
| 3 | **Destination cube matches sequence** | `destinationCube.color == sequence[sequenceIndex + 1]` |
| 4 | **Move exists in Move Map** | The (source, destination) pair must appear as a valid entry in the Move Map (C12, C13, C14, C15, T01, T02, T03, T04) |
| 5 | **Correct hinge is active** | The hinge used matches the Move Map entry for the current (source, destination) pair |
| 6 | **Correct rotation angle reached** | Hinge angle is within the valid transfer window for the move series (see Section 4) |
| 7 | **Holes are aligned** | `HoleAlignmentDetector.AreHolesAligned()` returns `true` for the active hinge pair |
| 8 | **Destination seat exists** | A valid `Seat_<Color>` Transform is reachable for the destination cube |
| 9 | **Player has released** | Touch input state is Released; active hold flag is `false` |
| 10 | **Ball is not already transferring** | `_isTransferring == false` |

---

## 3. Validation Timing

```
HOLD PHASE (finger down / cube rotating)
  └─ Monitor only
     ├─ Ball follows currentSeat.position each frame
     ├─ No transfer permitted
     └─ No validation runs

RELEASE EVENT (finger lifted)
  └─ Validation pipeline runs (all 10 checks)
     ├─ Result: INVALID
     │    └─ Ball remains at currentSeat.position
     │    └─ sequenceIndex does not advance
     │    └─ Player may retry
     └─ Result: VALID_READY
          └─ Ball snaps atomically to destinationSeat.position
          └─ sequenceIndex increments by 1
          └─ If sequenceIndex reaches 3 → ROUND_COMPLETE
```

| Phase | Ball behaviour | Validation |
|-------|---------------|------------|
| Active hold | Follows `currentSeat.position` with the rotating cube | None |
| On release — invalid | Stays at `currentSeat.position` | Checks ran; result is INVALID |
| On release — valid | Snaps to `destinationSeat.position` (atomic) | Checks ran; result is VALID_READY → TRANSFER_COMPLETE |

---

## 4. Move Validation — Series-Specific Rules

### C-Series (single-hinge, full arc)

| Property | Value |
|----------|-------|
| Moves | C12, C13, C14, C15 |
| Angle sequence | 0° → 90° → 180° → 90° → 0° |
| Transfer window | 180° peak (normalized frame 120 → 121) |
| Alignment condition | Anti-parallel: holes face each other across the hinge at 180° |
| Android note | Use normalized hinge angle state, not absolute Blender timeline position |

Transfer is valid only if the hinge reaches exactly the 180° peak before the player releases. If the player releases at any angle other than 180° the check for condition 6 fails and no transfer occurs.

### T-Series (multi-stage, non-adjacent)

| Property | Value |
|----------|-------|
| Moves | T01, T02, T03, T04 |
| Forward move | Two stages (Stage 1: frames 1–80; Stage 2: frames 81–160) |
| Transfer window | Normalized frame-equivalent stage 161, after both stages reach their target angles |
| Alignment condition | Both hinges must have reached their required stage angles simultaneously |
| Android note | Android uses normalized timing and state, not absolute Blender timeline offsets (e.g. 241–480) |

Transfer fires at the frame-161-equivalent moment — the instant both staged hinges are at their required positions and holes are confirmed aligned.

### Axis Reference

| Hinge | Unity pivot | Blender axis | C-series transfer angle | T-series stage angle |
|-------|-------------|--------------|------------------------|----------------------|
| HBR (`Pivot_Blue_Red`) | (+1.0, +0.5, 0.0) | X | 180° | 180° (Stage 1) |
| HRG (`Pivot_Red_Green`) | ( 0.0, +0.5, +1.0) | Y | 180° | 90° (Stage 2) |
| HGY (`Pivot_Green_Yellow`) | (−1.0, +0.5, 0.0) | X | 180° | 180° (Stage 1) |

---

## 5. Failure Behavior

| Condition | Outcome |
|-----------|---------|
| Any check fails | Result is INVALID |
| Penalty | None applied for a failed attempt (timer continues running) |
| Ball position | Remains at `currentSeat.position` inside the current cube |
| `sequenceIndex` | Does not advance |
| Retry | Player may attempt the same move again immediately |
| Timer expiry | Timer expiry is a separate system; this document does not define its penalty |

No additional consequence beyond the continued timer pressure is imposed by the validation system itself.

---

## 6. Output States

| State | Meaning | Next action |
|-------|---------|-------------|
| `INVALID` | One or more of the 10 checks failed on release | Ball stays in current cube; no index advance |
| `VALID_READY` | All 10 checks passed on release | Transfer pipeline executes |
| `TRANSFER_COMPLETE` | Ball has been atomically snapped to destination seat | `sequenceIndex` increments; game state updates |
| `ROUND_COMPLETE` | `sequenceIndex` has reached `3`; ball is on `sequence[3]` | Round ends; timer stops |

State transitions:

```
Release
  ├─ [checks fail]  → INVALID
  └─ [checks pass]  → VALID_READY
                         └─ [snap executes] → TRANSFER_COMPLETE
                                               ├─ [sequenceIndex < 3] → (await next move)
                                               └─ [sequenceIndex == 3] → ROUND_COMPLETE
```

---

## 7. Constraints

The following are absolute constraints for Level 1 and must never be violated by the validation system or any system it calls:

| Constraint | Detail |
|------------|--------|
| No physics | `Rigidbody` is not present on the ball or any cube |
| No collision-based validation | No `OnCollisionEnter`, `OnTriggerEnter`, or physics overlap tests |
| No automatic movement | The system never initiates a rotation or moves any cube |
| No transfer during hold | `_isTransferring` remains `false` and no snap occurs while touch is active |
| No interpolation | Ball position changes are atomic (single-frame snap); no lerp or tween in Level 1 |
| No fourth hinge | Only HBR, HRG, HGY exist; there is no `Pivot_Yellow_Blue` or equivalent |
| No absolute Blender offsets | Android uses normalized frame state (1–240); absolute Blender timeline positions (241–480, etc.) are not used |

---

## Scope Constraints

- This document covers **planning only**.
- No C# code, no UI design, no Unity scene work is included here.
- Implementation details (MonoBehaviour hooks, event bindings, animator states) are deferred to later build documents.
