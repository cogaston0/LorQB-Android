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
- Rotation advances in discrete steps only: `0° → 90° → 180° → 270° → (wrap back to 0°)`.
- All rotations are **snap rotations** — the group jumps instantly to the next step value. No interpolation, easing, or physics-based movement is used.
- A rotation step is triggered by a discrete input event (e.g., tap/button press). One event = one 90° step.
- Intermediate angles (e.g., 45°, 135°) are never valid states.

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

## Summary of Constraints

| Rule                        | Constraint                                      |
|-----------------------------|-------------------------------------------------|
| Active hinges at once       | Maximum 1                                       |
| Valid rotation angles       | 0°, 90°, 180°, 270° only                        |
| Rotation type               | Snap (instant), no smooth or physics motion     |
| Cube detachment             | Not allowed under any condition                 |
| Non-active pivot transforms | Immutable (fully locked)                        |
| Rigidbody / Physics         | Not used                                        |
| Parenting tricks            | Not used                                        |
| End state requirement       | All groups at 0° (canonical orientation)        |
