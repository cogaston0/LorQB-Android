# 05 — Interactive Gameplay Model

## Overview

LorQB Android is an **interactive puzzle game**.  
The player controls all meaningful actions. The system acts only as a validator and responder — it never executes moves automatically.

---

## Core Rule

> **The user controls ball placement and cube rotation.  
> The system does not automatically execute moves.**

---

## Gameplay Flow

### 1. Ball Color Selection
- At the start of each round the player selects a ball color from the available set.
- The selected color determines which sequence slot the ball belongs to.

### 2. Shuffle / Order Bar — 4-Color Sequence
- The Shuffle/Order Bar displays the required 4-color sequence for the current puzzle.
- The sequence defines which cube must receive which color and in what order.
- The bar is read-only during play; the player cannot reorder it mid-round.

### 3. Player Places Ball into First Required Cube
- The player taps/drags the ball to the cube that matches the first slot in the sequence.
- Placement is a deliberate player action — the ball does not move on its own.

### 4. Timer Starts After Ball Is Placed
- The countdown timer begins **only after** the player places the ball into the first cube.
- No timer activity occurs during color selection or before first placement.

### 5. Player Rotates Cubes Manually via Touch
- The player uses touch gestures (swipe/drag on a cube face) to rotate cubes around their hinge axis.
- Rotation is continuous and player-driven; there is no auto-snap to a target angle.
- The player decides when to stop rotating.

---

## System Validation (Read-Only Checks)

The system validates the following conditions **on every frame** or **on rotation-end event**, but takes no automatic action:

| # | Condition Checked | Description |
|---|-------------------|-------------|
| 1 | **Active Cube** | Confirms which cube currently holds the ball. |
| 2 | **Hinge Rotation** | Confirms the cube is rotating around its designated hinge axis (Z, X, or Z per Level 1 pivot groups). |
| 3 | **Angle** | Measures current rotation angle against the required transfer angle. |
| 4 | **Hole Alignment** | Checks whether the exit hole of the active cube aligns with the entry hole of the next cube. |
| 5 | **Ball Transfer Eligibility** | All of the above must be true simultaneously before a transfer is considered eligible. |

The system **does not move the ball** when eligibility is detected. It only sets a readiness flag.

---

## Ball Transfer

- Ball transfer occurs **only when the player's rotation action creates a valid alignment** (all five conditions above pass).
- Transfer is a **scripted seat-to-seat handoff**: the ball is repositioned from the exit seat of the active cube to the entry seat of the next cube in code — no physics forces, no velocity, no trajectory.
- If the player rotates past the valid angle without triggering the transfer, the flag resets and the player must re-align.

---

## No Physics / No Rigidbody

- There are **no Rigidbody components** on the ball or cubes.
- There is **no physics simulation** (gravity, collision response, forces).
- All ball movement is purely scripted Transform manipulation.
- Cube rotation is applied directly to the Transform (e.g., `Transform.Rotate` or setting `Transform.localEulerAngles`).

---

## Move Map Usage

- The Move Map (defined in `04_Blender_To_Android_Move_Map.md`) is used **exclusively as a set of validation rules**.
- It specifies: which cube pairs can exchange a ball, which hinge axis applies, and what angle constitutes valid alignment.
- The Move Map is **not an execution script** — the system reads it to decide whether a player-initiated state is valid, not to drive autonomous movement.

---

## Summary Table

| Mechanic | Controlled By | System Role |
|----------|--------------|-------------|
| Ball color selection | Player | Display available colors |
| Sequence definition | Shuffle/Order Bar | Display required order |
| Ball placement into cube | Player | Accept or reject placement |
| Timer | Triggered by first placement | Count down |
| Cube rotation | Player (touch) | Validate angle & alignment |
| Ball transfer | Player action → valid alignment | Scripted seat-to-seat move |
| Physics | None | None |
| Move Map | Validation source | Rules lookup only |
