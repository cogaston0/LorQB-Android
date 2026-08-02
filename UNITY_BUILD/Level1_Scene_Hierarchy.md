# Level 1 — Scene Hierarchy & World Positions

## Scene Hierarchy

```
Level1
├── Cubes
│   ├── Cube_Blue
│   ├── Cube_Red
│   ├── Cube_Green
│   └── Cube_Yellow
├── Seats
│   ├── Seat_Blue
│   ├── Seat_Red
│   ├── Seat_Green
│   └── Seat_Yellow
├── Ball
├── Pivot_Blue_Red
│   ├── Hinge_Blue_Red
│   └── RotationGroup_Blue_Red
├── Pivot_Red_Green
│   ├── Hinge_Red_Green
│   └── RotationGroup_Red_Green
├── Pivot_Green_Yellow
│   ├── Hinge_Green_Yellow
│   └── RotationGroup_Green_Yellow
└── UI
    ├── Shuffle_Order_Bar
    ├── Timer
    └── Player_Selector
```

---

## World Positions (X, Y, Z)

Cubes are 1×1×1 units. Square footprint is 2×2, cubes centred at each corner.

| Object                      | X    | Y    | Z    | Notes                            |
|-----------------------------|------|------|------|----------------------------------|
| **Cube_Blue**               | +1.0 |  0.0 | −1.0 | Front-Right corner               |
| **Cube_Red**                | +1.0 |  0.0 | +1.0 | Back-Right corner                |
| **Cube_Green**              | −1.0 |  0.0 | +1.0 | Back-Left corner                 |
| **Cube_Yellow**             | −1.0 |  0.0 | −1.0 | Front-Left corner                |
| **Seat_Blue**               | +1.0 | +0.5 | −1.0 | Top-centre of Cube_Blue          |
| **Seat_Red**                | +1.0 | +0.5 | +1.0 | Top-centre of Cube_Red           |
| **Seat_Green**              | −1.0 | +0.5 | +1.0 | Top-centre of Cube_Green         |
| **Seat_Yellow**             | −1.0 | +0.5 | −1.0 | Top-centre of Cube_Yellow        |
| **Ball**                    | +1.0 | +0.5 | −1.0 | Starts at Seat_Blue              |
| **Pivot_Blue_Red**          | +1.0 | +0.5 |  0.0 | Right-side top edge, Z midpoint  |
| Hinge_Blue_Red              | +1.0 | +0.5 |  0.0 | Same as Pivot (local origin)     |
| RotationGroup_Blue_Red      | +1.0 | +0.5 |  0.0 | Same as Pivot (local origin)     |
| **Pivot_Red_Green**         |  0.0 | +0.5 | +1.0 | Back-side top edge, X midpoint   |
| Hinge_Red_Green             |  0.0 | +0.5 | +1.0 | Same as Pivot (local origin)     |
| RotationGroup_Red_Green     |  0.0 | +0.5 | +1.0 | Same as Pivot (local origin)     |
| **Pivot_Green_Yellow**      | −1.0 | +0.5 |  0.0 | Left-side top edge, Z midpoint   |
| Hinge_Green_Yellow          | −1.0 | +0.5 |  0.0 | Same as Pivot (local origin)     |
| RotationGroup_Green_Yellow  | −1.0 | +0.5 |  0.0 | Same as Pivot (local origin)     |

---

## Ball Transfer Rules (Seat-Based, No Physics)

- Ball has **no Rigidbody**.
- Each `Seat_<Color>` is a world-positioned snap-target Transform on the top face of its cube.
- Transfer logic sets `ball.position = targetSeat.position` — no gravity, no launch, no physics.
- Transfer is **atomic**: once triggered it cannot be interrupted.
- Transfer only occurs when the HoleAlignmentDetector confirms the adjacent pair is aligned (±5°).

---

## Hinge Rotation Axes

| Pivot                  | Axis   | Unity Axis Vector | Edge Description          |
|------------------------|--------|-------------------|---------------------------|
| Pivot_Blue_Red         | Z-axis | Vector3.forward   | Right side (X = +1.0)     |
| Pivot_Red_Green        | X-axis | Vector3.right     | Back side (Z = +1.0)      |
| Pivot_Green_Yellow     | Z-axis | Vector3.forward   | Left side (X = −1.0)      |

---

## Script Component Map

| Script                      | Attached To               | Notes                                   |
|-----------------------------|---------------------------|-----------------------------------------|
| `CubeIdentifier`            | Each Cube_*               | Identifies colour, holds SeatTransform  |
| `CubeRotationController`    | Cube_Red, Green, Yellow   | Blue is anchor — no rotation controller |
| `HoleAlignmentDetector`     | Each Hinge_* child        | Reads HoleAxis.forward from both sides  |
| `BallTransferController`    | GameManager               | Seat-based snap transfer                |
| `TouchInputManager`         | GameManager               | Gesture detection + raycasting          |
| `ShuffleDisplayController`  | Shuffle_Order_Bar (UI)    |                                         |
| `GameTimerController`       | Timer (UI)                |                                         |
| `PlayerSelectorController`  | Player_Selector (UI)      |                                         |
| `TextModeController`        | UI root or header bar     |                                         |
| `RotationButtonController`  | Rotation controls row     |                                         |
| `LevelCompleteController`   | LevelComplete panel       |                                         |
| `GameManager`               | GameManager GO            | Central coordinator / singleton         |

---

## How to Build the Scene

1. Open the `Level_1` Unity scene (or create a new empty scene).
2. In the Unity menu select: **LorQB → Build Level 1 Scene**
3. The full hierarchy is created with correct world positions.
4. Assign coloured materials to each Cube in the Inspector.
5. Assign the `cubeLayer` LayerMask on `TouchInputManager`.
6. Wire remaining Inspector references (BallTransferController seats, HoleAlignmentDetector HoleAxis transforms, etc.).
