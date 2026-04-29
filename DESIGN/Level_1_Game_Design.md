# Level 1 — Game Design Document

## Overview

**Game:** LorQB / Cubolita
**Platform:** Android (Unity)
**Level:** 1 (of planned multi-level game)
**Genre:** Educational Puzzle
**Target audience:** Children ages 6–12 and learners of all ages
**Accessibility:** Text mode available for early readers

---

## Game Objective

Move the **ball** from the **starting cube** (Blue) through all four cubes — **Blue → [shuffle order]** — by rotating cubes so that holes align, allowing the ball to pass through. Complete the full shuffle sequence as fast as possible.

---

## Game Elements

### Cubes

| Cube | Color | Hex | Position in Chain |
|------|-------|-----|------------------|
| Blue | Blue | `#4488FF` | Position 1 (left anchor) |
| Red | Red | `#FF4444` | Position 2 |
| Green | Green | `#44BB44` | Position 3 |
| Yellow | Yellow | `#FFDD00` | Position 4 (right end) |

**Cube properties:**
- **Hollow**: the interior is empty so the ball can reside inside
- **Transparent**: the cube walls are semi-transparent (alpha ~0.4) so the ball is always visible
- **Holed**: each cube has two aligned circular holes on opposite faces (X axis — left and right faces)
- **Scale**: 1×1×1 Unity units
- **Hinged**: connected to adjacent cubes via a top-edge hinge (Z axis)

### The Ball

- Single sphere
- Diameter: 0.4 Unity units (fits through hole with clearance)
- Color: white/pearl (`#F0F0F0`)
- **Start position**: inside Cube_Blue, centered
- **Movement**: scripted linear translation through holes (no physics)
- The ball is **always inside exactly one cube** (never in transit between two cubes simultaneously from a game state perspective — transfer is atomic)

### Hinges

- **Three hinges** connect the four cubes:
  - Hinge 1: Blue ↔ Red (top edge)
  - Hinge 2: Red ↔ Green (top edge)
  - Hinge 3: Green ↔ Yellow (top edge)
- Hinge axis: **Z axis** (horizontal, along the top edge)
- Rotation direction: opens like a lid (positive Z = opens forward)
- **No detachment**: cubes always remain connected via hinges
- **No physics joints**: hinge behavior is scripted

---

## Gameplay Flow

### Pre-Game
```
1. Scene loads → Player Selector panel appears (game paused)
2. Player selects name or enters custom name
3. Player taps [START]
4. Player Selector hides
5. Shuffle order is generated randomly (Blue always first)
6. Shuffle display shown in HUD
7. Timer starts
```

### During Game
```
8. Ball is in Blue cube (initial position)
9. Player views shuffle order (e.g., Blue → Green → Red → Yellow)
10. Player taps a cube to select it
11. Player swipes or taps arrow buttons to rotate cube
12. Player aligns the selected cube's hole with the adjacent cube's hole
13. When holes align: ball automatically moves to next cube
14. Shuffle display advances one step
15. Repeat steps 10–14 until all 4 cubes visited in order
```

### Post-Game
```
16. Ball reaches final cube → Level Complete triggers
17. Timer stops
18. Level Complete overlay appears with time and player name
19. Player can tap [PLAY AGAIN] or [MAIN MENU]
```

---

## Shuffle Order System

- The shuffle order defines the **sequence** the ball must travel through cubes
- Generated fresh each game using Fisher-Yates shuffle
- Blue is always the **starting position** (index 0)
- Example orders:
  - Blue → Red → Green → Yellow (linear, easiest)
  - Blue → Green → Red → Yellow
  - Blue → Yellow → Red → Green (skipping physically requires intermediate rotations)

> **Design Note:** In Level 1, the cubes are physically in a chain (Blue-Red-Green-Yellow). "Non-adjacent" transfers in the shuffle order (e.g., Blue → Yellow) require the ball to pass through intermediate cubes. The shuffle order defines the ball's **final destination sequence**, and the player must navigate it by rotating intermediate cubes out of the way or aligning holes across multiple cubes.
>
> **Simplification for v1:** For Level 1 implementation, only **sequential adjacent transfers** are supported. Non-adjacent transfers are a Level 2+ feature.

---

## Cube Rotation Rules

1. Only **one cube** can be rotating at a time
2. A cube can be rotated by:
   - **Swiping** on the cube (up = +90°, down = −90°)
   - **Tapping arrow buttons** in HUD after selecting a cube
3. Rotation is **90° increments** only (snaps to 0°, 90°, 180°, 270°)
4. A cube **cannot be rotated** while:
   - The ball is in transit
   - The cube is locked (long press)
5. The **Blue cube (Position 1) cannot be rotated** — it is the anchor

---

## Hole Alignment Logic

- Each cube has holes on the **left and right faces** (X axis)
- Holes are aligned when both adjacent cubes have their hole axes **within ±5°** of each other in world space
- Alignment is checked every frame during rotation
- When holes align: `HoleAlignmentDetector` fires `OnHolesAligned`
- `BallTransferController` moves ball to the next cube in shuffle order
- Ball transfer is only valid if the **next cube in the shuffle order** is the cube being aligned with

---

## Timer

- Starts when player confirms selection
- Counts up (MM:SS format)
- Visible at all times during gameplay
- Pauses if app goes to background
- Stops on Level Complete

---

## Text Mode

- Toggle available in header bar at any time (even during gameplay)
- When ON:
  - Color dot labels show cube names (Blue, Red, Green, Yellow)
  - Cube face labels appear (world space or billboard text)
- When OFF:
  - Only colored shapes shown (for visual/spatial learners)

---

## Player Selector

- Up to 4 preset players + Guest
- Custom name input available (opens Android keyboard)
- Player name shown on Level Complete screen
- No scoring persistence in Level 1 (times are shown but not saved)

---

## Visual Style

- **Background**: deep dark blue (`#1A1A2E`) — space-like, calming
- **Cubes**: semi-transparent colored walls, visible interior
- **Ball**: bright white/pearl sphere
- **UI font**: clean, rounded sans-serif (e.g., Nunito or Roboto Rounded)
- **UI colors**: high contrast, AAA accessibility compliant
- **Animations**: smooth ease-in/ease-out, no sudden jumps

---

## Audio (Planned — Not in v1 Scope)

- Ball transfer: soft chime
- Cube rotation: light mechanical click
- Hole alignment: satisfying ding
- Level complete: celebratory jingle

> Audio is out of scope for Level 1 planning. Placeholder audio hooks should be included in scripts.

---

## Out of Scope for Level 1

- Level 2 or any additional levels
- Multiplayer
- Leaderboards / cloud save
- Physics-based ball rolling
- Bones, armatures, or skinned meshes
- Particle effects (may be added in polish pass)
- Sound effects (hooks planned but not implemented)
