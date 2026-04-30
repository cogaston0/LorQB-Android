# Blender → Android Move Map (Level 1)

**Status:** All 8 moves confirmed working in Blender  
**Geometry source of truth:** `Assets/_LorQB/Editor/Level1SceneBuilder.cs`  
**Rotation sequence source of truth:** `UNITY_BUILD/02_Movement_Logic.md`  
**Frame timing source of truth:** Blender C/T animation scripts (external to repo)  
**Constraints:** No C# code. No Unity scene generation. No physics. No Rigidbody. Ball transfer is seat-to-seat only.

---

## World Positions (canonical — from Level1SceneBuilder.cs)

| Object          |     X |     Y |     Z | Notes                           |
|-----------------|-------|-------|-------|---------------------------------|
| Cube_Blue       | +1.0  |  0.0  | −1.0  | Front-Right corner, anchor      |
| Cube_Red        | +1.0  |  0.0  | +1.0  | Back-Right corner               |
| Cube_Green      | −1.0  |  0.0  | +1.0  | Back-Left corner                |
| Cube_Yellow     | −1.0  |  0.0  | −1.0  | Front-Left corner               |
| Seat_Blue       | +1.0  | +0.5  | −1.0  | Top-centre of Cube_Blue         |
| Seat_Red        | +1.0  | +0.5  | +1.0  | Top-centre of Cube_Red          |
| Seat_Green      | −1.0  | +0.5  | +1.0  | Top-centre of Cube_Green        |
| Seat_Yellow     | −1.0  | +0.5  | −1.0  | Top-centre of Cube_Yellow       |
| Pivot_Blue_Red  | +1.0  | +0.5  |  0.0  | Right top edge — Z-axis hinge   |
| Pivot_Red_Green |  0.0  | +0.5  | +1.0  | Back top edge — X-axis hinge    |
| Pivot_Green_Yellow | −1.0 | +0.5 | 0.0  | Left top edge — Z-axis hinge    |
| Pivot_Yellow_Blue* |  0.0 | +0.5 | −1.0 | Front top edge — X-axis hinge   |

> \* `Pivot_Yellow_Blue` is confirmed in the Blender ring model. It is not yet present in `Level1SceneBuilder.cs` and must be added before C15 and all T-series moves that use it can be implemented in Unity. See Android Implementation Note for C15.

---

## Rotation States (all cubes share the same discrete angle set)

| State | Euler angle | Hole direction |
|-------|------------|----------------|
| 0     | 0°         | ±X (aligned default)  |
| 1     | 90°        | ±Z (C12/C14) or ±Y (C13/C15) |
| 2     | 180°       | ±X (anti-aligned, also valid for pass-through) |
| 3     | 270°       | ±Z or ±Y (opposite of State 1) |

Transfer condition: holes are within ±5° in world space (State 0 or State 2).

---

## Blender Frame Convention

| Parameter | Value |
|-----------|-------|
| Frame rate | 24 FPS |
| Duration per 90° step | 30 frames (1.25 s) |
| C-series total frame range | 30 frames |
| T-series total frame range | 60 frames (2 sequential steps) |
| Transfer fires on frame | Last frame of each rotation step |

---

## C-Series Moves — Single-Hinge Rotation

Each C-series move activates one hinge, rotates one cube, and transfers the ball one seat.

---

### C12 — Blue → Red ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | C12 |
| **Source color** | Blue |
| **Destination color** | Red |
| **Active hinge** | `Pivot_Blue_Red` — position (+1.0, +0.5, 0.0), Z-axis (`Vector3.forward`) |
| **Moving cube / group** | `Cube_Red` via `RotationGroup_Blue_Red` (Cube_Blue is the anchor and never rotates) |
| **Angle sequence** | Cube_Red: 90° → 0° (−90° rotation about Z-axis; holes reach ±X alignment at 0°) |
| **Frame range** | Frames 1–30 |
| **Transfer frame** | Frame 30 (alignment confirmed at 0°) |
| **Seat source** | `Seat_Blue` (+1.0, +0.5, −1.0) |
| **Seat destination** | `Seat_Red` (+1.0, +0.5, +1.0) |
| **Return order** | Reverse: align via C12 with ball at Seat_Red → ball snaps to Seat_Blue |
| **Android implementation note** | `CubeRotationController` on Cube_Red references `Pivot_Blue_Red`. `HoleAlignmentDetector` on `Hinge_Blue_Red`. `BallTransferController` snaps ball from `Seat_Blue.position` to `Seat_Red.position` atomically when alignment fires. No interpolation in Level 1. |

