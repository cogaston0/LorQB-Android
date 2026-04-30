# 03_Rotation_System.md — Rotation System Logic Definition

---

## 1. Pivot Behavior

- Each `Pivot_*` object is responsible for rotating **only** its corresponding `RotationGroup_*`.
- No cube may detach from the chain during or after a rotation operation.
- All cubes within a `RotationGroup_*` must behave as a **rigid, connected chain** — their relative positions and orientations to each other do not change during rotation; only the group as a whole rotates around its pivot axis.
- `Pivot_C12`, `Pivot_C13`, and `Pivot_C14` are independent control points. Each drives exactly one group. There is no cross-group influence.

---

## 2. Rotation Rules

- **Only one hinge (pivot) may be active at any given moment.** Concurrent rotations are not permitted.
- Every hinge follows the fixed movement cycle: `0° → 90° → 180° → 90° → 0°`. This constitutes one full movement cycle.
- Only two rotation ranges are valid within that cycle: `0° → 90°` and `0° → 180°`. No other target angle is permitted.
- **270° rotation is not allowed under any circumstance.**
- Rotation validation must occur **before** any movement begins. If the requested target angle is not `90°` or `180°`, the rotation is rejected and no transform change takes place.
- All rotations are **snap rotations** — the group jumps instantly to the target value. No interpolation, easing, or physics-based movement is used.
- Intermediate angles (e.g., 45°, 135°) are never valid states.
- No physics, no Rigidbody, and no collision-based blocking are used to enforce limits. **Scripted limits only.**

---

## 3. Axis Constraints

| Hinge    | Cube | Rotation Axis |
|----------|------|---------------|
| Pivot_C12 | C12  | Z-axis        |
| Pivot_C13 | C13  | X-axis        |
| Pivot_C14 | C14  | Z-axis        |

- Rotation is strictly confined to the defined axis for each pivot. No off-axis movement is allowed.
- Axis directions are defined in the canonical (base) orientation of the system.

---

## 4. Lock Rules

- When a pivot is not the active hinge, it is **fully locked** — its transform (position, rotation, scale) must not change under any circumstance.
- A pivot becomes locked immediately after its rotation step completes.
- A pivot remains locked until it is explicitly selected as the active hinge for the next step.
- Locked state is enforced logically; no external force or cascading rotation from an adjacent active pivot may alter a locked pivot's transform.

---

## 5. Return-to-Base Requirement

- After the complete intended rotation sequence has been executed, the entire system must return to its **canonical orientation** — the state in which all groups are at `0°` on their respective axes.
- The canonical orientation is the reference frame for all axis constraint definitions (see Section 3).
- The system is considered to have completed a full cycle only when every `RotationGroup_*` is at `0°` on its axis simultaneously.
- No partial return is considered valid; all groups must reach `0°` together to satisfy the return-to-base condition.

---

## 6. Frame Timing

### Rotation Cycle

Each hinge executes exactly one rotation cycle per color-to-color move:

```
0° → 90° → 180° → 90° → 0°
```

This full cycle costs **240 frames** in total.

### Frame Breakdown (per cycle)

| Leg                  | Angle Change      | Frames |
|----------------------|-------------------|--------|
| Leg 1: outward start | 0° → 90°          | 60     |
| Leg 2: outward peak  | 90° → 180°        | 60     |
| Leg 3: return start  | 180° → 90°        | 60     |
| Leg 4: return finish | 90° → 0°          | 60     |
| **Total**            |                   | **240**|

### Color-to-Color Move Timing

Each color-to-color move = one rotation cycle = 240 frames.

| Move                  | Duration   |
|-----------------------|------------|
| C12 Blue → Red        | 240 frames |
| C13 Red → Green       | 240 frames |
| C14 Green → Yellow    | 240 frames |
| C15 Yellow → Blue     | 240 frames |

- The 240-frame value is the authoritative unit for sequencing and logic. Unity will convert this to a wall-clock duration separately; the frame count must be preserved in all planning and scripted logic.
- One color-to-color move = one complete `0° → 90° → 180° → 90° → 0°` cycle = 240 frames.

---

## Summary of Constraints

| Rule                        | Constraint                                              |
|-----------------------------|---------------------------------------------------------|
| Active hinges at once       | Maximum 1                                               |
| Valid rotation angles       | 90° or 180° only (from 0°); 270° is not allowed         |
| Rotation cycle per move     | 0° → 90° → 180° → 90° → 0°                             |
| Rotation validation         | Occurs before movement; invalid angles are rejected     |
| Rotation type               | Snap (instant), no smooth or physics motion             |
| Limit enforcement           | Scripted limits only — no physics, Rigidbody, or collision |
| Frame timing per cycle      | 240 frames total (4 legs × 60 frames each)              |
| Frame timing per move       | 240 frames per color-to-color move                      |
| Cube detachment             | Not allowed under any condition                         |
| Non-active pivot transforms | Immutable (fully locked)                                |
| Rigidbody / Physics         | Not used                                                |
| Parenting tricks            | Not used                                                |
| End state requirement       | All groups at 0° (canonical orientation)                |
