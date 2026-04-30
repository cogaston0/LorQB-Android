# Blender → Android Move Map (Level 1)

**Status:** All 8 moves confirmed working in Blender  
**Source of truth:** Blender C/T animation scripts  
**Geometry reference:** `Assets/_LorQB/Editor/Level1SceneBuilder.cs`  
**Constraints:** No C# code. No Unity scene generation. No physics. No Rigidbody. Ball transfer is seat-to-seat only.

---

## Valid Hinges

There are exactly **three** hinges in the Level 1 model. There is no fourth hinge and no `Pivot_Yellow_Blue` / `Hinge_Yellow_Blue`.

| Shortcode | Full name          | Unity pivot name        | Position (X, Y, Z)  | Blender rotation axis |
|-----------|--------------------|-------------------------|---------------------|-----------------------|
| **HBR**   | Hinge_Blue_Red     | `Pivot_Blue_Red`        | (+1.0, +0.5,  0.0)  | X                     |
| **HRG**   | Hinge_Red_Green    | `Pivot_Red_Green`       | ( 0.0, +0.5, +1.0)  | Y                     |
| **HGY**   | Hinge_Green_Yellow | `Pivot_Green_Yellow`    | (−1.0, +0.5,  0.0)  | X                     |

---

## World Positions (canonical — from Level1SceneBuilder.cs)

| Object      |     X |     Y |     Z | Notes                        |
|-------------|-------|-------|-------|------------------------------|
| Cube_Blue   | +1.0  |  0.0  | −1.0  | Front-Right corner, anchor   |
| Cube_Red    | +1.0  |  0.0  | +1.0  | Back-Right corner            |
| Cube_Green  | −1.0  |  0.0  | +1.0  | Back-Left corner             |
| Cube_Yellow | −1.0  |  0.0  | −1.0  | Front-Left corner            |
| Seat_Blue   | +1.0  | +0.5  | −1.0  | Top-centre of Cube_Blue      |
| Seat_Red    | +1.0  | +0.5  | +1.0  | Top-centre of Cube_Red       |
| Seat_Green  | −1.0  | +0.5  | +1.0  | Top-centre of Cube_Green     |
| Seat_Yellow | −1.0  | +0.5  | −1.0  | Top-centre of Cube_Yellow    |

---

## Blender Frame Convention

| Parameter | Value |
|-----------|-------|
| C-series angle sequence | 0° → 90° → 180° → 90° → 0° (peak at midframe, return to start) |
| C-series frame window per move | 240 frames |
| C-series transfer | Fires between the two frames at the 180° peak |
| T-series forward move | Frames 1–160 (two stages, 80 frames each) |
| T-series transfer | Frame 161 |
| T-series return | Frames 162–240 |

---

## C-Series Moves — Single-Hinge, Full Arc Rotation

Each C-series move activates one hinge, sweeps the rotating group 0° → 90° → 180° (peak, ball transfers), then returns 180° → 90° → 0°. Ball transfer is seat-to-seat and fires at the peak frame.

---

### C12 — Blue → Red ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | C12 |
| **Source color** | Blue |
| **Destination color** | Red |
| **Active hinge** | HBR (`Hinge_Blue_Red` / `Pivot_Blue_Red`) |
| **Blender rotation axis** | X |
| **Moving cube / group** | `Cube_Red` (Cube_Blue is the anchor and never rotates) |
| **Angle sequence** | 0° → 90° → 180° → 90° → 0° |
| **Frame range** | Frames 1–240 |
| **Transfer frame** | Frame 120 → 121 (ball snaps at 180° peak) |
| **Seat source** | `Seat_Blue` (+1.0, +0.5, −1.0) |
| **Seat destination** | `Seat_Red` (+1.0, +0.5, +1.0) |
| **Return order** | After transfer: hinge returns 180° → 90° → 0° (frames 121–240) |
| **Android implementation note** | `HoleAlignmentDetector` on `Hinge_Blue_Red` fires when Cube_Red reaches 180° (anti-parallel alignment). `BallTransferController` snaps ball atomically from `Seat_Blue.position` to `Seat_Red.position`. No interpolation in Level 1. |

---

### C13 — Red → Green ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | C13 |
| **Source color** | Red |
| **Destination color** | Green |
| **Active hinge** | HRG (`Hinge_Red_Green` / `Pivot_Red_Green`) |
| **Blender rotation axis** | Y |
| **Moving cube / group** | `Cube_Green` |
| **Angle sequence** | 0° → 90° → 180° → 90° → 0° |
| **Frame range** | Frames 241–480 |
| **Transfer frame** | Frame 360 → 361 (ball snaps at 180° peak) |
| **Seat source** | `Seat_Red` (+1.0, +0.5, +1.0) |
| **Seat destination** | `Seat_Green` (−1.0, +0.5, +1.0) |
| **Return order** | After transfer: hinge returns 180° → 90° → 0° (frames 361–480) |
| **Android implementation note** | `HoleAlignmentDetector` on `Hinge_Red_Green`. Blender axis Y maps to Unity `Vector3.up`; confirm `rotationAxis` field on `CubeRotationController` for Cube_Green is set correctly when mapping from Blender. |