---

### C13 — Red → Green ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | C13 |
| **Source color** | Red |
| **Destination color** | Green |
| **Active hinge** | `Pivot_Red_Green` — position (0.0, +0.5, +1.0), X-axis (`Vector3.right`) |
| **Moving cube / group** | `Cube_Green` via `RotationGroup_Red_Green` |
| **Angle sequence** | Cube_Green: 90° → 0° (−90° rotation about X-axis; holes reach ±X alignment at 0°) |
| **Frame range** | Frames 1–30 |
| **Transfer frame** | Frame 30 (alignment confirmed at 0°) |
| **Seat source** | `Seat_Red` (+1.0, +0.5, +1.0) |
| **Seat destination** | `Seat_Green` (−1.0, +0.5, +1.0) |
| **Return order** | Reverse: align via C13 with ball at Seat_Green → ball snaps to Seat_Red |
| **Android implementation note** | `CubeRotationController` on Cube_Green references `Pivot_Red_Green`. `HoleAlignmentDetector` on `Hinge_Red_Green`. Rotation axis is X (not Z); ensure `rotationAxis` field is set to `Vector3.right` in the Inspector or via `Level1SceneBuilder`. |

---

### C14 — Green → Yellow ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | C14 |
| **Source color** | Green |
| **Destination color** | Yellow |
| **Active hinge** | `Pivot_Green_Yellow` — position (−1.0, +0.5, 0.0), Z-axis (`Vector3.forward`) |
| **Moving cube / group** | `Cube_Yellow` via `RotationGroup_Green_Yellow` |
| **Angle sequence** | Cube_Yellow: 90° → 0° (−90° rotation about Z-axis via Pivot_Green_Yellow; holes reach ±X alignment at 0°) |
| **Frame range** | Frames 1–30 |
| **Transfer frame** | Frame 30 (alignment confirmed at 0°) |
| **Seat source** | `Seat_Green` (−1.0, +0.5, +1.0) |
| **Seat destination** | `Seat_Yellow` (−1.0, +0.5, −1.0) |
| **Return order** | Reverse: align via C14 with ball at Seat_Yellow → ball snaps to Seat_Green |
| **Android implementation note** | `CubeRotationController` on Cube_Yellow references `Pivot_Green_Yellow`. Cube_Yellow has a second pivot (Pivot_Yellow_Blue) for C15; the controller must track which pivot is active. Both pivot references must be wired in the Inspector. |

---

### C15 — Yellow → Blue ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | C15 |
| **Source color** | Yellow |
| **Destination color** | Blue |
| **Active hinge** | `Pivot_Yellow_Blue` — position (0.0, +0.5, −1.0), X-axis (`Vector3.right`) |
| **Moving cube / group** | `Cube_Yellow` via `RotationGroup_Yellow_Blue` (Cube_Blue is the anchor and never rotates) |
| **Angle sequence** | Cube_Yellow: 90° → 0° (−90° rotation about X-axis via Pivot_Yellow_Blue; holes reach ±X alignment at 0°) |
| **Frame range** | Frames 1–30 |
| **Transfer frame** | Frame 30 (alignment confirmed at 0°) |
| **Seat source** | `Seat_Yellow` (−1.0, +0.5, −1.0) |
| **Seat destination** | `Seat_Blue` (+1.0, +0.5, −1.0) |
| **Return order** | Reverse: align via C15 with ball at Seat_Blue → ball snaps to Seat_Yellow |
| **Android implementation note** | **`Pivot_Yellow_Blue` is not yet in `Level1SceneBuilder.cs` — it must be added.** Derived world position: (0.0, +0.5, −1.0), rotation axis `Vector3.right`. Add `Pivot_Yellow_Blue`, `Hinge_Yellow_Blue`, and `RotationGroup_Yellow_Blue` to the builder before implementing C15 or any T-series move that uses this hinge. Cube_Yellow's `CubeRotationController` shares world-space rotation angle across both its pivots (C14 and C15). |

---

## T-Series Moves — Two-Hinge (Non-Adjacent) Transfer

Each T-series move moves the ball between diagonally opposite cubes. It requires two sequential hinge operations with two intermediate transfers. All steps are confirmed working in Blender.

