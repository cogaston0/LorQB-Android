# Level 1 — Scene Setup (Final)

---

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

Cubes are 1×1×1 units. Square footprint is ~1×1, cubes centred at each corner.

### Cubes

| Object       |     X |     Y |    Z | Notes              |
|--------------|-------|-------|------|--------------------|
| Cube_Blue    | +0.51 | +0.51 | 0.50 | Front-Right corner |
| Cube_Red     | +0.51 | −0.51 | 0.50 | Back-Right corner  |
| Cube_Green   | −0.51 | −0.51 | 0.50 | Back-Left corner   |
| Cube_Yellow  | −0.51 | +0.51 | 0.50 | Front-Left corner  |

### Seats (ball snap-targets, top face of each cube)

| Object      |     X |     Y |    Z | Notes                     |
|-------------|-------|-------|------|---------------------------|
| Seat_Blue   | +0.51 | +0.51 | 0.25 | Top-centre of Cube_Blue   |
| Seat_Red    | +0.51 | −0.51 | 0.25 | Top-centre of Cube_Red    |
| Seat_Green  | −0.51 | −0.51 | 0.25 | Top-centre of Cube_Green  |
| Seat_Yellow | −0.51 | +0.51 | 0.25 | Top-centre of Cube_Yellow |

### Ball

| Object |     X |     Y |    Z | Notes                      |
|--------|-------|-------|------|----------------------------|
| Ball   | +0.51 | +0.51 | 0.25 | Starts at Seat_Blue        |

### Pivots & Hinges (children share the same world origin as their Pivot)

| Object                              |     X |     Y |    Z | Notes                            |
|-------------------------------------|-------|-------|------|----------------------------------|
| Pivot_Blue_Red / Hinge_Blue_Red     | +0.51 |  0.00 | 1.00 | Right-side top edge              |
| RotationGroup_Blue_Red              | +0.51 |  0.00 | 1.00 | Same origin as Pivot             |
| Pivot_Red_Green / Hinge_Red_Green   |  0.00 | −0.51 | 1.00 | Back-side top edge               |
| RotationGroup_Red_Green             |  0.00 | −0.51 | 1.00 | Same origin as Pivot             |
| Pivot_Green_Yellow / Hinge_Green_Yellow | −0.51 | 0.00 | 1.00 | Left-side top edge           |
| RotationGroup_Green_Yellow          | −0.51 |  0.00 | 1.00 | Same origin as Pivot             |

---

## Hinge Rotation Axes

| Pivot                  | Axis   | Edge                    |
|------------------------|--------|-------------------------|
| Pivot_Blue_Red         | Z-axis | Right side (X = +0.51)  |
| Pivot_Red_Green        | X-axis | Back side  (Y = −0.51)  |
| Pivot_Green_Yellow     | Z-axis | Left side  (X = −0.51)  |

---

## Rules

- Cubes hollow, transparent colours
- Hinges on top edge only — no detachment
- No physics, no Rigidbody on Ball or Cubes
- Colliders on cubes for touch raycasting only (no Rigidbody)
- Ball position set by script snap to Seat transform — no gravity, no launch
- Cube_Blue is the anchor; it does not rotate
- Cube_Red, Cube_Green, Cube_Yellow each get a CubeRotationController

---

## Naming Conventions

| Type           | Convention              | Example                  |
|----------------|-------------------------|--------------------------|
| Cube GO        | `Cube_[Color]`          | `Cube_Blue`              |
| Seat GO        | `Seat_[Color]`          | `Seat_Blue`              |
| Pivot group    | `Pivot_[ColorA]_[ColorB]` | `Pivot_Blue_Red`       |
| Hinge child    | `Hinge_[ColorA]_[ColorB]` | `Hinge_Blue_Red`       |
| RotationGroup  | `RotationGroup_[ColorA]_[ColorB]` | `RotationGroup_Blue_Red` |
| HoleAxis marker| `HoleAxis`              | child of Hinge or RotationGroup |