---

### C14 — Green → Yellow ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | C14 |
| **Source color** | Green |
| **Destination color** | Yellow |
| **Active hinge** | HGY (`Hinge_Green_Yellow` / `Pivot_Green_Yellow`) |
| **Blender rotation axis** | X |
| **Moving cube / group** | `Cube_Yellow` |
| **Angle sequence** | 0° → 90° → 180° → 90° → 0° |
| **Frame range** | Frames 481–720 |
| **Transfer frame** | Frame 600 → 601 (ball snaps at 180° peak) |
| **Seat source** | `Seat_Green` (−1.0, +0.5, +1.0) |
| **Seat destination** | `Seat_Yellow` (−1.0, +0.5, −1.0) |
| **Return order** | After transfer: hinge returns 180° → 90° → 0° (frames 601–720) |
| **Android implementation note** | `HoleAlignmentDetector` on `Hinge_Green_Yellow`. Same axis type as HBR (X-axis in Blender). |

---

### C15 — Yellow → Blue ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | C15 |
| **Source color** | Yellow |
| **Destination color** | Blue |
| **Active hinge** | HRG (`Hinge_Red_Green` / `Pivot_Red_Green`) |
| **Blender rotation axis** | Y |
| **Moving cube / group** | Cube_Green + Cube_Yellow swing together as one unit toward Cube_Blue + Cube_Red |
| **Angle sequence** | 0° → 90° → 180° → 90° → 0° |
| **Frame range** | Frames 720–960 |
| **Transfer frame** | Frame 840 → 841 (ball snaps at 180° peak) |
| **Seat source** | `Seat_Yellow` (−1.0, +0.5, −1.0) |
| **Seat destination** | `Seat_Blue` (+1.0, +0.5, −1.0) |
| **Return order** | After transfer: HRG returns 180° → 90° → 0° (frames 841–960) |
| **Android implementation note** | C15 reuses HRG — there is no fourth hinge. The entire Green+Yellow side rotates as a rigid unit around `Pivot_Red_Green`. `BallTransferController` snaps ball from `Seat_Yellow.position` to `Seat_Blue.position` when HRG reaches 180° alignment. |

---

## T-Series Moves — Multi-Stage Non-Adjacent Transfer

Each T-series move carries the ball between non-adjacent cubes using sequential hinge activations. Forward move runs frames 1–160 (two stages of 80 frames each); transfer fires at frame 161; return path runs frames 162–240. All steps confirmed working in Blender.

Ball transfer at frame 161 is atomic (seat-to-seat, no interpolation in Level 1).

---

### T01 — Blue → Green ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | T01 |
| **Source color** | Blue |
| **Destination color** | Green |
| **Stage 1 hinge** | HBR — 180°, frames 1–80 |
| **Stage 2 hinge** | HRG — 90°, frames 81–160 |
| **Transfer frame** | 161 |
| **Return path** | HRG returns: frames 162–200; HBR returns: frames 201–240 |
| **Moving cubes** | Stage 1: Cube_Red (around HBR); Stage 2: Cube_Green (around HRG) |
| **Seat source** | `Seat_Blue` (+1.0, +0.5, −1.0) |
| **Seat destination** | `Seat_Green` (−1.0, +0.5, +1.0) |
| **Android implementation note** | Stage 1 rotates Cube_Red to 180° around HBR, opening the Blue–Red gap fully. Stage 2 rotates Cube_Green to 90° around HRG to align with Blue. Transfer fires at frame 161. Intermediate state: ball remains at Seat_Blue through both stages until the atomic snap at frame 161. |

---

### T02 — Yellow → Red ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | T02 |
| **Source color** | Yellow |
| **Destination color** | Red |
| **Stage 1 hinge** | HGY — 180°, frames 1–80 |
| **Stage 2 hinge** | HRG — 90°, frames 81–160 |
| **Transfer frame** | 161 |
| **Return path** | HRG returns: frames 162–200; HGY returns: frames 201–240 |
| **Moving cubes** | Stage 1: Cube_Yellow (around HGY); Stage 2: Cube_Green+Yellow group (around HRG) |
| **Seat source** | `Seat_Yellow` (−1.0, +0.5, −1.0) |
| **Seat destination** | `Seat_Red` (+1.0, +0.5, +1.0) |
| **Android implementation note** | Stage 1 opens HGY fully (180°). Stage 2 swings the Green+Yellow group around HRG by 90° to bring Yellow alongside Red. Transfer at frame 161 snaps ball from Seat_Yellow to Seat_Red. All three hinges (HBR, HRG, HGY) are in the scene; no additional hinges required. |