Ball transfer at each intermediate seat is still atomic (seat-to-seat, no interpolation in Level 1).

---

### T01 — Blue → Green ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | T01 |
| **Source color** | Blue |
| **Destination color** | Green |
| **Active hinges** | Step 1: `Pivot_Blue_Red` (Z-axis) → Step 2: `Pivot_Red_Green` (X-axis) |
| **Moving cube / group** | Step 1: `Cube_Red` via `RotationGroup_Blue_Red` → Step 2: `Cube_Green` via `RotationGroup_Red_Green` |
| **Angle sequence** | Step 1: Cube_Red 90° → 0°; Step 2: Cube_Green 90° → 0° |
| **Frame range** | Frames 1–60 (two 30-frame steps in sequence) |
| **Transfer frames** | Frame 30 (Blue → Red, via C12 alignment); Frame 60 (Red → Green, via C13 alignment) |
| **Seat source** | `Seat_Blue` (+1.0, +0.5, −1.0) |
| **Seat destination** | `Seat_Green` (−1.0, +0.5, +1.0) |
| **Return order** | Reverse step 2 first: C13 reverse (Green → Red), then reverse step 1: C12 reverse (Red → Blue) |
| **Android implementation note** | Two sequential calls: first C12 rotation + transfer, then C13 rotation + transfer. `BallTransferController` must update `currentCube` to Cube_Red after frame 30 before step 2 can fire. `shuffleIndex` advances twice. No simultaneous hinge activation — each step must complete before the next begins. |

---

### T02 — Yellow → Red ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | T02 |
| **Source color** | Yellow |
| **Destination color** | Red |
| **Active hinges** | Step 1: `Pivot_Yellow_Blue` (X-axis) → Step 2: `Pivot_Blue_Red` (Z-axis) |
| **Moving cube / group** | Step 1: `Cube_Yellow` via `RotationGroup_Yellow_Blue` → Step 2: `Cube_Red` via `RotationGroup_Blue_Red` |
| **Angle sequence** | Step 1: Cube_Yellow 90° → 0° (via Pivot_Yellow_Blue, X-axis); Step 2: Cube_Red 90° → 0° (via Pivot_Blue_Red, Z-axis) |
| **Frame range** | Frames 1–60 |
| **Transfer frames** | Frame 30 (Yellow → Blue, via C15 alignment); Frame 60 (Blue → Red, via C12 alignment) |
| **Seat source** | `Seat_Yellow` (−1.0, +0.5, −1.0) |
| **Seat destination** | `Seat_Red` (+1.0, +0.5, +1.0) |
| **Return order** | Reverse step 2 first: C12 reverse (Red → Blue), then reverse step 1: C15 reverse (Blue → Yellow) |
| **Android implementation note** | Requires `Pivot_Yellow_Blue` to be present in the Unity scene (see C15 note). Step 1 uses Cube_Yellow rotating around its C15 pivot; step 2 uses Cube_Red rotating around its C12 pivot. Intermediate seat is `Seat_Blue` — ball touches Blue momentarily between the two hops. |

---

### T03 — Red → Yellow ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | T03 |
| **Source color** | Red |
| **Destination color** | Yellow |
| **Active hinges** | Step 1: `Pivot_Red_Green` (X-axis) → Step 2: `Pivot_Green_Yellow` (Z-axis) |
| **Moving cube / group** | Step 1: `Cube_Green` via `RotationGroup_Red_Green` → Step 2: `Cube_Yellow` via `RotationGroup_Green_Yellow` |
| **Angle sequence** | Step 1: Cube_Green 90° → 0° (X-axis); Step 2: Cube_Yellow 90° → 0° (Z-axis via Pivot_Green_Yellow) |
| **Frame range** | Frames 1–60 |
| **Transfer frames** | Frame 30 (Red → Green, via C13 alignment); Frame 60 (Green → Yellow, via C14 alignment) |
| **Seat source** | `Seat_Red` (+1.0, +0.5, +1.0) |
| **Seat destination** | `Seat_Yellow` (−1.0, +0.5, −1.0) |
| **Return order** | Reverse step 2 first: C14 reverse (Yellow → Green), then reverse step 1: C13 reverse (Green → Red) |
| **Android implementation note** | Both steps use cubes already present in `Level1SceneBuilder.cs` (no missing pivots). Intermediate seat is `Seat_Green`. Step 2 uses Cube_Yellow rotating around its C14 pivot (Pivot_Green_Yellow). |

