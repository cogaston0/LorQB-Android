# Level 1 — Rotation System

---

## Overview

All rotation in Level 1 is **fully scripted** — no physics, no Rigidbody, no collision-based movement. Each hinge rotates its assigned `RotationGroup` through a fixed, bounded cycle of discrete angles. The system enforces hard angle limits, blocking rules, and a single-active-pivot constraint entirely through code validation.

---

## 1. Valid Rotation Cycle

Each hinge follows the same bounded angle cycle. There is **no 270° step** and **no wrap-around**:

```
0° → 90° → 180° → 90° → 0°
```

| Position | Role        |
|----------|-------------|
| 0°       | Base / rest — default closed position |
| 90°      | Intermediate — transitional open position |
| 180°     | Maximum — fully open position |

> **270° is explicitly forbidden.** No hinge may advance past 180°.

---

## 2. Timing

Each **color-to-color ball transfer** requires the cube to travel the full 0° → 90° → 180° arc and back, totaling **240 frames** of rotation.

### Frame breakdown per arc segment

| Segment         | Frames |
|-----------------|--------|
| 0°  → 90°       | 60     |
| 90° → 180°      | 60     |
| 180° → 90°      | 60     |
| 90°  → 0°       | 60     |
| **Total**       | **240** |

Each 60-frame step corresponds to one discrete 90° rotation. Animation speed is constant across all segments.

---

## 3. Angle Limits

| Angle | Status      | Notes |
|-------|-------------|-------|
| 0°    | Allowed — base/rest | Default closed state |
| 90°   | Allowed — intermediate | Transition point only |
| 180°  | Allowed — maximum | Hard ceiling; cannot go further |
| 270°  | **Forbidden** | No hinge may reach this angle |

Validation is enforced per-frame during rotation. If a rotation step would carry the angle past 180°, it is **clamped to 180°** and the hinge stops.

---

## 4. Blocking Rules

Blocking is enforced by **scripted validation only**. Unity collision, triggers, and Rigidbody are not used.

| Rule | Detail |
|------|--------|
| Base plane | No cube may rotate through or below the base plane (Y = 0). |
| Cube-to-cube | No cube may rotate into the occupied volume of another cube. |
| Detection method | Pre-rotation angle check via code — not runtime physics collision. |
| Consequence | If a move would violate a blocking rule, the rotation is rejected before it begins. |

---

## 5. Movement Rules

| Rule | Detail |
|------|--------|
| One active pivot | Only one hinge/pivot may be rotating at any time. |
| Assigned group | An active pivot rotates **only** its own `RotationGroup`; no other objects move. |
| Locked pivots | All non-active pivots remain completely locked for the duration. |
| No detachment | Cubes remain permanently attached to their hinges; no separation is possible. |
| Ball transfer | Ball transfer from one `Seat` to the next happens **only** after valid hole alignment is confirmed at the end of a rotation step. |

---

## 6. Required Move Table — Level 1

The canonical ball path for Level 1 is a closed four-color ring. The principal hinge for each move is listed first; passive hinges that must follow are listed in parentheses.

> **Passive-hinge rule:** For each move in the C or T series, the principal hinge drives the rotation. Any hinge that shares a cube with the principal hinge rides along passively (no independent input, no independent angle change) but its angle state is updated alongside the principal hinge's rotation group to maintain hierarchy integrity.

| Move | Principal hinge | Passive hinges | Ball path |
|------|----------------|----------------|-----------|
| C12  | `HBR` (Pivot_Blue_Red)      | — | Blue → Red |
| C13  | `HRG` (Pivot_Red_Green)     | `HBR` (passive, follows Red) | Red → Green |
| C14  | `HGY` (Pivot_Green_Yellow)  | `HRG` (passive, follows Green) | Green → Yellow |
| C15  | `HYB` (Pivot_Yellow_Blue)   | `HGY` (passive, follows Yellow) | Yellow → Blue |

### Example — C14 (Green → Yellow): HGY is principal, HBR is passive

When the ball moves from Green to Yellow, `HGY` is the **principal hinge** and drives the rotation. `HBR` (and any other hinge attached to the rotating group) must come along **passively** — they do not fire their own rotation logic, but their attached transforms move because they are children of the `RotationGroup_Green_Yellow`. `HGY` is always the axis-of-record for angle tracking on this move.

---

## 7. Rotation Sequence Per Hinge

All hinges follow the identical 0° → 90° → 180° → 90° → 0° cycle. Below is the per-hinge state mapping.

### C12 — Blue ↔ Red (`HBR` / `Pivot_Blue_Red`)

```
State 0: Cube_Red at   0° — rest, holes aligned with Cube_Blue
State 1: Cube_Red at  90° — intermediate
State 2: Cube_Red at 180° — maximum, holes anti-aligned
State 3: Cube_Red at  90° — returning
State 4: Cube_Red at   0° — rest (transfer-ready)
```

### C13 — Red ↔ Green (`HRG` / `Pivot_Red_Green`)

```
State 0: Cube_Green at   0° — rest, holes aligned with Cube_Red
State 1: Cube_Green at  90° — intermediate
State 2: Cube_Green at 180° — maximum
State 3: Cube_Green at  90° — returning
State 4: Cube_Green at   0° — rest (transfer-ready)
```

### C14 — Green ↔ Yellow (`HGY` / `Pivot_Green_Yellow`)

```
State 0: Cube_Yellow at   0° — rest, holes aligned with Cube_Green
State 1: Cube_Yellow at  90° — intermediate
State 2: Cube_Yellow at 180° — maximum
State 3: Cube_Yellow at  90° — returning
State 4: Cube_Yellow at   0° — rest (transfer-ready)
```

### C15 — Yellow ↔ Blue (`HYB` / `Pivot_Yellow_Blue`)

```
State 0: Cube_Yellow at   0° — rest, holes aligned with Cube_Blue
State 1: Cube_Yellow at  90° — intermediate
State 2: Cube_Yellow at 180° — maximum
State 3: Cube_Yellow at  90° — returning
State 4: Cube_Yellow at   0° — rest (transfer-ready)
```

---

## 8. Constraints & Non-Goals

| Constraint | Status |
|------------|--------|
| Unity C# code | Not defined here — planning only |
| Physics / Rigidbody | Not used — ever |
| Collision-based blocking | Not used — scripted validation only |
| 270° rotation | Forbidden |
| Concurrent pivots | Forbidden — exactly one active at a time |
| Cube detachment | Forbidden |

---

*This document defines the final rotation rules for LorQB Level 1. It supersedes any earlier angle-cycle descriptions that allowed 270° or full wrapping.*