---

### T03 — Red → Yellow ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | T03 |
| **Source color** | Red |
| **Destination color** | Yellow |
| **Stage 1 hinge** | HGY — 180°, frames 1–80 |
| **Stage 2a** | Cube_Red rotates 90°, frames 81–120 |
| **Stage 2b hinge** | HRG — 90°, frames 121–160 |
| **Transfer frame** | 161 |
| **Return path** | HRG + Cube_Red return: frames 162–200; HGY returns: frames 201–240 |
| **Moving cubes** | Stage 1: Cube_Yellow (around HGY); Stage 2a: Cube_Red (independent); Stage 2b: Cube_Green (around HRG) |
| **Seat source** | `Seat_Red` (+1.0, +0.5, +1.0) |
| **Seat destination** | `Seat_Yellow` (−1.0, +0.5, −1.0) — Yellow side position |
| **Android implementation note** | Three-part move. Stage 1 clears the Green–Yellow hinge. Stage 2a and 2b run back-to-back; Cube_Red rotates 90° while HRG simultaneously or sequentially rotates 90° to bring Red alongside Yellow. Transfer at frame 161 snaps ball from Seat_Red to Seat_Yellow. |

---

### T04 — Green → Blue ✅ Confirmed in Blender

| Property | Value |
|----------|-------|
| **Move code** | T04 |
| **Source color** | Green |
| **Destination color** | Blue |
| **Stage 1 hinge** | HRG — 180°, frames 1–80 |
| **Stage 2 hinge** | HBR — 180°, frames 81–160 |
| **Transfer frame** | 161 |
| **Return path** | HBR returns: frames 162–200; HRG returns: frames 201–240 |
| **Moving cubes** | Stage 1: Cube_Green+Yellow group (around HRG); Stage 2: Cube_Red (around HBR) |
| **Seat source** | `Seat_Green` (−1.0, +0.5, +1.0) |
| **Seat destination** | `Seat_Blue` (+1.0, +0.5, −1.0) |
| **Android implementation note** | Stage 1 swings HRG fully open (180°), carrying the Green+Yellow group. Stage 2 opens HBR fully (180°) to bring Green alongside Blue. Transfer at frame 161 snaps ball from Seat_Green to Seat_Blue. Uses only HRG and HBR — no HGY and no invented fourth hinge. |

---

## Summary Table

| Code | Series | Source | Destination | Stage 1 Hinge | Stage 2 Hinge | Transfer Frame | Confirmed |
|------|--------|--------|-------------|---------------|---------------|----------------|-----------|
| C12  | C | Blue   | Red    | HBR (180° peak) | —           | 120 → 121 | ✅ |
| C13  | C | Red    | Green  | HRG (180° peak) | —           | 360 → 361 | ✅ |
| C14  | C | Green  | Yellow | HGY (180° peak) | —           | 600 → 601 | ✅ |
| C15  | C | Yellow | Blue   | HRG (180° peak) | —           | 840 → 841 | ✅ |
| T01  | T | Blue   | Green  | HBR 180° (fr 1–80) | HRG 90° (fr 81–160) | 161 | ✅ |
| T02  | T | Yellow | Red    | HGY 180° (fr 1–80) | HRG 90° (fr 81–160) | 161 | ✅ |
| T03  | T | Red    | Yellow | HGY 180° (fr 1–80) | Red 90° + HRG 90° (fr 81–160) | 161 | ✅ |
| T04  | T | Green  | Blue   | HRG 180° (fr 1–80) | HBR 180° (fr 81–160) | 161 | ✅ |

---

## Hinge Ring Reference

```
Blue (FR) ──[HBR, X-axis]──► Red (BR)
  ▲                                │
  │                           [HRG, Y-axis]
  │ (C15 uses HRG swing)           │
  │                                ▼
Yellow (FL) ◄──[HGY, X-axis]── Green (BL)

FR = Front-Right  BR = Back-Right
BL = Back-Left    FL = Front-Left

Three hinges only: HBR · HRG · HGY
```

---

## Pre-Implementation Checklist

- [ ] Confirm `HoleAlignmentDetector` on each of the three hinges fires at 180° (anti-parallel peak) for C-series
- [ ] Confirm C15 wires to `Hinge_Red_Green` (`Pivot_Red_Green`) — not any fourth hinge
- [ ] Confirm T04 uses HRG then HBR — not HGY and not any invented hinge
- [ ] Confirm `BallTransferController` fires at the correct transfer frame for each move
- [ ] Confirm all four seat transforms are wired into `BallTransferController`'s transfer table
- [ ] Confirm no reference to `Pivot_Yellow_Blue`, `Hinge_Yellow_Blue`, or `RotationGroup_Yellow_Blue` exists anywhere in the project
