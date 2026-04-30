# 05 — Unity Execution Model

## 1. Core Principle

LorQB moves in Unity are **fully deterministic and scripted**.

- No physics engine involvement
- No Rigidbody components
- No collision detection
- No forces or torques
- Every rotation, timing, and transfer is defined ahead of time in the Move Map and driven by controlled code

---

## 2. Execution Pipeline (Per Move)

Each move follows this exact sequence:

1. **Load move definition** — Read the move entry from the Move Map (move ID, hinge(s), rotation angles, stage count, transfer frame)
2. **Lock all pivots** — Freeze all hinge axes before any motion begins; only explicitly activated hinges may rotate
3. **Activate required hinge(s)** — Enable only the hinge(s) specified in the move definition (HBR, HRG, or HGY)
4. **Execute rotation sequence**
   - **Stage 1** — Rotate active hinge from start angle to target angle (or transfer angle for T-series)
   - **Stage 2** *(T-series only)* — After transfer, rotate second hinge from its start angle to final angle
5. **Monitor angle state** — Track current rotation against expected values at each time step
6. **Trigger transfer at exact frame/angle** — At the defined transfer point (see Time System), switch ball ownership from Seat_Source to Seat_Target (snap, no interpolation)
7. **Execute return sequence** — Rotate active hinge(s) back to resting angles
8. **Restore base orientation** — Confirm all cubes are back to canonical world positions before marking move complete

---

## 3. Time System

All move timing is expressed in **normalized time** `t`, ranging from `0.0` to `1.0`.

| Blender Frames | Normalized t |
|----------------|-------------|
| 240 frames     | Full move duration = t 0.0 → 1.0 |

### Anchor Points

| t value | Meaning |
|---------|---------|
| `0.0`   | Move start — hinges at resting angles, ball in Seat_Source |
| `0.5`   | Transfer point for **C-series** moves (frame 120 of 240) |
| `~0.67` | Transfer point for **T-series** moves (frame 161 of 240) |
| `1.0`   | Move end — all hinges returned, base orientation restored |

### Stage Boundaries (T-series)

| Stage | t range | Description |
|-------|---------|-------------|
| Stage 1 | `0.0 → ~0.67` | First hinge rotates to transfer angle |
| Transfer | `~0.67` | Ball snaps from Seat_Source to Seat_Target |
| Stage 2 | `~0.67 → 1.0` | Second hinge completes rotation; return begins |

---

## 4. Rotation System

- All motion is **pivot-based** — cubes rotate around defined hinge axes only
- **One hinge active at a time**, except during staged T-series transitions where Stage 1 ends and Stage 2 begins at the transfer point
- No transform parenting changes occur at runtime; parent-child relationships are fixed at scene load
- All rotations are performed via **controlled rotation functions** that accept:
  - Target pivot
  - Start angle
  - End angle
  - Normalized time `t`
- Rotation interpolation method: **linear** unless otherwise specified in the Move Map

---

## 5. Ball System

- The ball has **no physics**; it is a visual-only Transform child
- During motion, the ball follows its active Seat via Unity Transform hierarchy (position/rotation inherited automatically)
- **At transfer** (exact `t` value per move):
  - Ball is **unparented** from `Seat_Source`
  - Ball is **parented** to `Seat_Target`
  - This is an **instantaneous snap** — no lerp, no interpolation
- The ball's world position is implicitly correct because Seat_Target occupies the same world-space location as Seat_Source at the transfer moment (confirmed by Move Map geometry)

---

## 6. State Machine

Each move execution passes through the following states in order:

| State | Description |
|-------|-------------|
| `IDLE` | No move active; all hinges locked; ball resting in current Seat |
| `ROTATING_STAGE_1` | Active hinge(s) rotating; t advancing from 0.0 toward transfer point or 1.0 |
| `ROTATING_STAGE_2` | *(T-series only)* Second stage rotation after transfer; t advancing to 1.0 |
| `TRANSFER` | Instantaneous state: ball ownership switches from Seat_Source to Seat_Target |
| `RETURN` | Active hinge(s) rotating back to resting angles |
| `COMPLETE` | Move finished; base orientation confirmed; system returns to IDLE |

**Transitions:**

```
IDLE
  └─► ROTATING_STAGE_1
        ├─► TRANSFER (C-series: t = 0.5 | T-series: t ≈ 0.67)
        │     └─► ROTATING_STAGE_2 (T-series only)
        │           └─► RETURN
        │                 └─► COMPLETE → IDLE
        └─► RETURN (if no transfer in move)
              └─► COMPLETE → IDLE
```

---

## 7. Constraints

The following rules are **absolute** and must never be violated by any move execution:

| Constraint | Rule |
|------------|------|
| No detachment | Cubes are always connected; no cube ever separates from the structure |
| No base plane crossing | No cube may pass through or below the base plane at any point during a move |
| No overlap | No cube occupies the same space as another cube at any time step |
| Allowed hinges only | Only **HBR**, **HRG**, and **HGY** may be activated; no other rotation axes exist |

---

## Summary

| Property | Value |
|----------|-------|
| Motion type | Deterministic, scripted pivot rotation |
| Physics | None |
| Time unit | Normalized `t` (0.0 → 1.0), mapped from 240 Blender frames |
| Ball transfer | Instantaneous Transform re-parent at exact `t` |
| Active hinges | One at a time (except T-series stage boundary) |
| Valid hinges | HBR, HRG, HGY |
| State machine | IDLE → ROTATING_STAGE_1 → [TRANSFER] → [ROTATING_STAGE_2] → RETURN → COMPLETE |