---

### T04 — Green → Blue ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | T04 |
| **Source color** | Green |
| **Destination color** | Blue |
| **Active hinges** | Step 1: `Pivot_Green_Yellow` (Z-axis) → Step 2: `Pivot_Yellow_Blue` (X-axis) |
| **Moving cube / group** | Step 1: `Cube_Yellow` via `RotationGroup_Green_Yellow` → Step 2: `Cube_Yellow` via `RotationGroup_Yellow_Blue` |
| **Angle sequence** | Step 1: Cube_Yellow 90° → 0° (Z-axis via Pivot_Green_Yellow); Step 2: Cube_Yellow 90° → 0° (X-axis via Pivot_Yellow_Blue) |
| **Frame range** | Frames 1–60 |
| **Transfer frames** | Frame 30 (Green → Yellow, via C14 alignment); Frame 60 (Yellow → Blue, via C15 alignment) |
| **Seat source** | `Seat_Green` (−1.0, +0.5, +1.0) |
| **Seat destination** | `Seat_Blue` (+1.0, +0.5, −1.0) |
| **Return order** | Reverse step 2 first: C15 reverse (Blue → Yellow), then reverse step 1: C14 reverse (Yellow → Green) |
| **Android implementation note** | **Special case:** Cube_Yellow is the moving cube for both steps. After step 1 it holds the ball at `Seat_Yellow`, then its C15 pivot (Pivot_Yellow_Blue) becomes active for step 2. The `CubeRotationController` on Cube_Yellow must switch pivot context between the two steps. `Pivot_Yellow_Blue` must be added to `Level1SceneBuilder.cs` before this move can be implemented. |

---

## Summary Table

| Code | Series | Source | Destination | Active Hinge(s) | Moving Cube(s) | Transfer Frame(s) | Confirmed |
|------|--------|--------|-------------|-----------------|----------------|-------------------|-----------|
| C12  | C      | Blue   | Red         | Pivot_Blue_Red        | Cube_Red             | 30        | ✅ |
| C13  | C      | Red    | Green       | Pivot_Red_Green       | Cube_Green           | 30        | ✅ |
| C14  | C      | Green  | Yellow      | Pivot_Green_Yellow    | Cube_Yellow (C14 pivot) | 30     | ✅ |
| C15  | C      | Yellow | Blue        | Pivot_Yellow_Blue*    | Cube_Yellow (C15 pivot) | 30     | ✅ |
| T01  | T      | Blue   | Green       | Pivot_Blue_Red → Pivot_Red_Green | Cube_Red → Cube_Green | 30, 60 | ✅ |
| T02  | T      | Yellow | Red         | Pivot_Yellow_Blue* → Pivot_Blue_Red | Cube_Yellow → Cube_Red | 30, 60 | ✅ |
| T03  | T      | Red    | Yellow      | Pivot_Red_Green → Pivot_Green_Yellow | Cube_Green → Cube_Yellow | 30, 60 | ✅ |
| T04  | T      | Green  | Blue        | Pivot_Green_Yellow → Pivot_Yellow_Blue* | Cube_Yellow → Cube_Yellow | 30, 60 | ✅ |

> \* `Pivot_Yellow_Blue` must be added to `Level1SceneBuilder.cs` before C15, T02, and T04 can be implemented in Unity.

---

## Ring Structure Reference

```
Blue (FR) ──[C12, Z-axis]──► Red (BR)
  ▲                               │
[C15, X-axis]               [C13, X-axis]
  │                               ▼
Yellow (FL) ◄──[C14, Z-axis]── Green (BL)

FR = Front-Right  BR = Back-Right
BL = Back-Left    FL = Front-Left
```

All four hinges form a closed ring. C-series moves travel one edge clockwise. T-series moves travel two edges clockwise (diagonal).

---

## Pre-Implementation Checklist

- [ ] Confirm `Pivot_Yellow_Blue` is added to `Level1SceneBuilder.cs` before implementing C15, T02, or T04
- [ ] Confirm `Cube_Yellow`'s `CubeRotationController` supports two pivot contexts (C14 and C15)
- [ ] Confirm `BallTransferController` updates `currentCube` between T-series steps before firing the next alignment check
- [ ] Confirm all four `HoleAlignmentDetector` instances are wired to the correct `HoleAxis` transforms
- [ ] Confirm all eight seat transforms are wired into `BallTransferController`'s transfer table
